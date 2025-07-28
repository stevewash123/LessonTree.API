// **NEW FILE** - Services/EntityPositioningService.cs
// RESPONSIBILITY: Fix the positioning calculation bug
// DOES NOT: Create new DTOs or conversion methods
// CALLED BY: Controllers using existing DTOs

using LessonTree.DAL;
using LessonTree.DAL.Domain;  // ✅ ADD: For SubTopic, Topic domain classes
using LessonTree.Models;
using LessonTree.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace LessonTree.BLL.Service
{
    public class EntityPositioningService : IEntityPositioningService
    {
        private readonly LessonTreeContext _context;
        private readonly ILogger<EntityPositioningService> _logger;

        public EntityPositioningService(LessonTreeContext context, ILogger<EntityPositioningService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ✅ FIXED: Lesson positioning with correct calculation
        public async Task<EntityPositionResult> MoveLesson(LessonMoveResource request, int userId)
        {
            _logger.LogInformation("=== LESSON POSITIONING START ===");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var lesson = await _context.Lessons.FindAsync(request.LessonId);
                if (lesson == null || lesson.UserId != userId)
                {
                    return EntityPositionResult.Failure("Lesson not found or not owned by user");
                }

                var modifiedEntities = new List<EntityStateInfo>();
                int targetPosition;

                if (request.NewSubTopicId.HasValue)
                {
                    // Moving to SubTopic
                    targetPosition = await CalculateSubTopicPosition(request, userId);
                    lesson.SubTopicId = request.NewSubTopicId;
                    lesson.TopicId = null;
                    lesson.SortOrder = targetPosition;

                    var affected = await RenumberSubTopicLessons(request.NewSubTopicId.Value, request.LessonId, targetPosition, userId);
                    modifiedEntities.AddRange(affected);
                }
                else if (request.NewTopicId.HasValue)
                {
                    // ✅ FIXED: Moving to Topic - calculate position in mixed entity space
                    targetPosition = await CalculateTopicPosition(request, userId);
                    lesson.TopicId = request.NewTopicId;
                    lesson.SubTopicId = null;
                    lesson.SortOrder = targetPosition;

                    var affected = await RenumberTopicEntities(request.NewTopicId.Value, request.LessonId, targetPosition, userId);
                    modifiedEntities.AddRange(affected);
                }
                else
                {
                    return EntityPositionResult.Failure("Either NewSubTopicId or NewTopicId must be specified");
                }

                // Add the moved lesson to the result
                modifiedEntities.Add(new EntityStateInfo
                {
                    Id = lesson.Id,
                    Type = "Lesson",
                    Title = lesson.Title,
                    SortOrder = targetPosition,
                    TopicId = lesson.TopicId,
                    SubTopicId = lesson.SubTopicId,
                    IsMovedEntity = true
                });

                _context.Lessons.Update(lesson);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Lesson positioning completed successfully. Target sort order: {TargetPosition}", targetPosition);

                return EntityPositionResult.Success(modifiedEntities);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error during lesson positioning");
                return EntityPositionResult.Failure(ex.Message);
            }
        }

        // ✅ UPDATED: Return EntityPositionResult from MoveSubTopic  
        public async Task<EntityPositionResult> MoveSubTopic(SubTopicMoveResource request, int userId)
        {
            _logger.LogInformation("=== SUBTOPIC POSITIONING START ===");
            _logger.LogInformation("Request: SubTopicId={SubTopicId}, NewTopicId={TopicId}, RelativeToId={RelativeId}, RelativeToType={RelativeType}, Position={Position}",
                request.SubTopicId, request.NewTopicId, request.RelativeToId, request.RelativeToType, request.Position);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var subTopic = await _context.SubTopics.FindAsync(request.SubTopicId);
                if (subTopic == null)
                {
                    _logger.LogWarning("❌ SubTopic not found: SubTopicId={SubTopicId}", request.SubTopicId);
                    return EntityPositionResult.Failure("SubTopic not found");
                }

                if (subTopic.UserId != userId)
                {
                    _logger.LogWarning("❌ SubTopic not owned by user: SubTopicId={SubTopicId}, UserId={UserId}, OwnerUserId={OwnerUserId}",
                        request.SubTopicId, userId, subTopic.UserId);
                    return EntityPositionResult.Failure("SubTopic not owned by user");
                }

                _logger.LogInformation("✅ SubTopic found: Id={Id}, Title='{Title}', CurrentTopicId={CurrentTopicId}, CurrentSortOrder={CurrentSortOrder}",
                    subTopic.Id, subTopic.Title, subTopic.TopicId, subTopic.SortOrder);

                _logger.LogInformation("🔧 Calling CalculateTopicPositionForSubTopic...");
                var targetPosition = await CalculateTopicPositionForSubTopic(request, userId);
                _logger.LogInformation("✅ Calculated target position: {TargetPosition}", targetPosition);

                _logger.LogInformation("🔧 Updating SubTopic properties...");
                subTopic.TopicId = request.NewTopicId;
                subTopic.SortOrder = targetPosition;

                _logger.LogInformation("🔧 Calling RenumberTopicEntities...");
                var modifiedEntities = await RenumberTopicEntities(request.NewTopicId, request.SubTopicId, targetPosition, userId);
                _logger.LogInformation("✅ Renumbered {Count} other entities", modifiedEntities.Count);

                // Add the moved subtopic
                modifiedEntities.Add(new EntityStateInfo
                {
                    Id = subTopic.Id,
                    Type = "SubTopic",
                    Title = subTopic.Title,
                    SortOrder = targetPosition,
                    TopicId = subTopic.TopicId,
                    SubTopicId = null,
                    IsMovedEntity = true
                });

                _logger.LogInformation("🔧 Saving changes to database...");
                _context.SubTopics.Update(subTopic);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("=== SUBTOPIC POSITIONING SUCCESS ===");
                return EntityPositionResult.Success(modifiedEntities);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "❌ EXCEPTION in SubTopic positioning: Type={ExceptionType}, Message={Message}",
                    ex.GetType().Name, ex.Message);
                _logger.LogError("❌ Stack trace: {StackTrace}", ex.StackTrace);
                return EntityPositionResult.Failure($"Error: {ex.Message}");
            }
        }

        // ✅ UPDATED: Return EntityPositionResult from MoveTopic
        public async Task<EntityPositionResult> MoveTopic(TopicMoveResource request, int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var topic = await _context.Topics.FindAsync(request.TopicId);
                if (topic == null || topic.UserId != userId)
                {
                    return EntityPositionResult.Failure("Topic not found or not owned by user");
                }

                var targetPosition = await CalculateCourseTopicPosition(request, userId, topic.CourseId);
                topic.SortOrder = targetPosition;

                var modifiedEntities = await RenumberCourseTopics(topic.CourseId, request.TopicId, targetPosition, userId);

                // Add the moved topic
                modifiedEntities.Add(new EntityStateInfo
                {
                    Id = topic.Id,
                    Type = "Topic",
                    Title = topic.Title,
                    SortOrder = targetPosition,
                    TopicId = null,
                    SubTopicId = null,
                    IsMovedEntity = true
                });

                _context.Topics.Update(topic);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return EntityPositionResult.Success(modifiedEntities);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error during Topic positioning");
                return EntityPositionResult.Failure(ex.Message);
            }
        }

        #region Position Calculation - THE ACTUAL FIX

        // ✅ FIXED: Calculate position in mixed entity space (lessons + subtopics)
        private async Task<int> CalculateTopicPosition(LessonMoveResource request, int userId)
        {
            _logger.LogInformation("=== CalculateTopicPosition DEBUG START ===");
            _logger.LogInformation("Request: LessonId={LessonId}, NewTopicId={TopicId}, RelativeToId={RelativeId}, RelativeToType={RelativeType}, Position={Position}",
                request.LessonId, request.NewTopicId!.Value, request.RelativeToId, request.RelativeToType, request.Position);

            // ✅ FIX: Get all entities EXCLUDING the lesson being moved to get accurate relative positions
            var entities = await GetAllTopicEntitiesExcluding(request.NewTopicId!.Value, userId, request.LessonId, "Lesson");

            _logger.LogInformation("Found {Count} existing entities in topic {TopicId} (excluding moved lesson {LessonId})",
                entities.Count, request.NewTopicId!.Value, request.LessonId);

            // ✅ LOG ALL ENTITIES: See the exact entity list and sort orders
            for (int i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                _logger.LogInformation("  [{Index}] {Type} ID={Id} SortOrder={SortOrder}",
                    i, entity.Type, entity.Id, entity.SortOrder);
            }

            if (request.RelativeToId.HasValue && !string.IsNullOrEmpty(request.Position) && !string.IsNullOrEmpty(request.RelativeToType))
            {
                _logger.LogInformation("Looking for relative entity: Type='{Type}', Id={Id}",
                    request.RelativeToType, request.RelativeToId.Value);

                var relativeEntity = entities.FirstOrDefault(e => e.Id == request.RelativeToId.Value && e.Type == request.RelativeToType);
                if (relativeEntity != default)
                {
                    // ✅ THE FIX: Use index in the sorted list (excluding moved entity)
                    var relativeIndex = entities.IndexOf(relativeEntity);
                    var targetIndex = request.Position == "before" ? relativeIndex : relativeIndex + 1;

                    _logger.LogInformation("✅ FOUND: Relative entity '{Type}' id {Id} at index {RelativeIndex}",
                        request.RelativeToType, request.RelativeToId.Value, relativeIndex);
                    _logger.LogInformation("✅ CALCULATION: Position='{Position}' → targetIndex = {RelativeIndex} {Operator} = {TargetIndex}",
                        request.Position, relativeIndex, request.Position == "before" ? "+ 0" : "+ 1", targetIndex);
                    _logger.LogInformation("=== CalculateTopicPosition RESULT: {TargetIndex} ===", targetIndex);

                    return targetIndex;
                }
                else
                {
                    _logger.LogWarning("❌ NOT FOUND: Relative entity '{Type}' id {Id} not found in entity list",
                        request.RelativeToType, request.RelativeToId.Value);

                    // ✅ DEBUG: Show what entities we did find for comparison
                    var availableEntities = entities.Where(e => e.Type == request.RelativeToType).ToList();
                    _logger.LogWarning("Available {Type} entities: {EntityIds}",
                        request.RelativeToType, string.Join(", ", availableEntities.Select(e => e.Id)));
                }
            }
            else
            {
                _logger.LogInformation("No relative positioning specified, using fallback: append to end");
            }

            // Fallback: append to end
            var fallbackPosition = entities.Count;
            _logger.LogInformation("=== CalculateTopicPosition FALLBACK: {Position} ===", fallbackPosition);
            return fallbackPosition;
        }

        private async Task<int> CalculateSubTopicPosition(LessonMoveResource request, int userId)
        {
            var lessons = await _context.Lessons
                .Where(l => l.SubTopicId == request.NewSubTopicId && l.UserId == userId && !l.Archived)
                .OrderBy(l => l.SortOrder)
                .ToListAsync();

            if (request.RelativeToId.HasValue && !string.IsNullOrEmpty(request.Position))
            {
                var relativeLesson = lessons.FirstOrDefault(l => l.Id == request.RelativeToId.Value);
                if (relativeLesson != null)
                {
                    var relativeIndex = lessons.IndexOf(relativeLesson);
                    return request.Position == "before" ? relativeIndex : relativeIndex + 1;
                }
            }

            return lessons.Count;
        }

        private async Task<int> CalculateTopicPositionForSubTopic(SubTopicMoveResource request, int userId)
        {
            _logger.LogInformation("=== CalculateTopicPositionForSubTopic DEBUG START ===");
            _logger.LogInformation("Request: SubTopicId={SubTopicId}, NewTopicId={TopicId}, RelativeToId={RelativeId}, RelativeToType={RelativeType}, Position={Position}",
                request.SubTopicId, request.NewTopicId, request.RelativeToId, request.RelativeToType, request.Position);

            // ✅ FIX: Use the same exclusion logic as lesson positioning
            var entities = await GetAllTopicEntitiesExcluding(request.NewTopicId, userId, request.SubTopicId, "SubTopic");

            _logger.LogInformation("Found {Count} existing entities in topic {TopicId} (excluding moved SubTopic {SubTopicId})",
                entities.Count, request.NewTopicId, request.SubTopicId);

            // ✅ LOG ALL ENTITIES: See the exact entity list and sort orders
            for (int i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                _logger.LogInformation("  [{Index}] {Type} ID={Id} SortOrder={SortOrder}",
                    i, entity.Type, entity.Id, entity.SortOrder);
            }

            if (request.RelativeToId.HasValue && !string.IsNullOrEmpty(request.Position) && !string.IsNullOrEmpty(request.RelativeToType))
            {
                _logger.LogInformation("Looking for relative entity: Type='{Type}', Id={Id}",
                    request.RelativeToType, request.RelativeToId.Value);

                var relativeEntity = entities.FirstOrDefault(e => e.Id == request.RelativeToId.Value && e.Type == request.RelativeToType);
                if (relativeEntity != default)
                {
                    // ✅ THE FIX: Use index in the sorted list (excluding moved entity)
                    var relativeIndex = entities.IndexOf(relativeEntity);
                    var targetIndex = request.Position == "before" ? relativeIndex : relativeIndex + 1;

                    _logger.LogInformation("✅ FOUND: Relative entity '{Type}' id {Id} at index {RelativeIndex}",
                        request.RelativeToType, request.RelativeToId.Value, relativeIndex);
                    _logger.LogInformation("✅ CALCULATION: Position='{Position}' → targetIndex = {RelativeIndex} {Operator} = {TargetIndex}",
                        request.Position, relativeIndex, request.Position == "before" ? "+ 0" : "+ 1", targetIndex);
                    _logger.LogInformation("=== CalculateTopicPositionForSubTopic RESULT: {TargetIndex} ===", targetIndex);

                    return targetIndex;
                }
                else
                {
                    _logger.LogWarning("❌ NOT FOUND: Relative entity '{Type}' id {Id} not found in entity list",
                        request.RelativeToType, request.RelativeToId.Value);

                    // ✅ DEBUG: Show what entities we did find for comparison
                    var availableEntities = entities.Where(e => e.Type == request.RelativeToType).ToList();
                    _logger.LogWarning("Available {Type} entities: {EntityIds}",
                        request.RelativeToType, string.Join(", ", availableEntities.Select(e => e.Id)));
                }
            }
            else
            {
                _logger.LogInformation("No relative positioning specified, using fallback: append to end");
            }

            // Fallback: append to end
            var fallbackPosition = entities.Count;
            _logger.LogInformation("=== CalculateTopicPositionForSubTopic FALLBACK: {Position} ===", fallbackPosition);
            return fallbackPosition;
        }

        private async Task<int> CalculateCourseTopicPosition(TopicMoveResource request, int userId, int courseId)
        {
            var topics = await _context.Topics
                .Where(t => t.CourseId == courseId && t.UserId == userId && !t.Archived)
                .OrderBy(t => t.SortOrder)
                .ToListAsync();

            if (request.RelativeToId.HasValue && !string.IsNullOrEmpty(request.Position))
            {
                var relativeTopic = topics.FirstOrDefault(t => t.Id == request.RelativeToId.Value);
                if (relativeTopic != null)
                {
                    var relativeIndex = topics.IndexOf(relativeTopic);
                    return request.Position == "before" ? relativeIndex : relativeIndex + 1;
                }
            }

            return topics.Count;
        }

        // ✅ CORE METHOD: Get all entities in a topic for position calculation
        private async Task<List<(int Id, string Type, int SortOrder)>> GetAllTopicEntities(int topicId, int userId)
        {
            var entities = new List<(int Id, string Type, int SortOrder)>();

            // Direct topic lessons
            var lessons = await _context.Lessons
                .Where(l => l.TopicId == topicId && l.SubTopicId == null && l.UserId == userId && !l.Archived)
                .Select(l => new { l.Id, l.SortOrder })
                .ToListAsync();
            entities.AddRange(lessons.Select(l => (l.Id, "Lesson", l.SortOrder)));

            // SubTopics
            var subTopics = await _context.SubTopics
                .Where(st => st.TopicId == topicId && st.UserId == userId && !st.Archived)
                .Select(st => new { st.Id, st.SortOrder })
                .ToListAsync();
            entities.AddRange(subTopics.Select(st => (st.Id, "SubTopic", st.SortOrder)));

            return entities.OrderBy(e => e.SortOrder).ToList();
        }

        private async Task<List<(int Id, string Type, int SortOrder)>> GetAllTopicEntitiesExcluding(int topicId, int userId, int excludeId, string excludeType)
        {
            _logger.LogInformation("=== GetAllTopicEntitiesExcluding DEBUG ===");
            _logger.LogInformation("Parameters: TopicId={TopicId}, UserId={UserId}, ExcludeId={ExcludeId}, ExcludeType={ExcludeType}",
                topicId, userId, excludeId, excludeType);

            var entities = new List<(int Id, string Type, int SortOrder)>();

            // Direct topic lessons (exclude if moving a lesson)
            var lessonQuery = _context.Lessons
                .Where(l => l.TopicId == topicId && l.SubTopicId == null && l.UserId == userId && !l.Archived);

            if (excludeType == "Lesson")
            {
                lessonQuery = lessonQuery.Where(l => l.Id != excludeId);
                _logger.LogInformation("Excluding Lesson ID {ExcludeId} from query", excludeId);
            }

            var lessons = await lessonQuery
                .Select(l => new { l.Id, l.SortOrder, l.Title })
                .ToListAsync();

            _logger.LogInformation("Found {Count} lessons in topic {TopicId}:", lessons.Count, topicId);
            foreach (var lesson in lessons)
            {
                _logger.LogInformation("  Lesson: ID={Id}, Title='{Title}', SortOrder={SortOrder}",
                    lesson.Id, lesson.Title, lesson.SortOrder);
            }
            entities.AddRange(lessons.Select(l => (l.Id, "Lesson", l.SortOrder)));

            // SubTopics (exclude if moving a subtopic)
            var subTopicQuery = _context.SubTopics
                .Where(st => st.TopicId == topicId && st.UserId == userId && !st.Archived);

            if (excludeType == "SubTopic")
            {
                subTopicQuery = subTopicQuery.Where(st => st.Id != excludeId);
                _logger.LogInformation("Excluding SubTopic ID {ExcludeId} from query", excludeId);
            }

            var subTopics = await subTopicQuery
                .Select(st => new { st.Id, st.SortOrder, st.Title })
                .ToListAsync();

            _logger.LogInformation("Found {Count} subtopics in topic {TopicId}:", subTopics.Count, topicId);
            foreach (var subTopic in subTopics)
            {
                _logger.LogInformation("  SubTopic: ID={Id}, Title='{Title}', SortOrder={SortOrder}",
                    subTopic.Id, subTopic.Title, subTopic.SortOrder);
            }
            entities.AddRange(subTopics.Select(st => (st.Id, "SubTopic", st.SortOrder)));

            var sortedEntities = entities.OrderBy(e => e.SortOrder).ToList();

            _logger.LogInformation("=== FINAL SORTED ENTITY LIST ===");
            for (int i = 0; i < sortedEntities.Count; i++)
            {
                var entity = sortedEntities[i];
                _logger.LogInformation("  [{Index}] {Type} ID={Id} SortOrder={SortOrder}",
                    i, entity.Type, entity.Id, entity.SortOrder);
            }
            _logger.LogInformation("=== END ENTITY LIST ===");

            return sortedEntities;
        }

        #endregion

        #region Renumbering Logic

        private async Task<List<EntityStateInfo>> RenumberTopicEntities(int topicId, int movedEntityId, int targetPosition, int userId)
        {
            var modifiedEntities = new List<EntityStateInfo>();
            var entities = await GetAllTopicEntities(topicId, userId);
            var otherEntities = entities.Where(e => e.Id != movedEntityId).ToList();

            int assignedPosition = 0;
            foreach (var entity in otherEntities)
            {
                if (assignedPosition == targetPosition)
                {
                    assignedPosition++; // Skip target position
                }

                if (entity.Type == "Lesson")
                {
                    var lesson = await _context.Lessons.FindAsync(entity.Id);
                    if (lesson != null && lesson.SortOrder != assignedPosition)
                    {
                        lesson.SortOrder = assignedPosition;
                        _context.Lessons.Update(lesson);

                        modifiedEntities.Add(new EntityStateInfo
                        {
                            Id = lesson.Id,
                            Type = "Lesson",
                            Title = lesson.Title,
                            SortOrder = assignedPosition,
                            TopicId = lesson.TopicId,
                            SubTopicId = lesson.SubTopicId,
                            IsMovedEntity = false
                        });
                    }
                }
                else if (entity.Type == "SubTopic")
                {
                    var subTopic = await _context.SubTopics.FindAsync(entity.Id);
                    if (subTopic != null && subTopic.SortOrder != assignedPosition)
                    {
                        subTopic.SortOrder = assignedPosition;
                        _context.SubTopics.Update(subTopic);

                        modifiedEntities.Add(new EntityStateInfo
                        {
                            Id = subTopic.Id,
                            Type = "SubTopic",
                            Title = subTopic.Title,
                            SortOrder = assignedPosition,
                            TopicId = subTopic.TopicId,
                            SubTopicId = null,
                            IsMovedEntity = false
                        });
                    }
                }

                assignedPosition++;
            }

            return modifiedEntities;
        }

        private async Task<List<EntityStateInfo>> RenumberSubTopicLessons(int subTopicId, int movedLessonId, int targetPosition, int userId)
        {
            var modifiedEntities = new List<EntityStateInfo>();
            var lessons = await _context.Lessons
                .Where(l => l.SubTopicId == subTopicId && l.Id != movedLessonId && l.UserId == userId && !l.Archived)
                .OrderBy(l => l.SortOrder)
                .ToListAsync();

            int assignedPosition = 0;
            foreach (var lesson in lessons)
            {
                if (assignedPosition == targetPosition)
                {
                    assignedPosition++; // Skip target position
                }

                if (lesson.SortOrder != assignedPosition)
                {
                    lesson.SortOrder = assignedPosition;
                    _context.Lessons.Update(lesson);

                    modifiedEntities.Add(new EntityStateInfo
                    {
                        Id = lesson.Id,
                        Type = "Lesson",
                        Title = lesson.Title,
                        SortOrder = assignedPosition,
                        TopicId = lesson.TopicId,
                        SubTopicId = lesson.SubTopicId,
                        IsMovedEntity = false
                    });
                }

                assignedPosition++;
            }

            return modifiedEntities;
        }

        private async Task<List<EntityStateInfo>> RenumberCourseTopics(int courseId, int movedTopicId, int targetPosition, int userId)
        {
            var modifiedEntities = new List<EntityStateInfo>();
            var topics = await _context.Topics
                .Where(t => t.CourseId == courseId && t.Id != movedTopicId && t.UserId == userId && !t.Archived)
                .OrderBy(t => t.SortOrder)
                .ToListAsync();

            int assignedPosition = 0;
            foreach (var topic in topics)
            {
                if (assignedPosition == targetPosition)
                {
                    assignedPosition++; // Skip target position
                }

                if (topic.SortOrder != assignedPosition)
                {
                    topic.SortOrder = assignedPosition;
                    _context.Topics.Update(topic);

                    modifiedEntities.Add(new EntityStateInfo
                    {
                        Id = topic.Id,
                        Type = "Topic",
                        Title = topic.Title,
                        SortOrder = assignedPosition,
                        TopicId = null,
                        SubTopicId = null,
                        IsMovedEntity = false
                    });
                }

                assignedPosition++;
            }

            return modifiedEntities;
        }

        #endregion
    }
}
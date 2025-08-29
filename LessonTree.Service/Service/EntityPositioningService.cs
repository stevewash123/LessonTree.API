// **ENHANCED FILE** - Services/EntityPositioningService.cs
// RESPONSIBILITY: Entity positioning + Schedule event repositioning integration
// INTEGRATION: Add schedule service dependencies and lesson move schedule updates

using LessonTree.DAL;
using LessonTree.DAL.Domain;
using LessonTree.Models;
using LessonTree.Models.DTO;
using LessonTree.BLL.Services; // Add for IScheduleService
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace LessonTree.BLL.Service
{
    public class EntityPositioningService : IEntityPositioningService
    {
        private readonly LessonTreeContext _context;
        private readonly ILogger<EntityPositioningService> _logger;
        private readonly IScheduleService _scheduleService; // ✅ NEW: Add schedule service
        private readonly IScheduleGenerationService _scheduleGenerationService; // ✅ NEW: Add for repositioning logic

        public EntityPositioningService(
            LessonTreeContext context,
            ILogger<EntityPositioningService> logger,
            IScheduleService scheduleService, // ✅ NEW: Inject schedule service
            IScheduleGenerationService scheduleGenerationService) // ✅ NEW: Inject generation service
        {
            _context = context;
            _logger = logger;
            _scheduleService = scheduleService;
            _scheduleGenerationService = scheduleGenerationService;
        }

        // ✅ ENHANCED: Lesson positioning with schedule event repositioning
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

                // ✅ STEP 1: Determine which course this lesson belongs to (for schedule updates)
                var courseId = await GetCourseIdForLessonAsync(request.LessonId);
                if (!courseId.HasValue)
                {
                    _logger.LogWarning("Could not determine course for lesson {LessonId}", request.LessonId);
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
                    // Moving to Topic - calculate position in mixed entity space
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

                // ✅ STEP 2: Update lesson hierarchy (existing logic)
                _context.Lessons.Update(lesson);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Lesson positioning completed successfully. Target sort order: {TargetPosition}", targetPosition);

                // ✅ STEP 3: Update schedule events (NEW FUNCTIONALITY)
                if (courseId.HasValue)
                {
                    try
                    {
                        await UpdateScheduleEventsForLessonMove(courseId.Value, request.LessonId, targetPosition, userId);
                        _logger.LogInformation("Schedule events updated successfully for lesson {LessonId} move", request.LessonId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to update schedule events for lesson {LessonId} move - lesson positioning succeeded but calendar may be out of sync", request.LessonId);
                    }
                }

                return EntityPositionResult.Success(modifiedEntities);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error during lesson positioning");
                return EntityPositionResult.Failure(ex.Message);
            }
        }

        private async Task UpdateScheduleEventsForLessonMove(int courseId, int lessonId, int newSortOrder, int userId)
        {
            _logger.LogInformation("=== SCHEDULE EVENT REPOSITIONING START ===");
            _logger.LogInformation("Updating schedule events for lesson move - CourseId: {CourseId}, LessonId: {LessonId}, NewSortOrder: {NewSortOrder}",
                courseId, lessonId, newSortOrder);

            // Find all schedules that contain this course
            var affectedSchedules = await _scheduleService.FindAllSchedulesByCourseIdAsync(courseId, userId);

            if (!affectedSchedules.Any())
            {
                _logger.LogInformation("No schedules found containing course {CourseId} - no schedule event updates needed", courseId);
                return;
            }

            _logger.LogInformation("Found {ScheduleCount} schedules containing course {CourseId}", affectedSchedules.Count, courseId);

            // Update each affected schedule
            foreach (var schedule in affectedSchedules)
            {
                try
                {
                    await UpdateScheduleEventsForLessonMoveInSchedule(schedule, courseId, lessonId, newSortOrder, userId);
                    _logger.LogInformation("Updated schedule events in schedule {ScheduleId} for lesson {LessonId} move", schedule.Id, lessonId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update schedule events in schedule {ScheduleId} for lesson {LessonId} move", schedule.Id, lessonId);
                    // Continue with other schedules - don't fail entire operation
                }
            }

            _logger.LogInformation("=== SCHEDULE EVENT REPOSITIONING COMPLETE ===");
        }

        // ✅ NEW METHOD: Update schedule events for lesson move in specific schedule
        private async Task UpdateScheduleEventsForLessonMoveInSchedule(ScheduleResource schedule, int courseId, int lessonId, int newSortOrder, int userId)
        {
            _logger.LogInformation("Updating schedule {ScheduleId} for lesson {LessonId} move to sort order {NewSortOrder}",
                schedule.Id, lessonId, newSortOrder);

            // Get the current lesson sequence for this course
            var lessons = await GetLessonsForCourseInSequenceOrder(courseId, userId);

            if (!lessons.Any())
            {
                _logger.LogWarning("No lessons found for course {CourseId} - cannot update schedule events", courseId);
                return;
            }

            // Find which periods in this schedule use this course
            var periodsUsingCourse = schedule.ScheduleConfiguration?.PeriodAssignments
                ?.Where(pa => pa.CourseId == courseId)
                ?.Select(pa => pa.Period)
                ?.ToList() ?? new List<int>();

            if (!periodsUsingCourse.Any())
            {
                _logger.LogInformation("Schedule {ScheduleId} does not use course {CourseId} in any periods - no updates needed", schedule.Id, courseId);
                return;
            }

            _logger.LogInformation("Schedule {ScheduleId} uses course {CourseId} in periods: {Periods}",
                schedule.Id, courseId, string.Join(", ", periodsUsingCourse));

            // Update schedule events for each period that uses this course
            var updatedEvents = new List<ScheduleEventResource>(schedule.ScheduleEvents);
            var eventsModified = false;

            foreach (var period in periodsUsingCourse)
            {
                var periodEventsModified = await UpdateScheduleEventsForPeriod(updatedEvents, period, courseId, lessons);
                if (periodEventsModified)
                {
                    eventsModified = true;
                    _logger.LogInformation("Updated schedule events for period {Period} in schedule {ScheduleId}", period, schedule.Id);
                }
            }

            // Save updated schedule if any events were modified
            if (eventsModified)
            {
                await _scheduleService.UpdateScheduleEventsAsync(schedule.Id, updatedEvents, userId);
                _logger.LogInformation("Saved updated schedule events for schedule {ScheduleId}", schedule.Id);
            }
            else
            {
                _logger.LogInformation("No schedule event updates needed for schedule {ScheduleId}", schedule.Id);
            }
        }

        // ✅ NEW METHOD: Update schedule events for specific period with new lesson sequence
        private async Task<bool> UpdateScheduleEventsForPeriod(List<ScheduleEventResource> scheduleEvents, int period, int courseId, List<Lesson> lessons)
        {
            var eventsModified = false;

            // Get all lesson events for this period/course, ordered by date
            var periodLessonEvents = scheduleEvents
                .Where(e => e.Period == period && e.CourseId == courseId && e.EventType == "Lesson")
                .OrderBy(e => e.Date)
                .ToList();

            if (!periodLessonEvents.Any())
            {
                _logger.LogInformation("No lesson events found for period {Period}, course {CourseId}", period, courseId);
                return false;
            }

            _logger.LogInformation("Found {EventCount} lesson events for period {Period}, course {CourseId}",
                periodLessonEvents.Count, period, courseId);

            // Reassign lessons to events based on new sequence order
            for (int i = 0; i < periodLessonEvents.Count; i++)
            {
                var scheduleEvent = periodLessonEvents[i];
                var targetLesson = i < lessons.Count ? lessons[i] : null;

                if (targetLesson != null)
                {
                    // Update event to point to correct lesson in sequence
                    var oldLessonId = scheduleEvent.LessonId;
                    if (scheduleEvent.LessonId != targetLesson.Id)
                    {
                        scheduleEvent.LessonId = targetLesson.Id;
                        scheduleEvent.LessonTitle = targetLesson.Title;
                        scheduleEvent.LessonObjective = targetLesson.Objective;
                        scheduleEvent.ScheduleSort = i; // Update sequence position
                        eventsModified = true;

                        _logger.LogDebug("Updated event {Date:yyyy-MM-dd} period {Period}: LessonId {OldId} → {NewId} ('{Title}')",
                            scheduleEvent.Date, period, oldLessonId, targetLesson.Id, targetLesson.Title);
                    }
                }
                else
                {
                    // No more lessons available - convert to error event
                    if (scheduleEvent.EventType == "Lesson")
                    {
                        scheduleEvent.LessonId = null;
                        scheduleEvent.LessonTitle = null;
                        scheduleEvent.LessonObjective = null;
                        scheduleEvent.EventType = "Error";
                        scheduleEvent.EventCategory = null;
                        scheduleEvent.Comment = "No lesson assigned - schedule needs more content";
                        eventsModified = true;

                        _logger.LogDebug("Converted event {Date:yyyy-MM-dd} period {Period} to error - no more lessons available",
                            scheduleEvent.Date, period);
                    }
                }
            }

            return eventsModified;
        }

        // ✅ NEW METHOD: Get lessons for course in proper sequence order
        private async Task<List<Lesson>> GetLessonsForCourseInSequenceOrder(int courseId, int userId)
        {
            var allUserLessons = await _context.Lessons
                .Where(l => l.UserId == userId && !l.Archived)
                .Include(l => l.Topic)
                .Include(l => l.SubTopic).ThenInclude(st => st.Topic)
                .OrderBy(l => l.Topic != null ? l.Topic.SortOrder : l.SubTopic.Topic.SortOrder) // Primary: Topic sort order
                .ThenBy(l => l.SubTopicId.HasValue ? l.SubTopic.SortOrder : 999) // Secondary: SubTopics by their sort order, direct lessons last
                .ThenBy(l => l.SortOrder) // Final: Lesson sort order within container
                .ToListAsync();

            // Filter to only lessons that belong to this course
            var courseLessons = allUserLessons.Where(l => BelongsToCourse(l, courseId)).ToList();

            _logger.LogInformation("Found {LessonCount} lessons for course {CourseId} in sequence order", courseLessons.Count, courseId);

            return courseLessons;
        }

        // ✅ NEW METHOD: Check if lesson belongs to specific course
        private bool BelongsToCourse(Lesson lesson, int courseId)
        {
            // Check if lesson belongs directly to a Topic in this Course
            if (lesson.TopicId.HasValue && lesson.Topic?.CourseId == courseId)
            {
                return true;
            }

            // Check if lesson belongs to a SubTopic whose Topic is in this Course  
            if (lesson.SubTopicId.HasValue && lesson.SubTopic?.Topic?.CourseId == courseId)
            {
                return true;
            }

            return false;
        }

        // ✅ NEW METHOD: Get course ID for a lesson through hierarchy
        private async Task<int?> GetCourseIdForLessonAsync(int lessonId)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Topic)
                .Include(l => l.SubTopic).ThenInclude(st => st.Topic)
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson?.TopicId.HasValue == true)
            {
                // Direct topic assignment: Lesson → Topic → Course
                return lesson.Topic?.CourseId;
            }
            else if (lesson?.SubTopicId.HasValue == true)
            {
                // SubTopic assignment: Lesson → SubTopic → Topic → Course
                return lesson.SubTopic?.Topic?.CourseId;
            }

            return null;
        }

        // ✅ EXISTING METHODS: SubTopic and Topic positioning (unchanged)
        public async Task<EntityPositionResult> MoveSubTopic(SubTopicMoveResource request, int userId)
        {
            _logger.LogInformation("=== SUBTOPIC POSITIONING START ===");
            _logger.LogInformation("Request: SubTopicId={SubTopicId}, NewTopicId={TopicId}, AfterSiblingId={AfterSiblingId}",
                request.SubTopicId, request.NewTopicId, request.AfterSiblingId);

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

        public async Task<EntityPositionResult> MoveTopic(TopicMoveResource request, int userId)
        {
            // ... existing implementation unchanged ...
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

        // ✅ EXISTING METHODS: All position calculation and renumbering logic (unchanged)
        #region Position Calculation - THE ACTUAL FIX

        private async Task<int> CalculateTopicPosition(LessonMoveResource request, int userId)
        {
            _logger.LogInformation("=== CalculateTopicPosition DEBUG START ===");
            _logger.LogInformation("Request: LessonId={LessonId}, NewTopicId={TopicId}, AfterSiblingId={AfterSiblingId}",
                request.LessonId, request.NewTopicId!.Value, request.AfterSiblingId);

            var entities = await GetAllTopicEntitiesExcluding(request.NewTopicId!.Value, userId, request.LessonId, "Lesson");

            _logger.LogInformation("Found {Count} existing entities in topic {TopicId} (excluding moved lesson {LessonId})",
                entities.Count, request.NewTopicId!.Value, request.LessonId);

            // ✅ UPDATED: Check for AfterSiblingId instead of RelativeToEntityId
            if (request.AfterSiblingId.HasValue)
            {
                _logger.LogInformation("Looking for sibling entity with Id={Id}", request.AfterSiblingId.Value);

                // Find the sibling (could be Lesson or SubTopic)
                var siblingEntity = entities.FirstOrDefault(e => e.Id == request.AfterSiblingId.Value);
                if (siblingEntity != default)
                {
                    var siblingIndex = entities.IndexOf(siblingEntity);
                    var targetIndex = siblingIndex + 1; // Always AFTER the sibling

                    _logger.LogInformation("✅ FOUND: Sibling entity id {Id} at index {SiblingIndex}",
                        request.AfterSiblingId.Value, siblingIndex);
                    _logger.LogInformation("✅ CALCULATION: Position AFTER sibling → targetIndex = {SiblingIndex} + 1 = {TargetIndex}",
                        siblingIndex, targetIndex);

                    return targetIndex;
                }
                else
                {
                    _logger.LogWarning("❌ NOT FOUND: Sibling entity id {Id} not found in entity list", request.AfterSiblingId.Value);
                    throw new ArgumentException($"Sibling entity {request.AfterSiblingId.Value} not found in target topic");
                }
            }
            else
            {
                // ✅ EMPTY CONTAINER: Position at beginning
                _logger.LogInformation("✅ EMPTY CONTAINER: No AfterSiblingId specified, positioning at start (index 0)");
                return 0;
            }
        }


        private async Task<int> CalculateSubTopicPosition(LessonMoveResource request, int userId)
        {
            var lessons = await _context.Lessons
                .Where(l => l.SubTopicId == request.NewSubTopicId && l.UserId == userId && !l.Archived)
                .OrderBy(l => l.SortOrder)
                .ToListAsync();

            // ✅ UPDATED: Check for AfterSiblingId instead of RelativeToEntityId
            if (request.AfterSiblingId.HasValue)
            {
                var siblingLesson = lessons.FirstOrDefault(l => l.Id == request.AfterSiblingId.Value);
                if (siblingLesson != null)
                {
                    var siblingIndex = lessons.IndexOf(siblingLesson);
                    return siblingIndex + 1; // Always AFTER the sibling
                }

                throw new ArgumentException($"Sibling lesson {request.AfterSiblingId.Value} not found in target subtopic");
            }
            else
            {
                // ✅ EMPTY CONTAINER: Position at beginning
                return 0;
            }
        }


        private async Task<int> CalculateTopicPositionForSubTopic(SubTopicMoveResource request, int userId)
        {
            _logger.LogInformation("=== CalculateTopicPositionForSubTopic DEBUG START ===");
            _logger.LogInformation("Request: SubTopicId={SubTopicId}, NewTopicId={TopicId}, AfterSiblingId={AfterSiblingId}",
                request.SubTopicId, request.NewTopicId, request.AfterSiblingId);

            var entities = await GetAllTopicEntitiesExcluding(request.NewTopicId, userId, request.SubTopicId, "SubTopic");

            // ✅ UPDATED: Check for AfterSiblingId instead of AfterSiblingId
            if (request.AfterSiblingId.HasValue)
            {
                _logger.LogInformation("Looking for sibling entity with Id={Id}", request.AfterSiblingId.Value);

                var siblingEntity = entities.FirstOrDefault(e => e.Id == request.AfterSiblingId.Value);
                if (siblingEntity != default)
                {
                    var siblingIndex = entities.IndexOf(siblingEntity);
                    var targetIndex = siblingIndex + 1; // Always AFTER the sibling

                    _logger.LogInformation("✅ FOUND: Sibling entity id {Id} at index {SiblingIndex}",
                        request.AfterSiblingId.Value, siblingIndex);
                    _logger.LogInformation("✅ CALCULATION: Position AFTER sibling → targetIndex = {TargetIndex}",
                        targetIndex);

                    return targetIndex;
                }
                else
                {
                    _logger.LogWarning("❌ NOT FOUND: Sibling entity id {Id} not found in entity list", request.AfterSiblingId.Value);
                    throw new ArgumentException($"Sibling entity {request.AfterSiblingId.Value} not found in target topic");
                }
            }
            else
            {
                // ✅ EMPTY CONTAINER: Position at beginning
                _logger.LogInformation("✅ EMPTY CONTAINER: No AfterSiblingId specified, positioning at start (index 0)");
                return 0;
            }
        }


        private async Task<int> CalculateCourseTopicPosition(TopicMoveResource request, int userId, int courseId)
        {
            var topics = await _context.Topics
                .Where(t => t.CourseId == courseId && t.UserId == userId && !t.Archived)
                .OrderBy(t => t.SortOrder)
                .ToListAsync();

            // ✅ UPDATED: Check for AfterSiblingId instead of AfterSiblingId
            if (request.AfterSiblingId.HasValue)
            {
                var siblingTopic = topics.FirstOrDefault(t => t.Id == request.AfterSiblingId.Value);
                if (siblingTopic != null)
                {
                    var siblingIndex = topics.IndexOf(siblingTopic);
                    return siblingIndex + 1; // Always AFTER the sibling
                }

                throw new ArgumentException($"Sibling topic {request.AfterSiblingId.Value} not found in target course");
            }
            else
            {
                // ✅ EMPTY CONTAINER: Position at beginning  
                return 0;
            }
        }

        private async Task<List<(int Id, string Type, int SortOrder)>> GetAllTopicEntities(int topicId, int userId)
        {
            var entities = new List<(int Id, string Type, int SortOrder)>();

            var lessons = await _context.Lessons
                .Where(l => l.TopicId == topicId && l.SubTopicId == null && l.UserId == userId && !l.Archived)
                .Select(l => new { l.Id, l.SortOrder })
                .ToListAsync();
            entities.AddRange(lessons.Select(l => (l.Id, "Lesson", l.SortOrder)));

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
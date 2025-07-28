// **COMPLETE FILE** - LessonRepository.cs - Standardized to enterprise patterns
// RESPONSIBILITY: Lesson data access with attachment relationships and filtering
// DOES NOT: Handle lesson content validation or attachment file management (that's in services)
// CALLED BY: LessonService for all lesson operations

using LessonTree.DAL;
using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
using LessonTree.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class LessonRepository : ILessonRepository
{
    private readonly LessonTreeContext _context;
    private readonly ILogger<LessonRepository> _logger;

    public LessonRepository(LessonTreeContext context, ILogger<LessonRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public IQueryable<Lesson> GetAll(Func<IQueryable<Lesson>, IQueryable<Lesson>> include = null)
    {
        _logger.LogInformation("GetAll: Retrieving all lessons");

        var query = _context.Lessons.AsQueryable();
        if (include != null)
        {
            query = include(query);
        }
        return query;
    }

    public async Task<Lesson?> GetByIdAsync(int id, Func<IQueryable<Lesson>, IQueryable<Lesson>> include = null)
    {
        _logger.LogInformation($"GetByIdAsync: Fetching lesson {id}");

        IQueryable<Lesson> query = _context.Lessons;
        if (include != null)
        {
            query = include(query);
        }
        else
        {
            query = query
                .Include(l => l.LessonAttachments).ThenInclude(ld => ld.Attachment)
                .Include(l => l.SubTopic)
                .Include(l => l.Topic)
                .Include(l => l.User)
                .Include(l => l.LessonStandards).ThenInclude(ls => ls.Standard);
        }

        var lesson = await query.FirstOrDefaultAsync(l => l.Id == id);

        if (lesson != null)
        {
            _logger.LogInformation($"GetByIdAsync: Found lesson {id} for user {lesson.UserId}");
        }
        else
        {
            _logger.LogInformation($"GetByIdAsync: Lesson {id} not found");
        }

        return lesson;
    }

    public async Task<int> AddAsync(Lesson lesson)
    {
        _logger.LogInformation($"AddAsync: Creating lesson '{lesson.Title}' for user {lesson.UserId}");

        _context.Lessons.Add(lesson);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"AddAsync: Created lesson {lesson.Id} for user {lesson.UserId}");
        return lesson.Id;
    }

    public async Task UpdateAsync(Lesson lesson)
    {
        _logger.LogInformation($"UpdateAsync: Updating lesson {lesson.Id}");

        _context.Lessons.Update(lesson);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"UpdateAsync: Updated lesson {lesson.Id}");
    }

    public async Task DeleteAsync(int id)
    {
        _logger.LogInformation($"DeleteAsync: Deleting lesson {id}");

        var lesson = await _context.Lessons.FindAsync(id);
        if (lesson == null)
        {
            throw new ArgumentException($"Lesson {id} not found");
        }

        _context.Lessons.Remove(lesson);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"DeleteAsync: Deleted lesson {id}");
    }


    public async Task AddAttachmentAsync(int lessonId, int attachmentId)
    {
        _logger.LogDebug("Adding attachment ID: {AttachmentId} to Lesson ID: {LessonId}", attachmentId, lessonId);
        var lesson = await GetByIdAsync(lessonId);
        if (lesson == null)
        {
            _logger.LogError("Lesson with ID {LessonId} not found for attachment addition", lessonId);
            throw new ArgumentException("Lesson not found");
        }
        var attachment = await _context.Attachments.FindAsync(attachmentId);
        if (attachment == null)
        {
            _logger.LogError("Attachment with ID {AttachmentId} not found", attachmentId);
            throw new ArgumentException("Attachment not found");
        }
        var lessonAttachment = new LessonAttachment { LessonId = lessonId, AttachmentId = attachmentId };
        _context.LessonAttachments.Add(lessonAttachment);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Added attachment ID: {AttachmentId} to lesson with ID: {LessonId}", attachmentId, lessonId);
    }

    public async Task RemoveAttachmentAsync(int lessonId, int attachmentId)
    {
        _logger.LogDebug("Removing attachment ID: {AttachmentId} from Lesson ID: {LessonId}", attachmentId, lessonId);
        var lessonAttachment = await _context.LessonAttachments
            .FirstOrDefaultAsync(ld => ld.LessonId == lessonId && ld.AttachmentId == attachmentId);
        if (lessonAttachment == null)
        {
            _logger.LogError("Attachment with ID {AttachmentId} not found in lesson with ID {LessonId}", attachmentId, lessonId);
            throw new ArgumentException("Attachment not found in lesson");
        }
        _context.LessonAttachments.Remove(lessonAttachment);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Removed attachment ID: {AttachmentId} from lesson with ID: {LessonId}", attachmentId, lessonId);
    }

    public IQueryable<Lesson> GetByTitle(string title)
    {
        _logger.LogDebug("Retrieving lessons by title: {Title}", title);
        var lessons = _context.Lessons
            .Include(l => l.LessonAttachments).ThenInclude(ld => ld.Attachment)
            .Include(l => l.SubTopic)
            .Include(l => l.Topic) // New
            .Where(l => l.Title.Contains(title));
        return lessons;
    }

    // New Methods
    public IQueryable<Lesson> GetByTopicId(int topicId, bool includeArchived = false)
    {
        _logger.LogDebug("Retrieving lessons by Topic ID: {TopicId}", topicId);
        var query = _context.Lessons
            .Where(l => l.TopicId == topicId);
        if (!includeArchived)
            query = query.Where(l => !l.Archived);
        return query;
    }

    public IQueryable<Lesson> GetBySubTopicId(int subTopicId, bool includeArchived = false)
    {
        _logger.LogDebug("Retrieving lessons by SubTopic ID: {SubTopicId}", subTopicId);
        var query = _context.Lessons
            .Where(l => l.SubTopicId == subTopicId);
        if (!includeArchived)
            query = query.Where(l => !l.Archived);
        return query;
    }

    public IQueryable<Lesson> GetByUserId(int userId, bool includeArchived = false)
    {
        _logger.LogDebug("Retrieving lessons by User ID: {UserId}", userId);
        var query = _context.Lessons
            .Where(l => l.UserId == userId);
        if (!includeArchived)
            query = query.Where(l => !l.Archived);
        return query;
    }

    // **PARTIAL FILE** - Add this method to LessonRepository.cs
    // Insert after the existing methods

    // **PARTIAL FILE** - Add this method to LessonRepository.cs
    // Insert after the existing methods

    // **PARTIAL FILE** - LessonRepository.cs - Replace MoveLessonToPositionAsync method
    // INTEGRATION: Replace the existing MoveLessonToPositionAsync method with this enhanced version

    private async Task RenumberAllTopicEntitiesAsync(int topicId, int movedLessonId, int targetSortOrder, int userId)
    {
        _logger.LogInformation("RenumberAllTopicEntitiesAsync: Renumbering all entities in Topic {TopicId}, moved lesson {MovedLessonId} to position {TargetSortOrder}",
            topicId, movedLessonId, targetSortOrder);

        // Get all entities in the topic
        var allEntities = await GetAllTopicEntitiesAsync(topicId, userId);

        // Remove the moved lesson from the list (we'll handle it separately)
        var otherEntities = allEntities.Where(e => !(e.Type == "Lesson" && e.Id == movedLessonId)).ToList();

        _logger.LogInformation("RenumberAllTopicEntitiesAsync: Found {EntityCount} other entities to renumber", otherEntities.Count);

        // Create new clean sequence with moved lesson slot reserved
        int assignedSortOrder = 0;

        foreach (var entity in otherEntities.OrderBy(e => e.SortOrder))
        {
            // Reserve slot for moved lesson
            if (assignedSortOrder == targetSortOrder)
            {
                assignedSortOrder++; // Skip the target position
                _logger.LogInformation("RenumberAllTopicEntitiesAsync: Reserved slot {TargetSortOrder} for moved lesson", targetSortOrder);
            }

            // Only update if sort order actually changes
            if (entity.SortOrder != assignedSortOrder)
            {
                _logger.LogInformation("RenumberAllTopicEntitiesAsync: Updating {EntityType} {EntityId} from {OldSort} to {NewSort}",
                    entity.Type, entity.Id, entity.SortOrder, assignedSortOrder);

                if (entity.Type == "Lesson")
                {
                    var lesson = await _context.Lessons.FindAsync(entity.Id);
                    if (lesson != null)
                    {
                        lesson.SortOrder = assignedSortOrder;
                        _context.Lessons.Update(lesson);
                    }
                }
                else if (entity.Type == "SubTopic")
                {
                    var subTopic = await _context.SubTopics.FindAsync(entity.Id);
                    if (subTopic != null)
                    {
                        subTopic.SortOrder = assignedSortOrder;
                        _context.SubTopics.Update(subTopic);
                    }
                }
            }

            assignedSortOrder++;
        }

        _logger.LogInformation("RenumberAllTopicEntitiesAsync: Renumbering complete, moved lesson will have position {TargetSortOrder}", targetSortOrder);
    }

    // ✅ UPDATED: Use new renumbering method for Topic moves
    public async Task MoveLessonToPositionAsync(LessonMoveResource moveResource, int userId)
    {
        _logger.LogInformation("=== REPOSITORY POSITION MOVE DIAGNOSTICS START ===");
        _logger.LogInformation("MoveLessonToPositionAsync: LessonId={LessonId}, NewSubTopicId={NewSubTopicId}, NewTopicId={NewTopicId}",
            moveResource.LessonId, moveResource.NewSubTopicId, moveResource.NewTopicId);
        _logger.LogInformation("MoveLessonToPositionAsync: RelativeToId={RelativeToId}, Position={Position}, RelativeToType={RelativeToType}",
            moveResource.RelativeToId, moveResource.Position, moveResource.RelativeToType);

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Get the lesson to move
            var lesson = await GetByIdAsync(moveResource.LessonId);
            if (lesson == null || lesson.UserId != userId)
            {
                throw new ArgumentException("Lesson not found or not owned by user");
            }

            _logger.LogInformation("MoveLessonToPositionAsync: Current lesson - SubTopicId={CurrentSubTopicId}, TopicId={CurrentTopicId}, SortOrder={CurrentSortOrder}",
                lesson.SubTopicId, lesson.TopicId, lesson.SortOrder);

            int targetSortOrder;

            if (moveResource.NewSubTopicId.HasValue)
            {
                // Moving to SubTopic - use existing logic
                var containerLessons = await GetContainerLessonsAsync(
                    moveResource.NewSubTopicId,
                    null,
                    userId
                );

                targetSortOrder = CalculateTargetSortOrder(
                    containerLessons,
                    moveResource.RelativeToId.Value,
                    moveResource.Position,
                    moveResource.RelativeToType
                );

                // Update lesson container and position
                lesson.SubTopicId = moveResource.NewSubTopicId;
                lesson.TopicId = null;
                lesson.SortOrder = targetSortOrder;

                // Renumber SubTopic lessons only
                await RenumberContainerLessonsAsync(
                    containerLessons,
                    moveResource.LessonId,
                    targetSortOrder
                );
            }
            else if (moveResource.NewTopicId.HasValue)
            {
                // ✅ ENHANCED: Moving to direct Topic - use new logic that considers ALL Topic entities
                targetSortOrder = await CalculateTargetSortOrderForTopicAsync(
                    moveResource.NewTopicId.Value,
                    moveResource.RelativeToId.Value,
                    moveResource.Position,
                    moveResource.RelativeToType,
                    userId
                );

                // Update lesson container and position
                lesson.SubTopicId = null;
                lesson.TopicId = moveResource.NewTopicId;
                lesson.SortOrder = targetSortOrder;

                // ✅ NEW: Renumber ALL Topic entities (Lessons + SubTopics)
                await RenumberAllTopicEntitiesAsync(
                    moveResource.NewTopicId.Value,
                    moveResource.LessonId,
                    targetSortOrder,
                    userId
                );
            }
            else
            {
                throw new ArgumentException("Must specify either NewSubTopicId or NewTopicId");
            }

            _logger.LogInformation("MoveLessonToPositionAsync: Calculated target sort order {TargetSortOrder} for lesson {LessonId}", targetSortOrder, moveResource.LessonId);
            _logger.LogInformation("MoveLessonToPositionAsync: Updated lesson container - NewSubTopicId={NewSubTopicId}, NewTopicId={NewTopicId}, NewSortOrder={NewSortOrder}",
                lesson.SubTopicId, lesson.TopicId, lesson.SortOrder);

            // Save the moved lesson
            _context.Lessons.Update(lesson);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            _logger.LogInformation("MoveLessonToPositionAsync: Successfully completed positional move");
            _logger.LogInformation("=== REPOSITORY POSITION MOVE DIAGNOSTICS END ===");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "MoveLessonToPositionAsync: Transaction rolled back due to error");
            throw;
        }
    }

    private int CalculateTargetSortOrder(
    List<Lesson> containerLessons,
    int relativeToId,
    string position,
    string relativeToType)
    {
        _logger.LogInformation("CalculateTargetSortOrder: RelativeToId={RelativeToId}, Position={Position}, RelativeToType={RelativeToType}",
            relativeToId, position, relativeToType);

        if (relativeToType == "Lesson")
        {
            var relativeLesson = containerLessons.FirstOrDefault(l => l.Id == relativeToId);
            if (relativeLesson != null)
            {
                var targetSort = position == "before" ? relativeLesson.SortOrder : relativeLesson.SortOrder + 1;
                _logger.LogInformation("CalculateTargetSortOrder: Found relative lesson Id={RelativeLessonId}, SortOrder={RelativeSortOrder}, calculated target={TargetSort}",
                    relativeLesson.Id, relativeLesson.SortOrder, targetSort);
                return targetSort;
            }
            else
            {
                _logger.LogWarning("CalculateTargetSortOrder: Relative lesson Id={RelativeToId} not found in container", relativeToId);
            }
        }
        else if (relativeToType == "SubTopic")
        {
            var subtopicLessons = containerLessons.Where(l => l.SubTopicId == relativeToId).ToList();
            _logger.LogInformation("CalculateTargetSortOrder: Found {SubTopicLessonCount} lessons in relative SubTopic Id={RelativeToId}",
                subtopicLessons.Count, relativeToId);

            if (subtopicLessons.Any())
            {
                var sortedSubtopicLessons = subtopicLessons.OrderBy(l => l.SortOrder).ToList();
                int targetSort;
                if (position == "before")
                {
                    targetSort = sortedSubtopicLessons.First().SortOrder;
                }
                else
                {
                    targetSort = sortedSubtopicLessons.Last().SortOrder + 1;
                }
                _logger.LogInformation("CalculateTargetSortOrder: SubTopic position calculation - first={FirstSort}, last={LastSort}, target={TargetSort}",
                    sortedSubtopicLessons.First().SortOrder, sortedSubtopicLessons.Last().SortOrder, targetSort);
                return targetSort;
            }
        }

        // Fallback: append to end
        var fallbackSort = containerLessons.Any() ? containerLessons.Max(l => l.SortOrder) + 1 : 0;
        _logger.LogWarning("CalculateTargetSortOrder: Using fallback sort order {FallbackSort}", fallbackSort);
        return fallbackSort;
    }

    // ✅ UPDATED: Enhanced position calculation using ALL Topic entities
    private async Task<int> CalculateTargetSortOrderForTopicAsync(
        int topicId,
        int relativeToId,
        string position,
        string relativeToType,
        int userId)
    {
        _logger.LogInformation("CalculateTargetSortOrderForTopic: TopicId={TopicId}, RelativeToId={RelativeToId}, Position={Position}, RelativeToType={RelativeToType}",
            topicId, relativeToId, position, relativeToType);

        // Get all entities in Topic to understand the complete sort order space
        var allEntities = await GetAllTopicEntitiesAsync(topicId, userId);

        if (relativeToType == "Lesson")
        {
            var relativeEntity = allEntities.FirstOrDefault(e => e.Id == relativeToId && e.Type == "Lesson");
            if (relativeEntity != default)
            {
                var targetSort = position == "before" ? relativeEntity.SortOrder : relativeEntity.SortOrder + 1;
                _logger.LogInformation("CalculateTargetSortOrderForTopic: Found relative lesson Id={RelativeLessonId}, SortOrder={RelativeSortOrder}, calculated target={TargetSort}",
                    relativeEntity.Id, relativeEntity.SortOrder, targetSort);
                return targetSort;
            }
        }
        else if (relativeToType == "SubTopic")
        {
            var relativeEntity = allEntities.FirstOrDefault(e => e.Id == relativeToId && e.Type == "SubTopic");
            if (relativeEntity != default)
            {
                var targetSort = position == "before" ? relativeEntity.SortOrder : relativeEntity.SortOrder + 1;
                _logger.LogInformation("CalculateTargetSortOrderForTopic: Found relative SubTopic Id={RelativeSubTopicId}, SortOrder={RelativeSortOrder}, calculated target={TargetSort}",
                    relativeEntity.Id, relativeEntity.SortOrder, targetSort);
                return targetSort;
            }
        }

        // Fallback: append to end
        var fallbackSort = allEntities.Any() ? allEntities.Max(e => e.SortOrder) + 1 : 0;
        _logger.LogWarning("CalculateTargetSortOrderForTopic: Using fallback sort order {FallbackSort}", fallbackSort);
        return fallbackSort;
    }

    private async Task<List<Lesson>> GetContainerLessonsAsync(int? subTopicId, int? topicId, int userId)
    {
        if (subTopicId.HasValue)
        {
            // Moving to SubTopic - only get lessons within that specific SubTopic
            _logger.LogInformation("GetContainerLessonsAsync: Getting lessons for SubTopic {SubTopicId}", subTopicId.Value);

            return await _context.Lessons
                .Where(l => l.SubTopicId == subTopicId.Value && l.UserId == userId && !l.Archived)
                .OrderBy(l => l.SortOrder)
                .ToListAsync();
        }
        else if (topicId.HasValue)
        {
            // ✅ FIXED: Moving to direct Topic - only get direct Topic lessons for renumbering
            _logger.LogInformation("GetContainerLessonsAsync: Getting DIRECT lessons for Topic {TopicId} (excluding SubTopic lessons)", topicId.Value);

            return await _context.Lessons
                .Where(l => l.TopicId == topicId.Value && l.SubTopicId == null && l.UserId == userId && !l.Archived)
                .OrderBy(l => l.SortOrder)
                .ToListAsync();
        }

        _logger.LogWarning("GetContainerLessonsAsync: No valid container specified");
        return new List<Lesson>();
    }

    // ✅ NEW: Get all entities in Topic for position calculation
    private async Task<List<(int Id, int SortOrder, string Type)>> GetAllTopicEntitiesAsync(int topicId, int userId)
    {
        _logger.LogInformation("GetAllTopicEntitiesAsync: Getting all entities for Topic {TopicId}", topicId);

        var entities = new List<(int Id, int SortOrder, string Type)>();

        // Get direct Topic lessons
        var directLessons = await _context.Lessons
            .Where(l => l.TopicId == topicId && l.SubTopicId == null && l.UserId == userId && !l.Archived)
            .Select(l => new { l.Id, l.SortOrder })
            .ToListAsync();

        entities.AddRange(directLessons.Select(l => (l.Id, l.SortOrder, "Lesson")));

        // Get SubTopics
        var subTopics = await _context.SubTopics
            .Where(st => st.TopicId == topicId && st.UserId == userId && !st.Archived)
            .Select(st => new { st.Id, st.SortOrder })
            .ToListAsync();

        entities.AddRange(subTopics.Select(st => (st.Id, st.SortOrder, "SubTopic")));

        var sortedEntities = entities.OrderBy(e => e.SortOrder).ToList();

        _logger.LogInformation("GetAllTopicEntitiesAsync: Found {EntityCount} entities", sortedEntities.Count);
        foreach (var entity in sortedEntities)
        {
            _logger.LogInformation("Topic entity: Id={Id}, SortOrder={SortOrder}, Type={Type}",
                entity.Id, entity.SortOrder, entity.Type);
        }

        return sortedEntities;
    }

    private async Task RenumberContainerLessonsAsync(
        List<Lesson> containerLessons,
        int movedLessonId,
        int targetSortOrder)
    {
        _logger.LogDebug($"RenumberContainerLessonsAsync: Renumbering container lessons, target position: {targetSortOrder}");

        // Filter out the moved lesson
        var otherLessons = containerLessons.Where(l => l.Id != movedLessonId).ToList();

        // Create new clean sequence with moved lesson slot reserved
        var assignedSortOrder = 0;
        foreach (var lesson in otherLessons.OrderBy(l => l.SortOrder))
        {
            // Reserve slot for moved lesson
            if (assignedSortOrder == targetSortOrder)
            {
                assignedSortOrder++; // Skip the target position
                _logger.LogDebug($"RenumberContainerLessonsAsync: Reserved slot {targetSortOrder} for moved lesson");
            }

            // Only update if sort order actually changes
            if (lesson.SortOrder != assignedSortOrder)
            {
                _logger.LogDebug($"RenumberContainerLessonsAsync: Updating lesson {lesson.Id} from {lesson.SortOrder} to {assignedSortOrder}");
                lesson.SortOrder = assignedSortOrder;
                _context.Lessons.Update(lesson);
            }

            assignedSortOrder++;
        }

        _logger.LogInformation($"RenumberContainerLessonsAsync: Renumbering complete, moved lesson will have position {targetSortOrder}");
    }
}
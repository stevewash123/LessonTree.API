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

    public async Task MoveLessonToPositionAsync(LessonMoveResource moveResource, int userId)
    {
        _logger.LogInformation($"MoveLessonToPositionAsync: Moving lesson {moveResource.LessonId} to position for user {userId}");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Get the lesson to move
            var lesson = await GetByIdAsync(moveResource.LessonId);
            if (lesson == null || lesson.UserId != userId)
            {
                throw new ArgumentException("Lesson not found or not owned by user");
            }

            // Get all lessons in target container
            var containerLessons = await GetContainerLessonsAsync(
                moveResource.NewSubTopicId,
                moveResource.NewTopicId,
                userId
            );

            // Calculate target position
            var targetSortOrder = CalculateTargetSortOrder(
                containerLessons,
                moveResource.RelativeToId.Value,
                moveResource.Position,
                moveResource.RelativeToType
            );

            _logger.LogInformation($"MoveLessonToPositionAsync: Calculated target sort order {targetSortOrder} for lesson {moveResource.LessonId}");

            // Update lesson container and position
            lesson.SubTopicId = moveResource.NewSubTopicId;
            lesson.TopicId = moveResource.NewTopicId;
            lesson.SortOrder = targetSortOrder;

            // Renumber all affected lessons to prevent collisions
            await RenumberContainerLessonsAsync(
                containerLessons,
                moveResource.LessonId,
                targetSortOrder
            );

            // Save the moved lesson
            _context.Lessons.Update(lesson);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            _logger.LogInformation($"MoveLessonToPositionAsync: Successfully moved lesson {moveResource.LessonId} to position {targetSortOrder}");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<List<Lesson>> GetContainerLessonsAsync(int? subTopicId, int? topicId, int userId)
    {
        if (subTopicId.HasValue)
        {
            return await _context.Lessons
                .Where(l => l.SubTopicId == subTopicId.Value && l.UserId == userId && !l.Archived)
                .OrderBy(l => l.SortOrder)
                .ToListAsync();
        }
        else if (topicId.HasValue)
        {
            // Get all lessons in topic (direct + from subtopics)
            var directLessons = await _context.Lessons
                .Where(l => l.TopicId == topicId.Value && l.UserId == userId && !l.Archived)
                .ToListAsync();

            var subtopicLessons = await _context.Lessons
                .Include(l => l.SubTopic)
                .Where(l => l.SubTopic.TopicId == topicId.Value && l.UserId == userId && !l.Archived)
                .ToListAsync();

            return directLessons.Concat(subtopicLessons).OrderBy(l => l.SortOrder).ToList();
        }

        return new List<Lesson>();
    }

    private int CalculateTargetSortOrder(
        List<Lesson> containerLessons,
        int relativeToId,
        string position,
        string relativeToType)
    {
        if (relativeToType == "Lesson")
        {
            var relativeLesson = containerLessons.FirstOrDefault(l => l.Id == relativeToId);
            if (relativeLesson != null)
            {
                return position == "before" ? relativeLesson.SortOrder : relativeLesson.SortOrder + 1;
            }
        }
        else if (relativeToType == "SubTopic")
        {
            var subtopicLessons = containerLessons.Where(l => l.SubTopicId == relativeToId).ToList();
            if (subtopicLessons.Any())
            {
                var sortedSubtopicLessons = subtopicLessons.OrderBy(l => l.SortOrder).ToList();
                if (position == "before")
                {
                    return sortedSubtopicLessons.First().SortOrder;
                }
                else
                {
                    return sortedSubtopicLessons.Last().SortOrder + 1;
                }
            }
        }

        // Fallback: append to end
        return containerLessons.Any() ? containerLessons.Max(l => l.SortOrder) + 1 : 0;
    }

    private async Task RenumberContainerLessonsAsync(
        List<Lesson> containerLessons,
        int movedLessonId,
        int targetSortOrder)
    {
        // Filter out the moved lesson
        var otherLessons = containerLessons.Where(l => l.Id != movedLessonId).ToList();

        // Create new clean sequence
        var sortOrder = 0;
        foreach (var lesson in otherLessons.OrderBy(l => l.SortOrder))
        {
            // Skip target position for moved lesson
            if (sortOrder == targetSortOrder)
            {
                sortOrder++;
            }

            // Only update if sort order actually changes
            if (lesson.SortOrder != sortOrder)
            {
                lesson.SortOrder = sortOrder;
                _context.Lessons.Update(lesson);
                _logger.LogDebug($"RenumberContainerLessonsAsync: Updated lesson {lesson.Id} to sort order {sortOrder}");
            }

            sortOrder++;
        }
    }
}
// **COMPLETE FILE** - LessonRepository.cs - Standardized to enterprise patterns
// RESPONSIBILITY: Lesson data access with attachment relationships and filtering
// DOES NOT: Handle lesson content validation or attachment file management (that's in services)
// CALLED BY: LessonService for all lesson operations

using LessonTree.DAL;
using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
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
}
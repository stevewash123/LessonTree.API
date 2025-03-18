using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
using LessonTree.DAL;
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
        _logger.LogDebug("Retrieving all lessons");
        var query = _context.Lessons.AsQueryable();
        if (include != null)
        {
            query = include(query);
        }
        return query;
    }

    public async Task<Lesson?> GetByIdAsync(int id, Func<IQueryable<Lesson>, IQueryable<Lesson>> include = null)
    {
        _logger.LogDebug("Retrieving lesson by ID: {LessonId}", id);
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
                .Include(l => l.LessonStandards).ThenInclude(ls => ls.Standard);
        }
        var lesson = await query.FirstOrDefaultAsync(l => l.Id == id);
        if (lesson == null)
        {
            _logger.LogWarning("Lesson with ID {LessonId} not found", id);
        }
        return lesson;
    }

    public async Task<int> AddAsync(Lesson lesson)
    {
        _logger.LogDebug("Adding lesson: {Title}", lesson.Title);
        _context.Lessons.Add(lesson);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Added lesson with ID: {LessonId}, Title: {Title}", lesson.Id, lesson.Title);
        return lesson.Id;
    }

    public async Task UpdateAsync(Lesson lesson)
    {
        _logger.LogDebug("Updating lesson: {Title}", lesson.Title);
        _context.Lessons.Update(lesson);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated lesson with ID: {LessonId}, Title: {Title}", lesson.Id, lesson.Title);
    }

    public async Task DeleteAsync(int id)
    {
        _logger.LogDebug("Deleting lesson with ID: {LessonId}", id);
        var lesson = await _context.Lessons.FindAsync(id);
        if (lesson != null)
        {
            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Deleted lesson with ID: {LessonId}", id);
        }
        else
        {
            _logger.LogWarning("Lesson with ID {LessonId} not found for deletion", id);
        }
    }

    // Existing Async Method (Already Present)
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

    // Existing Async Method (Already Present)
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
            .Where(l => l.Title.Contains(title));
        return lessons;
    }
}
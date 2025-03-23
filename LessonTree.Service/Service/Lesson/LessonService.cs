using AutoMapper;
using AutoMapper.QueryableExtensions;
using LessonTree.BLL.Service;
using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
using LessonTree.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class LessonService : ILessonService
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ISubTopicRepository _subTopicRepository;
    private readonly IStandardRepository _standardRepository;
    private readonly ILogger<LessonService> _logger;
    private readonly IMapper _mapper;

    public LessonService(ILessonRepository lessonRepository, ISubTopicRepository subTopicRepository, IStandardRepository standardRepository, ILogger<LessonService> logger, IMapper mapper)
    {
        _lessonRepository = lessonRepository;
        _subTopicRepository = subTopicRepository;
        _standardRepository = standardRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<LessonDetailResource?> GetByIdAsync(int id)
    {
        _logger.LogDebug("Fetching lesson by ID: {LessonId} in service", id);
        var lesson = await _lessonRepository.GetByIdAsync(id, q => q
            .Include(l => l.SubTopic)
            .Include(l => l.LessonAttachments).ThenInclude(ld => ld.Attachment)
            .Include(l => l.LessonStandards).ThenInclude(ls => ls.Standard));

        if (lesson == null)
        {
            _logger.LogWarning("Lesson with ID {LessonId} not found in service", id);
            return null;
        }

        _logger.LogDebug("Lesson with ID {LessonId} found. Title: {Title}, SubTopicId: {SubTopicId}",
            lesson.Id, lesson.Title, lesson.SubTopicId);
        var lessonResource = _mapper.Map<LessonDetailResource>(lesson);
        _logger.LogDebug("Mapped lesson with ID {LessonId} to LessonDetailResource",
            lessonResource.Id);
        return lessonResource;
    }
    
    public async Task<List<LessonResource>> GetAllAsync()
    {
        _logger.LogDebug("Fetching all lessons in service");
        var lessons = await _lessonRepository.GetAll()
            .ProjectTo<LessonResource>(_mapper.ConfigurationProvider)
            .ToListAsync();
        _logger.LogDebug("Fetched {Count} lessons", lessons.Count);
        return lessons;
    }

    public async Task<List<LessonResource>> GetLessonsBySubtopic(int subtopicId)
    {
        _logger.LogDebug("Fetching all lessons in service");
        var lessons = await _lessonRepository.GetAll()
            .Where(l => l.SubTopicId == subtopicId)
            .ProjectTo<LessonResource>(_mapper.ConfigurationProvider)
            .ToListAsync();
        _logger.LogDebug("Fetched {Count} lessons", lessons.Count);
        return lessons;
    }

    public async Task<int> AddAsync(LessonCreateResource lessonCreateResource)
    {
        _logger.LogDebug("Adding lesson: {Title} in service", lessonCreateResource.Title);
        var lesson = _mapper.Map<Lesson>(lessonCreateResource);
        var createdLessonId = await _lessonRepository.AddAsync(lesson);
        _logger.LogInformation("Lesson added with ID: {LessonId}", createdLessonId);
        return createdLessonId; // Return the ID for potential use
    }

    public async Task UpdateAsync(LessonUpdateResource lessonUpdateResource)
    {
        _logger.LogDebug("Updating lesson with ID: {LessonId}, Title: {Title} in service",
            lessonUpdateResource.Id, lessonUpdateResource.Title);
        var existingLesson = await _lessonRepository.GetByIdAsync(lessonUpdateResource.Id);
        if (existingLesson == null)
        {
            _logger.LogWarning("Lesson with ID {LessonId} not found for update", lessonUpdateResource.Id);
            throw new ArgumentException("Lesson not found");
        }

        _mapper.Map(lessonUpdateResource, existingLesson);
        await _lessonRepository.UpdateAsync(existingLesson);
        _logger.LogInformation("Lesson updated with ID: {LessonId}", existingLesson.Id);
    }

    public async Task DeleteAsync(int id)
    {
        _logger.LogDebug("Deleting lesson with ID: {LessonId} in service", id);
        var lesson = await _lessonRepository.GetByIdAsync(id);
        if (lesson == null)
        {
            _logger.LogWarning("Lesson with ID {LessonId} not found for deletion", id);
            throw new ArgumentException($"Lesson with ID {id} not found");
        }

        await _lessonRepository.DeleteAsync(id);
        _logger.LogInformation("Lesson deleted with ID: {LessonId}", id);
    }

    public async Task AddAttachmentAsync(int lessonId, int attachmentId)
    {
        _logger.LogDebug("Adding attachment ID: {AttachmentId} to Lesson ID: {LessonId} in service", attachmentId, lessonId);
        try
        {
            await _lessonRepository.AddAttachmentAsync(lessonId, attachmentId);
            _logger.LogInformation("Attachment ID: {AttachmentId} added to lesson with ID: {LessonId}", attachmentId, lessonId);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Failed to add attachment ID: {AttachmentId} to lesson ID: {LessonId}", attachmentId, lessonId);
            throw;
        }
    }

    public async Task RemoveAttachmentAsync(int lessonId, int attachmentId)
    {
        _logger.LogDebug("Removing attachment ID: {AttachmentId} from Lesson ID: {LessonId} in service", attachmentId, lessonId);
        await _lessonRepository.RemoveAttachmentAsync(lessonId, attachmentId);
        _logger.LogInformation("Attachment ID: {AttachmentId} removed from lesson with ID: {LessonId}", attachmentId, lessonId);
    }

    public async Task<List<LessonResource>> GetByTitleAsync(string title)
    {
        _logger.LogDebug("Fetching lessons by title: {Title} in service", title);
        var lessons = await _lessonRepository.GetByTitle(title).ToListAsync();
        _logger.LogDebug("Found {Count} lessons with title containing: {Title}", lessons.Count, title);
        return _mapper.Map<List<LessonResource>>(lessons ?? new List<Lesson>());
    }

    public async Task MoveLessonAsync(int lessonId, int newSubTopicId)
    {
        _logger.LogDebug("Moving Lesson ID: {LessonId} to SubTopic ID: {NewSubTopicId} in service", lessonId, newSubTopicId);
        var lesson = await _lessonRepository.GetByIdAsync(lessonId, q => q.Include(l => l.SubTopic));
        if (lesson == null)
        {
            _logger.LogError("Lesson with ID {LessonId} not found", lessonId);
            throw new ArgumentException("Lesson not found");
        }

        var newSubTopic = await _subTopicRepository.GetByIdAsync(newSubTopicId);
        if (newSubTopic == null)
        {
            _logger.LogError("SubTopic with ID {SubTopicId} not found", newSubTopicId);
            throw new ArgumentException("SubTopic not found");
        }

        lesson.SubTopicId = newSubTopicId;
        await _lessonRepository.UpdateAsync(lesson);
        _logger.LogInformation("Lesson ID: {LessonId} moved to SubTopic ID: {NewSubTopicId}", lessonId, newSubTopicId);
    }

    public async Task AddStandardToLessonAsync(int lessonId, int standardId)
    {
        _logger.LogDebug("Adding standard ID: {StandardId} to Lesson ID: {LessonId} in service", standardId, lessonId);
        var lesson = await _lessonRepository.GetByIdAsync(lessonId);
        if (lesson == null)
        {
            _logger.LogError("Lesson with ID {LessonId} not found", lessonId);
            throw new ArgumentException("Lesson not found");
        }

        var standard = await _standardRepository.GetByIdAsync(standardId);
        if (standard == null)
        {
            _logger.LogError("Standard with ID {StandardId} not found", standardId);
            throw new ArgumentException("Standard not found");
        }

        if (!lesson.LessonStandards.Any(ls => ls.StandardId == standardId))
        {
            lesson.LessonStandards.Add(new LessonStandard { LessonId = lessonId, StandardId = standardId });
            await _lessonRepository.UpdateAsync(lesson);
            _logger.LogInformation("Standard ID: {StandardId} added to lesson with ID: {LessonId}", standardId, lessonId);
        }
        else
        {
            _logger.LogDebug("Standard ID: {StandardId} already exists in lesson with ID: {LessonId}", standardId, lessonId);
        }
    }

    public async Task RemoveStandardFromLessonAsync(int lessonId, int standardId)
    {
        _logger.LogDebug("Removing standard ID: {StandardId} from Lesson ID: {LessonId} in service", standardId, lessonId);
        var lesson = await _lessonRepository.GetByIdAsync(lessonId);
        if (lesson == null)
        {
            _logger.LogError("Lesson with ID {LessonId} not found", lessonId);
            throw new ArgumentException("Lesson not found");
        }

        var lessonStandard = lesson.LessonStandards.FirstOrDefault(ls => ls.StandardId == standardId);
        if (lessonStandard != null)
        {
            lesson.LessonStandards.Remove(lessonStandard);
            await _lessonRepository.UpdateAsync(lesson);
            _logger.LogInformation("Standard ID: {StandardId} removed from lesson with ID: {LessonId}", standardId, lessonId);
        }
        else
        {
            _logger.LogDebug("Standard ID: {StandardId} not found in lesson with ID: {LessonId}", standardId, lessonId);
        }
    }

    public async Task<LessonResource> CopyLessonAsync(int lessonId, int newSubTopicId)
    {
        _logger.LogDebug("Copying Lesson ID: {LessonId} to SubTopic ID: {NewSubTopicId} in service", lessonId, newSubTopicId);
        var originalLesson = await _lessonRepository.GetByIdAsync(lessonId, q => q
            .Include(l => l.LessonAttachments).ThenInclude(ld => ld.Attachment)
            .Include(l => l.LessonStandards));

        if (originalLesson == null)
        {
            _logger.LogError("Lesson with ID {LessonId} not found", lessonId);
            throw new ArgumentException("Lesson not found");
        }

        var newLesson = new Lesson
        {
            Title = originalLesson.Title,
            SubTopicId = newSubTopicId,
            LessonAttachments = originalLesson.LessonAttachments.Select(ld => new LessonAttachment
            {
                AttachmentId = ld.AttachmentId
            }).ToList(),
            LessonStandards = originalLesson.LessonStandards.Select(ls => new LessonStandard
            {
                StandardId = ls.StandardId
            }).ToList()
        };

        await _lessonRepository.AddAsync(newLesson);
        _logger.LogInformation("Copied Lesson ID: {OriginalLessonId} to new Lesson ID: {NewLessonId} under SubTopic ID: {NewSubTopicId}",
            lessonId, newLesson.Id, newSubTopicId);

        return _mapper.Map<LessonResource>(newLesson);
    }
}
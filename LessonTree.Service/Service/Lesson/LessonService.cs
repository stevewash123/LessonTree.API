using AutoMapper;
using AutoMapper.QueryableExtensions;
using LessonTree.BLL.Service;
using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
using LessonTree.Models.DTO;
using LessonTree.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class LessonService : ILessonService
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ISubTopicRepository _subTopicRepository;
    private readonly IStandardRepository _standardRepository;
    private readonly ILogger<LessonService> _logger;
    private readonly IMapper _mapper;

    public LessonService(
        ILessonRepository lessonRepository,
        ISubTopicRepository subTopicRepository,
        IStandardRepository standardRepository,
        ILogger<LessonService> logger,
        IMapper mapper)
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
            .Include(l => l.Topic)
            .Include(l => l.User)
            .Include(l => l.Team)
            .Include(l => l.LessonAttachments).ThenInclude(ld => ld.Attachment)
            .Include(l => l.LessonStandards).ThenInclude(ls => ls.Standard));

        if (lesson == null)
        {
            _logger.LogWarning("Lesson with ID {LessonId} not found in service", id);
            return null;
        }

        _logger.LogDebug("Lesson with ID {LessonId} found. Title: {Title}, SubTopicId: {SubTopicId}, TopicId: {TopicId}, UserId: {UserId}, Archived: {Archived}",
            lesson.Id, lesson.Title, lesson.SubTopicId, lesson.TopicId, lesson.UserId, lesson.Archived);
        var lessonResource = _mapper.Map<LessonDetailResource>(lesson);
        _logger.LogDebug("Mapped lesson with ID {LessonId} to LessonDetailResource", lessonResource.Id);
        return lessonResource;
    }

    public async Task<List<LessonResource>> GetAllAsync(int? userId = null, ArchiveFilter filter = ArchiveFilter.Active)
    {
        _logger.LogDebug("Fetching all lessons in service. UserId: {UserId}, Filter: {Filter}", userId, filter);
        var query = _lessonRepository.GetAll();

        if (userId.HasValue)
        {
            query = query.Where(l => l.UserId == userId.Value);
        }

        query = filter switch
        {
            ArchiveFilter.Active => query.Where(l => !l.Archived),
            ArchiveFilter.Archived => query.Where(l => l.Archived),
            ArchiveFilter.Both => query,
            _ => throw new ArgumentOutOfRangeException(nameof(filter), "Invalid filter value")
        };

        var lessons = await query
            .ProjectTo<LessonResource>(_mapper.ConfigurationProvider)
            .ToListAsync();

        _logger.LogDebug("Fetched {Count} lessons", lessons.Count);
        return lessons;
    }

    public async Task<List<LessonResource>> GetLessonsBySubtopic(int subTopicId, int? userId = null, ArchiveFilter filter = ArchiveFilter.Active)
    {
        _logger.LogDebug("Fetching lessons by SubTopic ID: {SubTopicId} in service. UserId: {UserId}, Filter: {Filter}",
            subTopicId, userId, filter);

        var query = _lessonRepository.GetBySubTopicId(subTopicId, true) // Fetch all initially, filter below
            .Where(l => !userId.HasValue || l.UserId == userId.Value);

        query = filter switch
        {
            ArchiveFilter.Active => query.Where(l => !l.Archived),
            ArchiveFilter.Archived => query.Where(l => l.Archived),
            ArchiveFilter.Both => query,
            _ => throw new ArgumentOutOfRangeException(nameof(filter), "Invalid filter value")
        };

        var lessons = await query
            .ProjectTo<LessonResource>(_mapper.ConfigurationProvider)
            .ToListAsync();

        _logger.LogDebug("Fetched {Count} lessons for SubTopic ID: {SubTopicId}", lessons.Count, subTopicId);
        return lessons;
    }

    public async Task<List<LessonResource>> GetLessonsByTopic(int topicId, int? userId = null, ArchiveFilter filter = ArchiveFilter.Active)
    {
        _logger.LogDebug("Fetching lessons by Topic ID: {TopicId} in service. UserId: {UserId}, Filter: {Filter}",
            topicId, userId, filter);

        var query = _lessonRepository.GetByTopicId(topicId, true) // Fetch all initially, filter below
            .Where(l => !userId.HasValue || l.UserId == userId.Value);

        query = filter switch
        {
            ArchiveFilter.Active => query.Where(l => !l.Archived),
            ArchiveFilter.Archived => query.Where(l => l.Archived),
            ArchiveFilter.Both => query,
            _ => throw new ArgumentOutOfRangeException(nameof(filter), "Invalid filter value")
        };

        var lessons = await query
            .ProjectTo<LessonResource>(_mapper.ConfigurationProvider)
            .ToListAsync();

        _logger.LogDebug("Fetched {Count} lessons for Topic ID: {TopicId}", lessons.Count, topicId);
        return lessons;
    }
    public async Task<int> AddAsync(LessonCreateResource lessonCreateResource, int userId)
    {
        _logger.LogDebug("Adding lesson: {Title} for User ID: {UserId}", lessonCreateResource.Title, userId);

        if (lessonCreateResource.SubTopicId.HasValue && lessonCreateResource.TopicId.HasValue)
        {
            _logger.LogError("Lesson cannot have both SubTopicId and TopicId assigned");
            throw new ArgumentException("Lesson must be linked to either a SubTopic or a Topic, not both.");
        }

        if (!lessonCreateResource.SubTopicId.HasValue && !lessonCreateResource.TopicId.HasValue)
        {
            _logger.LogError("Lesson must have either a SubTopicId or TopicId assigned");
            throw new ArgumentException("Lesson must be linked to either a SubTopic or a Topic.");
        }

        var lesson = _mapper.Map<Lesson>(lessonCreateResource);
        lesson.UserId = userId; // Set UserId here
        var createdLessonId = await _lessonRepository.AddAsync(lesson);
        _logger.LogInformation("Lesson added with ID: {LessonId}, Title: {Title}", createdLessonId, lesson.Title);
        return createdLessonId;
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
        _logger.LogInformation("Lesson updated with ID: {LessonId}, Title: {Title}", existingLesson.Id, existingLesson.Title);
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
        try
        {
            await _lessonRepository.RemoveAttachmentAsync(lessonId, attachmentId);
            _logger.LogInformation("Attachment ID: {AttachmentId} removed from lesson with ID: {LessonId}", attachmentId, lessonId);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Failed to remove attachment ID: {AttachmentId} from lesson ID: {LessonId}", attachmentId, lessonId);
            throw;
        }
    }

    public async Task<List<LessonResource>> GetByTitleAsync(string title, int? userId = null, bool includeArchived = false)
    {
        _logger.LogDebug("Fetching lessons by title: {Title} in service. UserId: {UserId}, IncludeArchived: {IncludeArchived}",
            title, userId, includeArchived);

        var query = _lessonRepository.GetByTitle(title);
        if (userId.HasValue)
        {
            query = query.Where(l => l.UserId == userId.Value);
        }
        if (!includeArchived)
        {
            query = query.Where(l => !l.Archived);
        }

        var lessons = await query.ToListAsync();
        _logger.LogDebug("Found {Count} lessons with title containing: {Title}", lessons.Count, title);
        return _mapper.Map<List<LessonResource>>(lessons);
    }

    public async Task MoveLessonAsync(int lessonId, int? newSubTopicId, int? newTopicId)
    {
        _logger.LogDebug("Moving Lesson ID: {LessonId} to SubTopic ID: {NewSubTopicId} or Topic ID: {NewTopicId} in service",
            lessonId, newSubTopicId, newTopicId);

        var lesson = await _lessonRepository.GetByIdAsync(lessonId, q => q.Include(l => l.SubTopic).Include(l => l.Topic));
        if (lesson == null)
        {
            _logger.LogError("Lesson with ID {LessonId} not found", lessonId);
            throw new ArgumentException("Lesson not found");
        }

        if (newSubTopicId.HasValue && newTopicId.HasValue)
        {
            _logger.LogError("Lesson cannot be moved to both SubTopicId {NewSubTopicId} and TopicId {NewTopicId}", newSubTopicId, newTopicId);
            throw new ArgumentException("Lesson can only be moved to either a SubTopic or a Topic, not both.");
        }

        if (!newSubTopicId.HasValue && !newTopicId.HasValue)
        {
            _logger.LogError("Lesson must be moved to either a SubTopicId or TopicId");
            throw new ArgumentException("Lesson must be moved to either a SubTopic or a Topic.");
        }

        if (newSubTopicId.HasValue)
        {
            var newSubTopic = await _subTopicRepository.GetByIdAsync(newSubTopicId.Value);
            if (newSubTopic == null)
            {
                _logger.LogError("SubTopic with ID {SubTopicId} not found", newSubTopicId);
                throw new ArgumentException("SubTopic not found");
            }
            lesson.SubTopicId = newSubTopicId;
            lesson.TopicId = null;
        }
        else if (newTopicId.HasValue)
        {
            // Assuming a TopicRepository exists; if not, adjust accordingly
            var topicExists = await _lessonRepository.GetByTopicId(newTopicId.Value).AnyAsync();
            if (!topicExists)
            {
                _logger.LogError("Topic with ID {TopicId} not found", newTopicId);
                throw new ArgumentException("Topic not found");
            }
            lesson.TopicId = newTopicId;
            lesson.SubTopicId = null;
        }

        await _lessonRepository.UpdateAsync(lesson);
        _logger.LogInformation("Lesson ID: {LessonId} moved to SubTopic ID: {NewSubTopicId} or Topic ID: {NewTopicId}",
            lessonId, newSubTopicId, newTopicId);
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

    public async Task<LessonResource> CopyLessonAsync(int lessonId, int? newSubTopicId, int? newTopicId, int userId)
    {
        _logger.LogDebug("Copying Lesson ID: {LessonId} to SubTopic ID: {NewSubTopicId} or Topic ID: {NewTopicId} for User ID: {UserId}",
            lessonId, newSubTopicId, newTopicId, userId);

        var originalLesson = await _lessonRepository.GetByIdAsync(lessonId, q => q
            .Include(l => l.LessonAttachments).ThenInclude(ld => ld.Attachment)
            .Include(l => l.LessonStandards));

        if (originalLesson == null)
        {
            _logger.LogError("Lesson with ID {LessonId} not found", lessonId);
            throw new ArgumentException("Lesson not found");
        }

        if (newSubTopicId.HasValue && newTopicId.HasValue)
        {
            _logger.LogError("Lesson cannot be copied to both SubTopicId {NewSubTopicId} and TopicId {NewTopicId}", newSubTopicId, newTopicId);
            throw new ArgumentException("Lesson can only be copied to either a SubTopic or a Topic, not both.");
        }

        if (!newSubTopicId.HasValue && !newTopicId.HasValue)
        {
            _logger.LogError("Lesson must be copied to either a SubTopicId or TopicId");
            throw new ArgumentException("Lesson must be copied to either a SubTopic or a Topic.");
        }

        var newLesson = new Lesson
        {
            Title = originalLesson.Title + " (Copy)",
            Objective = originalLesson.Objective,
            Level = originalLesson.Level,
            Materials = originalLesson.Materials,
            ClassTime = originalLesson.ClassTime,
            Methods = originalLesson.Methods,
            SpecialNeeds = originalLesson.SpecialNeeds,
            Assessment = originalLesson.Assessment,
            UserId = userId, // Set to the copier’s UserId
            Visibility = originalLesson.Visibility,
            TeamId = originalLesson.TeamId,
            SubTopicId = newSubTopicId,
            TopicId = newTopicId,
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
        _logger.LogInformation("Copied Lesson ID: {OriginalLessonId} to new Lesson ID: {NewLessonId} under SubTopic ID: {NewSubTopicId} or Topic ID: {NewTopicId} by User ID: {UserId}",
            lessonId, newLesson.Id, newSubTopicId, newTopicId, userId);

        return _mapper.Map<LessonResource>(newLesson);
    }

    public async Task<Lesson?> GetDomainLessonByIdAsync(int id)
    {
        _logger.LogDebug("Fetching domain lesson by ID: {LessonId}", id);
        var lesson = await _lessonRepository.GetByIdAsync(id);
        if (lesson == null)
        {
            _logger.LogWarning("Domain lesson with ID {LessonId} not found", id);
        }
        return lesson;
    }
}
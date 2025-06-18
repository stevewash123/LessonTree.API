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
    private readonly ITopicRepository _topicRepository;
    private readonly ISubTopicRepository _subTopicRepository;
    private readonly IStandardRepository _standardRepository;
    private readonly ILogger<LessonService> _logger;
    private readonly IMapper _mapper;

    public LessonService(
        ILessonRepository lessonRepository,
        ITopicRepository topicRepository,
        ISubTopicRepository subTopicRepository,
        IStandardRepository standardRepository,
        ILogger<LessonService> logger,
        IMapper mapper)
    {
        _lessonRepository = lessonRepository;
        _topicRepository = topicRepository;
        _subTopicRepository = subTopicRepository;
        _standardRepository = standardRepository;
        _logger = logger;
        _mapper = mapper;
    }

    // **PARTIAL FILE** - LessonService.cs - Logging Standardization (Key Methods)
    // INTEGRATION: Replace the main CRUD method logging patterns

    public async Task<LessonDetailResource?> GetByIdAsync(int id, int userId)
    {
        _logger.LogInformation($"GetByIdAsync: Fetching lesson {id} for user {userId}");

        var lesson = await _lessonRepository.GetByIdAsync(id, q => q
            .Include(l => l.SubTopic).ThenInclude(s => s.Topic)
            .Include(l => l.Topic)
            .Include(l => l.User)
            .Include(l => l.Notes)
            .Include(l => l.LessonAttachments).ThenInclude(ld => ld.Attachment)
            .Include(l => l.LessonStandards).ThenInclude(ls => ls.Standard));

        if (lesson == null || lesson.UserId != userId)
        {
            _logger.LogWarning($"GetByIdAsync: Lesson {id} not found or not owned by user {userId}");
            return null;
        }

        _logger.LogInformation($"GetByIdAsync: Found lesson {id} '{lesson.Title}' for user {userId} - SubTopicId: {lesson.SubTopicId}, TopicId: {lesson.TopicId}");
        return _mapper.Map<LessonDetailResource>(lesson);
    }

    public async Task<List<LessonResource>> GetAllAsync(int userId, ArchiveFilter filter = ArchiveFilter.Active)
    {
        _logger.LogInformation($"GetAllAsync: Fetching lessons for user {userId}, filter: {filter}");

        var query = _lessonRepository.GetAll().Where(l => l.UserId == userId);

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

        _logger.LogInformation($"GetAllAsync: Found {lessons.Count} lessons for user {userId}");
        return lessons;
    }

    public async Task<int> AddAsync(LessonCreateResource lessonCreateResource, int userId)
    {
        _logger.LogInformation($"AddAsync: Creating lesson '{lessonCreateResource.Title}' for user {userId}");

        if (lessonCreateResource.SubTopicId.HasValue && lessonCreateResource.TopicId.HasValue)
        {
            _logger.LogError("AddAsync: Lesson cannot have both SubTopicId and TopicId assigned");
            throw new ArgumentException("Lesson must be linked to either a SubTopic or a Topic, not both");
        }

        if (!lessonCreateResource.SubTopicId.HasValue && !lessonCreateResource.TopicId.HasValue)
        {
            _logger.LogError("AddAsync: Lesson must have either a SubTopicId or TopicId assigned");
            throw new ArgumentException("Lesson must be linked to either a SubTopic or a Topic");
        }

        var lesson = _mapper.Map<Lesson>(lessonCreateResource);
        lesson.UserId = userId;
        var createdLessonId = await _lessonRepository.AddAsync(lesson);

        _logger.LogInformation($"AddAsync: Created lesson {createdLessonId} '{lesson.Title}' for user {userId}");
        return createdLessonId;
    }

    public async Task<LessonDetailResource> UpdateAsync(LessonUpdateResource lessonUpdateResource, int userId)
    {
        _logger.LogInformation($"UpdateAsync: Updating lesson {lessonUpdateResource.Id} '{lessonUpdateResource.Title}' for user {userId}");

        var existingLesson = await _lessonRepository.GetByIdAsync(lessonUpdateResource.Id);
        if (existingLesson == null)
        {
            _logger.LogInformation($"UpdateAsync: Lesson {lessonUpdateResource.Id} not found");
            throw new ArgumentException($"Lesson {lessonUpdateResource.Id} not found");
        }

        // Verify ownership
        if (existingLesson.UserId != userId)
        {
            _logger.LogWarning($"UpdateAsync: Lesson {lessonUpdateResource.Id} not owned by user {userId}");
            throw new UnauthorizedAccessException($"Lesson {lessonUpdateResource.Id} not owned by user");
        }

        _mapper.Map(lessonUpdateResource, existingLesson);
        await _lessonRepository.UpdateAsync(existingLesson);

        _logger.LogInformation($"UpdateAsync: Updated lesson {existingLesson.Id} '{existingLesson.Title}' for user {userId}");

        // Return the updated entity
        return await GetByIdAsync(existingLesson.Id, userId) ?? throw new InvalidOperationException("Updated lesson could not be retrieved");
    }

    public async Task DeleteAsync(int id, int userId)
    {
        _logger.LogInformation($"DeleteAsync: Deleting lesson {id} for user {userId}");

        var lesson = await _lessonRepository.GetByIdAsync(id);
        if (lesson == null)
        {
            _logger.LogInformation($"DeleteAsync: Lesson {id} not found");
            throw new ArgumentException($"Lesson {id} not found");
        }

        // Ownership validation - moved from controller to service
        if (lesson.UserId != userId)
        {
            _logger.LogWarning($"DeleteAsync: Lesson {id} not owned by user {userId}");
            throw new UnauthorizedAccessException($"Lesson {id} not owned by user");
        }

        await _lessonRepository.DeleteAsync(id);

        _logger.LogInformation($"DeleteAsync: Deleted lesson {id} for user {userId}");
    }
   
    public async Task<List<LessonResource>> GetLessonsBySubtopic(int subTopicId, int userId, ArchiveFilter filter = ArchiveFilter.Active)
    {
        _logger.LogDebug("Fetching lessons by SubTopic ID: {SubTopicId} for User ID: {UserId}, Filter: {Filter}", subTopicId, userId, filter);
        var query = _lessonRepository.GetBySubTopicId(subTopicId, true)
            .Where(l => l.UserId == userId);

        query = filter switch
        {
            ArchiveFilter.Active => query.Where(l => !l.Archived),
            ArchiveFilter.Archived => query.Where(l => l.Archived),
            ArchiveFilter.Both => query,
            _ => throw new ArgumentOutOfRangeException(nameof(filter), "Invalid filter value")
        };

        var lessons = await query
            .OrderBy(l => l.SortOrder)
            .ProjectTo<LessonResource>(_mapper.ConfigurationProvider)
            .ToListAsync();

        _logger.LogDebug("Fetched {Count} lessons for SubTopic ID: {SubTopicId}, User ID: {UserId}", lessons.Count, subTopicId, userId);
        return lessons;
    }

    public async Task<List<LessonResource>> GetLessonsByTopic(int topicId, int userId, ArchiveFilter filter = ArchiveFilter.Active)
    {
        _logger.LogDebug("Fetching lessons by Topic ID: {TopicId} for User ID: {UserId}, Filter: {Filter}", topicId, userId, filter);
        var query = _lessonRepository.GetByTopicId(topicId, true)
            .Where(l => l.UserId == userId);

        query = filter switch
        {
            ArchiveFilter.Active => query.Where(l => !l.Archived),
            ArchiveFilter.Archived => query.Where(l => l.Archived),
            ArchiveFilter.Both => query,
            _ => throw new ArgumentOutOfRangeException(nameof(filter), "Invalid filter value")
        };

        var lessons = await query
            .OrderBy(l => l.SortOrder)
            .ProjectTo<LessonResource>(_mapper.ConfigurationProvider)
            .ToListAsync();

        _logger.LogDebug("Fetched {Count} lessons for Topic ID: {TopicId}, User ID: {UserId}", lessons.Count, topicId, userId);
        return lessons;
    }

    public async Task UpdateSortOrderAsync(int lessonId, int sortOrder, int userId)
    {
        _logger.LogDebug("Updating sort order for Lesson ID: {LessonId} to {SortOrder} for User ID: {UserId}", lessonId, sortOrder, userId);

        var lesson = await _lessonRepository.GetByIdAsync(lessonId);
        if (lesson == null)
        {
            _logger.LogError("Lesson with ID {LessonId} not found", lessonId);
            throw new ArgumentException("Lesson not found");
        }

        // Ownership validation - moved from controller to service
        if (lesson.UserId != userId)
        {
            _logger.LogWarning("User ID {UserId} attempted to update sort order for lesson ID {LessonId} owned by another user", userId, lessonId);
            throw new UnauthorizedAccessException("Lesson not owned by user");
        }

        lesson.SortOrder = sortOrder;
        await _lessonRepository.UpdateAsync(lesson);
        _logger.LogInformation("Sort order updated for Lesson ID: {LessonId} to {SortOrder} by User ID: {UserId}", lessonId, sortOrder, userId);
    }

    public async Task AddAttachmentAsync(int lessonId, int attachmentId, int userId)
    {
        _logger.LogDebug("Adding attachment ID: {AttachmentId} to Lesson ID: {LessonId} for User ID: {UserId}", attachmentId, lessonId, userId);

        // Verify lesson exists and is owned by user
        var lesson = await _lessonRepository.GetByIdAsync(lessonId);
        if (lesson == null)
        {
            _logger.LogError("Lesson with ID {LessonId} not found", lessonId);
            throw new ArgumentException("Lesson not found");
        }

        if (lesson.UserId != userId)
        {
            _logger.LogWarning("User ID {UserId} attempted to add attachment to lesson ID {LessonId} owned by another user", userId, lessonId);
            throw new UnauthorizedAccessException("Lesson not owned by user");
        }

        try
        {
            await _lessonRepository.AddAttachmentAsync(lessonId, attachmentId);
            _logger.LogInformation("Attachment ID: {AttachmentId} added to lesson with ID: {LessonId} by User ID: {UserId}", attachmentId, lessonId, userId);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Failed to add attachment ID: {AttachmentId} to lesson ID: {LessonId}", attachmentId, lessonId);
            throw;
        }
    }

    public async Task RemoveAttachmentAsync(int lessonId, int attachmentId, int userId)
    {
        _logger.LogDebug("Removing attachment ID: {AttachmentId} from Lesson ID: {LessonId} for User ID: {UserId}", attachmentId, lessonId, userId);

        // Verify lesson exists and is owned by user
        var lesson = await _lessonRepository.GetByIdAsync(lessonId);
        if (lesson == null)
        {
            _logger.LogError("Lesson with ID {LessonId} not found", lessonId);
            throw new ArgumentException("Lesson not found");
        }

        if (lesson.UserId != userId)
        {
            _logger.LogWarning("User ID {UserId} attempted to remove attachment from lesson ID {LessonId} owned by another user", userId, lessonId);
            throw new UnauthorizedAccessException("Lesson not owned by user");
        }

        try
        {
            await _lessonRepository.RemoveAttachmentAsync(lessonId, attachmentId);
            _logger.LogInformation("Attachment ID: {AttachmentId} removed from lesson with ID: {LessonId} by User ID: {UserId}", attachmentId, lessonId, userId);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Failed to remove attachment ID: {AttachmentId} from lesson ID: {LessonId}", attachmentId, lessonId);
            throw;
        }
    }

    public async Task MoveLessonAsync(int lessonId, int? newSubTopicId, int? newTopicId, int userId)
    {
        _logger.LogDebug("Moving Lesson ID: {LessonId} to SubTopic ID: {NewSubTopicId} or Topic ID: {NewTopicId} for User ID: {UserId}",
            lessonId, newSubTopicId, newTopicId, userId);

        var lesson = await _lessonRepository.GetByIdAsync(lessonId, q => q.Include(l => l.SubTopic).Include(l => l.Topic));
        if (lesson == null)
        {
            _logger.LogError("Lesson with ID {LessonId} not found", lessonId);
            throw new ArgumentException("Lesson not found");
        }

        // Ownership validation - moved from controller to service
        if (lesson.UserId != userId)
        {
            _logger.LogWarning("User ID {UserId} attempted to move lesson ID {LessonId} owned by another user", userId, lessonId);
            throw new UnauthorizedAccessException("Lesson not owned by user");
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

        int sortOrder;
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
            sortOrder = (await _lessonRepository.GetBySubTopicId(newSubTopicId.Value).MaxAsync(l => (int?)l.SortOrder) ?? -1) + 1;
        }
        else
        {
            var topicExists = await _lessonRepository.GetByTopicId(newTopicId.Value).AnyAsync();
            if (!topicExists)
            {
                var topic = await _topicRepository.GetByIdAsync(newTopicId.Value);
                if (topic == null)
                {
                    _logger.LogError("Topic with ID {TopicId} not found", newTopicId);
                    throw new ArgumentException("Topic not found");
                }
            }
            lesson.TopicId = newTopicId;
            lesson.SubTopicId = null;
            sortOrder = (await _lessonRepository.GetByTopicId(newTopicId.Value).MaxAsync(l => (int?)l.SortOrder) ?? -1) + 1;
        }

        lesson.SortOrder = sortOrder;
        await _lessonRepository.UpdateAsync(lesson);
        _logger.LogInformation("Lesson ID: {LessonId} moved to SubTopic ID: {NewSubTopicId} or Topic ID: {NewTopicId} with SortOrder: {SortOrder} by User ID: {UserId}",
            lessonId, newSubTopicId, newTopicId, sortOrder, userId);
    }

    public async Task AddStandardToLessonAsync(int lessonId, int standardId, int userId)
    {
        _logger.LogDebug("Adding standard ID: {StandardId} to Lesson ID: {LessonId} for User ID: {UserId}", standardId, lessonId, userId);

        var lesson = await _lessonRepository.GetByIdAsync(lessonId);
        if (lesson == null)
        {
            _logger.LogError("Lesson with ID {LessonId} not found", lessonId);
            throw new ArgumentException("Lesson not found");
        }

        // Ownership validation
        if (lesson.UserId != userId)
        {
            _logger.LogWarning("User ID {UserId} attempted to add standard to lesson ID {LessonId} owned by another user", userId, lessonId);
            throw new UnauthorizedAccessException("Lesson not owned by user");
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
            _logger.LogInformation("Standard ID: {StandardId} added to lesson with ID: {LessonId} by User ID: {UserId}", standardId, lessonId, userId);
        }
        else
        {
            _logger.LogDebug("Standard ID: {StandardId} already exists in lesson with ID: {LessonId}", standardId, lessonId);
        }
    }

    public async Task RemoveStandardFromLessonAsync(int lessonId, int standardId, int userId)
    {
        _logger.LogDebug("Removing standard ID: {StandardId} from Lesson ID: {LessonId} for User ID: {UserId}", standardId, lessonId, userId);

        var lesson = await _lessonRepository.GetByIdAsync(lessonId);
        if (lesson == null)
        {
            _logger.LogError("Lesson with ID {LessonId} not found", lessonId);
            throw new ArgumentException("Lesson not found");
        }

        // Ownership validation
        if (lesson.UserId != userId)
        {
            _logger.LogWarning("User ID {UserId} attempted to remove standard from lesson ID {LessonId} owned by another user", userId, lessonId);
            throw new UnauthorizedAccessException("Lesson not owned by user");
        }

        var lessonStandard = lesson.LessonStandards.FirstOrDefault(ls => ls.StandardId == standardId);
        if (lessonStandard != null)
        {
            lesson.LessonStandards.Remove(lessonStandard);
            await _lessonRepository.UpdateAsync(lesson);
            _logger.LogInformation("Standard ID: {StandardId} removed from lesson with ID: {LessonId} by User ID: {UserId}", standardId, lessonId, userId);
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
            UserId = userId,
            Visibility = originalLesson.Visibility,
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
}
using AutoMapper;
using LessonTree.BLL.Service;
using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
using LessonTree.Models.DTO;
using LessonTree.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class TopicService : ITopicService
{
    private readonly ITopicRepository _topicRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ISubTopicRepository _subTopicRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<TopicService> _logger;

    public TopicService(
        ITopicRepository topicRepository,
        ICourseRepository courseRepository,
        ISubTopicRepository subTopicRepository,
        ILessonRepository lessonRepository,
        IMapper mapper,
        ILogger<TopicService> logger)
    {
        _topicRepository = topicRepository;
        _courseRepository = courseRepository;
        _subTopicRepository = subTopicRepository;
        _lessonRepository = lessonRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TopicResource> GetByIdAsync(int id, int userId)
    {
        _logger.LogDebug("Fetching topic by ID: {TopicId} for User ID: {UserId}", id, userId);
        var topic = await _topicRepository.GetByIdAsync(id, q => q
            .Include(t => t.SubTopics)
            .ThenInclude(s => s.Lessons)
            .ThenInclude(l => l.LessonAttachments)
            .ThenInclude(ld => ld.Attachment)
            .Include(t => t.Lessons)
            .ThenInclude(l => l.LessonAttachments)
            .ThenInclude(ld => ld.Attachment));

        if (topic == null || topic.UserId != userId)
        {
            _logger.LogWarning("Topic with ID {TopicId} not found or not owned by User ID {UserId}", id, userId);
            throw new KeyNotFoundException($"Topic with ID {id} not found or not owned by user.");
        }

        _logger.LogDebug("Topic with ID {TopicId} found. Title: {Title}, SubTopicCount: {SubTopicCount}, LessonCount: {LessonCount}",
            topic.Id, topic.Title, topic.SubTopics?.Count ?? 0, topic.Lessons?.Count ?? 0);

        return _mapper.Map<TopicResource>(topic);
    }

    public async Task<List<TopicResource>> GetAllAsync(int userId, ArchiveFilter filter = ArchiveFilter.Active)
    {
        _logger.LogDebug("Fetching all topics for User ID: {UserId}, Filter: {Filter}", userId, filter);
        var query = _topicRepository.GetAll(q => q
            .Where(t => t.UserId == userId)
            .Include(t => t.SubTopics)
            .Include(t => t.Lessons));

        query = filter switch
        {
            ArchiveFilter.Active => query.Where(t => !t.Archived),
            ArchiveFilter.Archived => query.Where(t => t.Archived),
            ArchiveFilter.Both => query,
            _ => throw new ArgumentOutOfRangeException(nameof(filter), "Invalid filter value")
        };

        var topics = await query.ToListAsync();
        return _mapper.Map<List<TopicResource>>(topics ?? new List<Topic>());
    }

    // Add SortOrder method
    public async Task UpdateSortOrderAsync(int topicId, int sortOrder, int userId)
    {
        _logger.LogDebug("Updating sort order for Topic ID: {TopicId} to {SortOrder} for User ID: {UserId}", topicId, sortOrder, userId);

        var topic = await _topicRepository.GetByIdAsync(topicId);
        if (topic == null)
        {
            _logger.LogError("Topic with ID {TopicId} not found", topicId);
            throw new ArgumentException("Topic not found");
        }

        // Ownership validation - moved from controller to service
        if (topic.UserId != userId)
        {
            _logger.LogWarning("User ID {UserId} attempted to update sort order for topic ID {TopicId} owned by another user", userId, topicId);
            throw new UnauthorizedAccessException("Topic not owned by user");
        }

        topic.SortOrder = sortOrder;
        await _topicRepository.UpdateAsync(topic);
        _logger.LogInformation("Sort order updated for Topic ID: {TopicId} to {SortOrder} by User ID: {UserId}", topicId, sortOrder, userId);
    }

    // Update Get methods to sort by SortOrder
    public async Task<List<TopicResource>> GetTopicsByCourseAsync(int courseId, int userId, ArchiveFilter filter = ArchiveFilter.Active)
    {
        _logger.LogDebug("Fetching topics for Course ID: {CourseId}, User ID: {UserId}, Filter: {Filter}", courseId, userId, filter);
        var query = _topicRepository.GetAll(q => q
            .Where(t => t.CourseId == courseId && t.UserId == userId)
            .Include(t => t.SubTopics)
            .Include(t => t.Lessons));

        query = filter switch
        {
            ArchiveFilter.Active => query.Where(t => !t.Archived),
            ArchiveFilter.Archived => query.Where(t => t.Archived),
            ArchiveFilter.Both => query,
            _ => throw new ArgumentOutOfRangeException(nameof(filter), "Invalid filter value")
        };

        var topics = await query
            .OrderBy(t => t.SortOrder) // Sort by SortOrder
            .ToListAsync();
        return _mapper.Map<List<TopicResource>>(topics ?? new List<Topic>());
    }

    public async Task<int> AddAsync(TopicCreateResource topicCreateResource, int userId)
    {
        _logger.LogDebug("Adding topic: {Title} for User ID: {UserId}", topicCreateResource.Title, userId);
        var topic = _mapper.Map<Topic>(topicCreateResource);
        topic.UserId = userId;
        var createdTopicId = await _topicRepository.AddAsync(topic);
        _logger.LogInformation("Topic added with ID: {TopicId}", createdTopicId);
        return createdTopicId;
    }

    public async Task<TopicResource> UpdateAsync(TopicUpdateResource topicUpdateResource, int userId)
    {
        _logger.LogDebug("Updating topic with ID: {TopicId} for User ID: {UserId}", topicUpdateResource.Id, userId);
        var existingTopic = await _topicRepository.GetByIdAsync(topicUpdateResource.Id);
        if (existingTopic == null)
        {
            _logger.LogWarning("Topic with ID {TopicId} not found for update", topicUpdateResource.Id);
            throw new ArgumentException("Topic not found");
        }

        // Verify ownership
        if (existingTopic.UserId != userId)
        {
            _logger.LogWarning("User ID {UserId} attempted to update topic ID {TopicId} owned by another user", userId, topicUpdateResource.Id);
            throw new UnauthorizedAccessException("Topic not owned by user");
        }

        _mapper.Map(topicUpdateResource, existingTopic);
        await _topicRepository.UpdateAsync(existingTopic);
        _logger.LogInformation("Topic updated with ID: {TopicId}", existingTopic.Id);

        // Return the updated entity
        return await GetByIdAsync(existingTopic.Id, userId);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        _logger.LogDebug("Deleting topic with ID: {TopicId} for User ID: {UserId}", id, userId);

        var topic = await _topicRepository.GetByIdAsync(id, q => q
            .Include(t => t.SubTopics)
            .ThenInclude(s => s.Lessons)
            .Include(t => t.Lessons));

        if (topic == null)
        {
            _logger.LogWarning("Cannot delete topic with ID {TopicId} because it was not found", id);
            throw new ArgumentException($"Topic with ID {id} not found");
        }

        // Ownership validation - moved from controller to service
        if (topic.UserId != userId)
        {
            _logger.LogWarning("User ID {UserId} attempted to delete topic ID {TopicId} owned by another user", userId, id);
            throw new UnauthorizedAccessException("Topic not owned by user");
        }

        // Log the scope of the deletion for transparency
        _logger.LogDebug("Topic ID: {TopicId} has {SubTopicCount} SubTopics and {LessonCount} direct Lessons to be deleted",
            id, topic.SubTopics?.Count ?? 0, topic.Lessons?.Count ?? 0);

        foreach (var subTopic in topic.SubTopics)
        {
            _logger.LogDebug("SubTopic ID: {SubTopicId} has {LessonCount} Lessons to be deleted",
                subTopic.Id, subTopic.Lessons?.Count ?? 0);
        }

        await _topicRepository.DeleteAsync(id);
        _logger.LogInformation("Topic deleted with ID: {TopicId} by User ID: {UserId}, including all associated SubTopics and Lessons", id, userId);
    }

    public async Task MoveTopicAsync(int topicId, int newCourseId, int userId)
    {
        _logger.LogDebug("Moving Topic ID: {TopicId} to Course ID: {NewCourseId} for User ID: {UserId}", topicId, newCourseId, userId);
        var topic = await _topicRepository.GetByIdAsync(topicId, q => q
            .Include(t => t.SubTopics).ThenInclude(s => s.Lessons)
            .Include(t => t.Lessons));
        if (topic == null || topic.UserId != userId)
        {
            _logger.LogError("Topic with ID {TopicId} not found or not owned by User ID {UserId}", topicId, userId);
            throw new ArgumentException("Topic not found or not owned by user");
        }

        var newCourse = await _courseRepository.GetByIdAsync(newCourseId);
        if (newCourse == null)
        {
            _logger.LogError("Course with ID {CourseId} not found", newCourseId);
            throw new ArgumentException("Course not found");
        }

        topic.CourseId = newCourseId;
        await _topicRepository.UpdateAsync(topic);
        _logger.LogInformation("Moved Topic ID: {TopicId} to Course ID: {NewCourseId}", topicId, newCourseId);
    }

    public async Task<TopicResource> CopyTopicAsync(int topicId, int newCourseId, int userId)
    {
        _logger.LogDebug("Copying Topic ID: {TopicId} to Course ID: {NewCourseId} for User ID: {UserId}", topicId, newCourseId, userId);
        var originalTopic = await _topicRepository.GetByIdAsync(topicId, q => q
            .Include(t => t.SubTopics).ThenInclude(s => s.Lessons).ThenInclude(l => l.LessonAttachments).ThenInclude(ld => ld.Attachment)
            .Include(t => t.SubTopics).ThenInclude(s => s.Lessons).ThenInclude(l => l.LessonStandards)
            .Include(t => t.Lessons).ThenInclude(l => l.LessonAttachments).ThenInclude(ld => ld.Attachment)
            .Include(t => t.Lessons).ThenInclude(l => l.LessonStandards));

        if (originalTopic == null)
        {
            _logger.LogError("Topic with ID {TopicId} not found", topicId);
            throw new ArgumentException("Topic not found");
        }

        var newTopic = new Topic
        {
            Title = originalTopic.Title,
            Description = originalTopic.Description,
            CourseId = newCourseId,
            UserId = userId,
            Visibility = originalTopic.Visibility,
            SubTopics = originalTopic.SubTopics.Select(originalSubTopic => new SubTopic
            {
                Title = originalSubTopic.Title,
                Description = originalSubTopic.Description,
                UserId = userId,
                Visibility = originalSubTopic.Visibility,
                Lessons = originalSubTopic.Lessons.Select(originalLesson => new Lesson
                {
                    Title = originalLesson.Title,
                    Objective = originalLesson.Objective,
                    UserId = userId,
                    Visibility = originalLesson.Visibility,
                    LessonAttachments = originalLesson.LessonAttachments.Select(ld => new LessonAttachment
                    {
                        AttachmentId = ld.AttachmentId
                    }).ToList(),
                    LessonStandards = originalLesson.LessonStandards.Select(ls => new LessonStandard
                    {
                        StandardId = ls.StandardId
                    }).ToList()
                }).ToList()
            }).ToList(),
            Lessons = originalTopic.Lessons.Select(originalLesson => new Lesson
            {
                Title = originalLesson.Title,
                Objective = originalLesson.Objective,
                UserId = userId,
                Visibility = originalLesson.Visibility,
                LessonAttachments = originalLesson.LessonAttachments.Select(ld => new LessonAttachment
                {
                    AttachmentId = ld.AttachmentId
                }).ToList(),
                LessonStandards = originalLesson.LessonStandards.Select(ls => new LessonStandard
                {
                    StandardId = ls.StandardId
                }).ToList()
            }).ToList()
        };

        await _topicRepository.AddAsync(newTopic);
        _logger.LogInformation("Copied Topic ID: {OriginalTopicId} to new Topic ID: {NewTopicId} under Course ID: {NewCourseId} by User ID: {UserId}",
            topicId, newTopic.Id, newCourseId, userId);

        return _mapper.Map<TopicResource>(newTopic);
    }
}
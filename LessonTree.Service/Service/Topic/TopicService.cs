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
        _logger.LogInformation($"GetByIdAsync: Fetching topic {id} for user {userId}");

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
            _logger.LogWarning($"GetByIdAsync: Topic {id} not found or not owned by user {userId}");
            throw new KeyNotFoundException($"Topic {id} not found or not owned by user");
        }

        _logger.LogInformation($"GetByIdAsync: Found topic {id} '{topic.Title}' for user {userId} - SubTopics: {topic.SubTopics?.Count ?? 0}, Lessons: {topic.Lessons?.Count ?? 0}");
        return _mapper.Map<TopicResource>(topic);
    }

    public async Task<List<TopicResource>> GetAllAsync(int userId, ArchiveFilter filter = ArchiveFilter.Active)
    {
        _logger.LogInformation($"GetAllAsync: Fetching topics for user {userId}, filter: {filter}");

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

        _logger.LogInformation($"GetAllAsync: Found {topics.Count} topics for user {userId}");
        return _mapper.Map<List<TopicResource>>(topics ?? new List<Topic>());
    }

    public async Task<int> AddAsync(TopicCreateResource topicCreateResource, int userId)
    {
        _logger.LogInformation($"AddAsync: Creating topic '{topicCreateResource.Title}' for user {userId}");

        var topic = _mapper.Map<Topic>(topicCreateResource);
        topic.UserId = userId;

        // ✅ FIXED: Use existing repository method
        topic.SortOrder = await _topicRepository.GetNextSortOrderForCourseAsync(topicCreateResource.CourseId);

        var createdTopicId = await _topicRepository.AddAsync(topic);

        _logger.LogInformation($"AddAsync: Created topic {createdTopicId} '{topicCreateResource.Title}' with sort order {topic.SortOrder} for user {userId}");
        return createdTopicId;
    }


    public async Task<TopicResource> UpdateAsync(TopicUpdateResource topicUpdateResource, int userId)
    {
        _logger.LogInformation($"UpdateAsync: Updating topic {topicUpdateResource.Id} for user {userId}");

        var existingTopic = await _topicRepository.GetByIdAsync(topicUpdateResource.Id);
        if (existingTopic == null)
        {
            _logger.LogInformation($"UpdateAsync: Topic {topicUpdateResource.Id} not found");
            throw new ArgumentException($"Topic {topicUpdateResource.Id} not found");
        }

        // Verify ownership
        if (existingTopic.UserId != userId)
        {
            _logger.LogWarning($"UpdateAsync: Topic {topicUpdateResource.Id} not owned by user {userId}");
            throw new UnauthorizedAccessException($"Topic {topicUpdateResource.Id} not owned by user");
        }

        _mapper.Map(topicUpdateResource, existingTopic);
        await _topicRepository.UpdateAsync(existingTopic);

        _logger.LogInformation($"UpdateAsync: Updated topic {existingTopic.Id} for user {userId}");

        // Return the updated entity
        return await GetByIdAsync(existingTopic.Id, userId);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        _logger.LogInformation($"DeleteAsync: Deleting topic {id} for user {userId}");

        var topic = await _topicRepository.GetByIdAsync(id, q => q
            .Include(t => t.SubTopics)
            .ThenInclude(s => s.Lessons)
            .Include(t => t.Lessons));

        if (topic == null)
        {
            _logger.LogInformation($"DeleteAsync: Topic {id} not found");
            throw new ArgumentException($"Topic {id} not found");
        }

        // Ownership validation - moved from controller to service
        if (topic.UserId != userId)
        {
            _logger.LogWarning($"DeleteAsync: Topic {id} not owned by user {userId}");
            throw new UnauthorizedAccessException($"Topic {id} not owned by user");
        }

        // Log the scope of the deletion for transparency
        _logger.LogDebug($"DeleteAsync: Topic {id} has {topic.SubTopics?.Count ?? 0} SubTopics and {topic.Lessons?.Count ?? 0} direct Lessons to be deleted");

        foreach (var subTopic in topic.SubTopics)
        {
            _logger.LogDebug($"DeleteAsync: SubTopic {subTopic.Id} has {subTopic.Lessons?.Count ?? 0} Lessons to be deleted");
        }

        await _topicRepository.DeleteAsync(id);

        _logger.LogInformation($"DeleteAsync: Deleted topic {id} for user {userId} (including all associated SubTopics and Lessons)");
    }

    // Add SortOrder method
    // Add SortOrder method
    public async Task UpdateSortOrderAsync(int topicId, int sortOrder)
    {
        _logger.LogDebug("Updating sort order for Topic ID: {TopicId} to {SortOrder}", topicId, sortOrder);
        var topic = await _topicRepository.GetByIdAsync(topicId);
        if (topic == null)
        {
            _logger.LogError("Topic with ID {TopicId} not found", topicId);
            throw new ArgumentException("Topic not found");
        }

        topic.SortOrder = sortOrder;
        await _topicRepository.UpdateAsync(topic);
        _logger.LogInformation("Sort order updated for Topic ID: {TopicId} to {SortOrder}", topicId, sortOrder);
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

    public async Task<TopicResource> MoveTopicAsync(TopicMoveResource moveResource, int userId)
    {
        _logger.LogInformation($"MoveTopicAsync: Moving topic {moveResource.TopicId} for user {userId}");

        // Validate topic exists and user owns it
        var topic = await _topicRepository.GetByIdAsync(moveResource.TopicId);
        if (topic == null)
        {
            throw new ArgumentException($"Topic {moveResource.TopicId} not found");
        }
        if (topic.UserId != userId)
        {
            throw new UnauthorizedAccessException($"Topic {moveResource.TopicId} not owned by user {userId}");
        }

        // Validate target course exists and user owns it
        var targetCourse = await _courseRepository.GetByIdAsync(moveResource.NewCourseId);
        if (targetCourse == null)
        {
            throw new ArgumentException($"Target course {moveResource.NewCourseId} not found");
        }
        if (targetCourse.UserId != userId)
        {
            throw new UnauthorizedAccessException($"Target course {moveResource.NewCourseId} not owned by user {userId}");
        }

        // ✅ UPDATED: Route operation based on sibling positioning
        TopicResource result;
        if (moveResource.AfterSiblingId.HasValue)  // ✅ CHANGED: RelativeToId → AfterSiblingId
        {
            // Positional move - delegate to repository for atomic operation
            result = await MoveTopicToPositionAsync(moveResource, userId);
        }
        else
        {
            // Simple move - update course and append to end (first position in empty course)
            result = await MoveTopicSimpleAsync(moveResource, userId);
        }

        _logger.LogInformation($"MoveTopicAsync: Successfully moved topic {moveResource.TopicId}");
        return result;
    }

    private async Task<TopicResource> MoveTopicSimpleAsync(TopicMoveResource moveResource, int userId)
    {
        // Simple move - update course and position at start (SortOrder = 0)
        var topic = await _topicRepository.GetByIdAsync(moveResource.TopicId);
        if (topic == null)
        {
            throw new ArgumentException($"Topic {moveResource.TopicId} not found");
        }

        // Update topic to new course, first position
        topic.CourseId = moveResource.NewCourseId;
        topic.SortOrder = 0; // ✅ First position in empty course

        await _topicRepository.UpdateAsync(topic);

        _logger.LogInformation($"MoveTopicSimpleAsync: Moved topic {moveResource.TopicId} to course {moveResource.NewCourseId} at first position");

        return _mapper.Map<TopicResource>(topic);
    }


    private async Task<TopicResource> MoveTopicToPositionAsync(TopicMoveResource moveResource, int userId)
    {
        // ✅ SIMPLIFIED: Validate sibling topic exists and is in target course
        var siblingTopic = await _topicRepository.GetByIdAsync(moveResource.AfterSiblingId.Value);
        if (siblingTopic == null)
        {
            throw new ArgumentException($"Sibling topic {moveResource.AfterSiblingId.Value} not found");
        }
        if (siblingTopic.CourseId != moveResource.NewCourseId)
        {
            throw new ArgumentException("Sibling topic must be in the target course");
        }

        // ✅ UPDATED: Delegate atomic positioning to repository with sibling approach
        var positionedTopic = await _topicRepository.MoveTopicToPositionAsync(
            moveResource.TopicId,
            moveResource.NewCourseId,
            moveResource.AfterSiblingId.Value  // ✅ SIMPLIFIED: Just pass sibling ID
        );

        return _mapper.Map<TopicResource>(positionedTopic);
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
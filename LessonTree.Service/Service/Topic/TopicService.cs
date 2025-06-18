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

    // **PARTIAL FILE** - TopicService.cs - Logging Standardization (Key Methods)
    // INTEGRATION: Replace the GetByIdAsync, GetAllAsync, AddAsync, UpdateAsync, DeleteAsync methods

    public async Task<SubTopicResource> GetByIdAsync(int id, int userId)
    {
        _logger.LogInformation($"GetByIdAsync: Fetching subtopic {id} for user {userId}");

        var subTopic = await _subTopicRepository.GetByIdAsync(id, q => q
            .Include(s => s.Lessons).ThenInclude(l => l.LessonAttachments).ThenInclude(ld => ld.Attachment));
        if (subTopic == null || subTopic.UserId != userId)
        {
            _logger.LogWarning($"GetByIdAsync: SubTopic {id} not found or not owned by user {userId}");
            throw new KeyNotFoundException($"SubTopic {id} not found or not owned by user");
        }
        if (subTopic.Lessons == null)
        {
            _logger.LogError($"GetByIdAsync: SubTopic {id} has invalid lesson data");
            throw new InvalidOperationException("SubTopic data is in an invalid state");
        }

        _logger.LogInformation($"GetByIdAsync: Found subtopic {id} for user {userId}");
        return _mapper.Map<SubTopicResource>(subTopic);
    }

    public async Task<List<SubTopicResource>> GetAllAsync(int userId, ArchiveFilter filter = ArchiveFilter.Active)
    {
        _logger.LogInformation($"GetAllAsync: Fetching subtopics for user {userId}, filter: {filter}");

        try
        {
            var query = _subTopicRepository.GetAll(q => q
                .Where(s => s.UserId == userId)
                .Include(s => s.Lessons));

            query = filter switch
            {
                ArchiveFilter.Active => query.Where(s => !s.Archived),
                ArchiveFilter.Archived => query.Where(s => s.Archived),
                ArchiveFilter.Both => query,
                _ => throw new ArgumentOutOfRangeException(nameof(filter), "Invalid filter value")
            };

            var subTopics = await query.ToListAsync();

            _logger.LogInformation($"GetAllAsync: Found {subTopics.Count} subtopics for user {userId}");
            return _mapper.Map<List<SubTopicResource>>(subTopics ?? new List<SubTopic>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"GetAllAsync: Failed to retrieve SubTopics for user {userId}");
            throw new InvalidOperationException("Failed to retrieve SubTopics due to a data access error", ex);
        }
    }

    public async Task<int> AddAsync(SubTopicCreateResource subTopicCreateResource, int userId)
    {
        _logger.LogInformation($"AddAsync: Creating subtopic '{subTopicCreateResource.Title}' for user {userId}");

        if (string.IsNullOrWhiteSpace(subTopicCreateResource.Title))
        {
            throw new ArgumentException("Title is required", nameof(subTopicCreateResource.Title));
        }
        var topic = await _topicRepository.GetByIdAsync(subTopicCreateResource.TopicId);
        if (topic == null)
        {
            _logger.LogWarning($"AddAsync: Topic {subTopicCreateResource.TopicId} not found for new SubTopic");
            throw new ArgumentException("The specified Topic does not exist", nameof(subTopicCreateResource.TopicId));
        }
        var subTopic = _mapper.Map<SubTopic>(subTopicCreateResource);
        subTopic.UserId = userId; // Set UserId here
        subTopic.Archived = false; // Default to active on creation
        var createdId = await _subTopicRepository.AddAsync(subTopic);

        _logger.LogInformation($"AddAsync: Created subtopic {createdId} '{subTopicCreateResource.Title}' for user {userId}");
        return createdId;
    }

    public async Task<SubTopicResource> UpdateAsync(SubTopicUpdateResource subTopicUpdateResource, int userId)
    {
        _logger.LogInformation($"UpdateAsync: Updating subtopic {subTopicUpdateResource.Id} for user {userId}");

        // Fetch the existing SubTopic
        var existingSubTopic = await _subTopicRepository.GetByIdAsync(subTopicUpdateResource.Id);
        if (existingSubTopic == null)
        {
            throw new KeyNotFoundException($"SubTopic {subTopicUpdateResource.Id} not found");
        }

        // Verify ownership
        if (existingSubTopic.UserId != userId)
        {
            _logger.LogWarning($"UpdateAsync: SubTopic {subTopicUpdateResource.Id} not owned by user {userId}");
            throw new UnauthorizedAccessException($"SubTopic {subTopicUpdateResource.Id} not owned by user");
        }

        // Check if the SubTopic is default
        if (existingSubTopic.IsDefault)
        {
            _logger.LogWarning($"UpdateAsync: Cannot update default SubTopic {existingSubTopic.Id}");
            throw new InvalidOperationException("Cannot update a default SubTopic");
        }

        // Map the DTO onto the existing entity, leaving TopicId unchanged
        _mapper.Map(subTopicUpdateResource, existingSubTopic);

        // Persist the changes
        await _subTopicRepository.UpdateAsync(existingSubTopic);

        _logger.LogInformation($"UpdateAsync: Updated subtopic {subTopicUpdateResource.Id} for user {userId}");

        // Return the updated entity
        return await GetByIdAsync(existingSubTopic.Id, userId);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        _logger.LogInformation($"DeleteAsync: Deleting subtopic {id} for user {userId}");

        // Fetch the SubTopic to check its properties
        var subTopic = await _subTopicRepository.GetByIdAsync(id);
        if (subTopic == null)
        {
            _logger.LogInformation($"DeleteAsync: SubTopic {id} not found");
            throw new ArgumentException($"SubTopic {id} not found");
        }

        // Ownership validation - moved from controller to service
        if (subTopic.UserId != userId)
        {
            _logger.LogWarning($"DeleteAsync: SubTopic {id} not owned by user {userId}");
            throw new UnauthorizedAccessException($"SubTopic {id} not owned by user");
        }

        // Check if the SubTopic is default
        if (subTopic.IsDefault)
        {
            _logger.LogWarning($"DeleteAsync: Cannot delete default SubTopic {id}");
            throw new InvalidOperationException("Cannot delete a default SubTopic");
        }

        await _subTopicRepository.DeleteAsync(id);

        _logger.LogInformation($"DeleteAsync: Deleted subtopic {id} for user {userId}");
    }
}
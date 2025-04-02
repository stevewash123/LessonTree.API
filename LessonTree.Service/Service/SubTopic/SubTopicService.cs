// Full File
using AutoMapper;
using LessonTree.BLL.Service;
using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
using LessonTree.Models.DTO;
using LessonTree.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class SubTopicService : ISubTopicService
{
    private readonly ISubTopicRepository _subTopicRepository;
    private readonly ITopicRepository _topicRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly ILogger<SubTopicService> _logger;
    private readonly IMapper _mapper;

    public SubTopicService(
        ISubTopicRepository subTopicRepository,
        ITopicRepository topicRepository,
        ILessonRepository lessonRepository,
        ILogger<SubTopicService> logger,
        IMapper mapper)
    {
        _subTopicRepository = subTopicRepository;
        _topicRepository = topicRepository;
        _lessonRepository = lessonRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<SubTopicResource> GetByIdAsync(int id, int userId)
    {
        _logger.LogDebug("Fetching subtopic by ID: {SubTopicId} for User ID: {UserId}", id, userId);
        var subTopic = await _subTopicRepository.GetByIdAsync(id, q => q
            .Include(s => s.Lessons).ThenInclude(l => l.LessonAttachments).ThenInclude(ld => ld.Attachment));
        if (subTopic == null || subTopic.UserId != userId)
        {
            _logger.LogWarning("SubTopic with ID {SubTopicId} not found or not owned by User ID {UserId}", id, userId);
            throw new KeyNotFoundException($"SubTopic with ID {id} not found or not owned by user.");
        }
        if (subTopic.Lessons == null)
        {
            _logger.LogError("SubTopic ID {SubTopicId} has invalid lesson data.", id);
            throw new InvalidOperationException("SubTopic data is in an invalid state.");
        }
        return _mapper.Map<SubTopicResource>(subTopic);
    }

    public async Task<SubTopic?> GetDomainSubTopicByIdAsync(int id)
    {
        _logger.LogDebug("Fetching domain subtopic by ID: {SubTopicId}", id);
        var subTopic = await _subTopicRepository.GetByIdAsync(id);
        if (subTopic == null)
        {
            _logger.LogWarning("Domain subtopic with ID {SubTopicId} not found", id);
        }
        return subTopic;
    }

    public async Task<List<SubTopicResource>> GetAllAsync(int userId, ArchiveFilter filter = ArchiveFilter.Active)
    {
        _logger.LogDebug("Fetching all subtopics for User ID: {UserId}, Filter: {Filter}", userId, filter);
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
            return _mapper.Map<List<SubTopicResource>>(subTopics ?? new List<SubTopic>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve SubTopics for User ID {UserId}", userId);
            throw new InvalidOperationException("Failed to retrieve SubTopics due to a data access error.", ex);
        }
    }

    // Add SortOrder method
    public async Task UpdateSortOrderAsync(int subTopicId, int sortOrder)
    {
        _logger.LogDebug("Updating sort order for SubTopic ID: {SubTopicId} to {SortOrder}", subTopicId, sortOrder);
        var subTopic = await _subTopicRepository.GetByIdAsync(subTopicId);
        if (subTopic == null)
        {
            _logger.LogError("SubTopic with ID {SubTopicId} not found", subTopicId);
            throw new ArgumentException("SubTopic not found");
        }

        subTopic.SortOrder = sortOrder;
        await _subTopicRepository.UpdateAsync(subTopic);
        _logger.LogInformation("Sort order updated for SubTopic ID: {SubTopicId} to {SortOrder}", subTopicId, sortOrder);
    }

    // Update Get methods to sort by SortOrder
    public async Task<List<SubTopicResource>> GetSubtopicsByTopicIdAsync(int topicId, int userId, ArchiveFilter filter = ArchiveFilter.Active)
    {
        _logger.LogDebug("Fetching subtopics for Topic ID: {TopicId}, User ID: {UserId}, Filter: {Filter}", topicId, userId, filter);
        try
        {
            var query = _subTopicRepository.GetAll(q => q
                .Where(s => s.TopicId == topicId && s.UserId == userId)
                .Include(s => s.Lessons));

            query = filter switch
            {
                ArchiveFilter.Active => query.Where(s => !s.Archived),
                ArchiveFilter.Archived => query.Where(s => s.Archived),
                ArchiveFilter.Both => query,
                _ => throw new ArgumentOutOfRangeException(nameof(filter), "Invalid filter value")
            };

            var subTopics = await query
                .OrderBy(s => s.SortOrder) // Sort by SortOrder
                .ToListAsync();
            return _mapper.Map<List<SubTopicResource>>(subTopics ?? new List<SubTopic>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve SubTopics for Topic ID {TopicId}, User ID {UserId}", topicId, userId);
            throw new InvalidOperationException("Failed to retrieve SubTopics due to a data access error.", ex);
        }
    }

    public async Task<int> AddAsync(SubTopicCreateResource subTopicCreateResource, int userId)
    {
        _logger.LogDebug("Adding subtopic: {Title} for User ID: {UserId}", subTopicCreateResource.Title, userId);
        if (string.IsNullOrWhiteSpace(subTopicCreateResource.Title))
        {
            throw new ArgumentException("Title is required", nameof(subTopicCreateResource.Title));
        }
        var topic = await _topicRepository.GetByIdAsync(subTopicCreateResource.TopicId);
        if (topic == null)
        {
            _logger.LogWarning("Topic ID {TopicId} not found for new SubTopic.", subTopicCreateResource.TopicId);
            throw new ArgumentException("The specified Topic does not exist.", nameof(subTopicCreateResource.TopicId));
        }
        var subTopic = _mapper.Map<SubTopic>(subTopicCreateResource);
        subTopic.UserId = userId; // Set UserId here
        subTopic.Archived = false; // Default to active on creation
        var createdId = await _subTopicRepository.AddAsync(subTopic);
        _logger.LogInformation("SubTopic added with ID: {SubTopicId}", createdId);
        return createdId;
    }

    public async Task UpdateAsync(SubTopicUpdateResource subTopicUpdateResource)
    {
        _logger.LogDebug("Attempting to update subtopic: {Title}", subTopicUpdateResource.Title);

        // Fetch the existing SubTopic
        var existingSubTopic = await _subTopicRepository.GetByIdAsync(subTopicUpdateResource.Id);
        if (existingSubTopic == null)
        {
            throw new KeyNotFoundException($"SubTopic with ID {subTopicUpdateResource.Id} not found.");
        }

        // Check if the SubTopic is default
        if (existingSubTopic.IsDefault)
        {
            _logger.LogWarning("Cannot update a default SubTopic. Unable to update default SubTopic with ID {SubTopicId}", existingSubTopic.Id);
            throw new InvalidOperationException("Cannot update a default SubTopic.");
        }

        // Map the DTO onto the existing entity, leaving TopicId unchanged
        _mapper.Map(subTopicUpdateResource, existingSubTopic);
        _logger.LogDebug("Updating subtopic: {Title}", subTopicUpdateResource.Title);

        // Persist the changes
        await _subTopicRepository.UpdateAsync(existingSubTopic);
        _logger.LogInformation("SubTopic updated with ID: {SubTopicId}", subTopicUpdateResource.Id);
    }

    public async Task DeleteAsync(int id)
    {
        _logger.LogDebug("Attempting to delete subtopic with ID: {SubTopicId}", id);

        // Fetch the SubTopic to check its properties
        var subTopic = await _subTopicRepository.GetByIdAsync(id);
        if (subTopic == null)
        {
            _logger.LogWarning("SubTopic with ID {SubTopicId} not found for deletion", id);
            throw new KeyNotFoundException($"SubTopic with ID {id} not found.");
        }

        // Check if the SubTopic is default
        if (subTopic.IsDefault)
        {
            _logger.LogWarning("Attempt to delete default SubTopic with ID {SubTopicId}", id);
            throw new InvalidOperationException("Cannot delete a default SubTopic.");
        }
        _logger.LogDebug("Deleting subtopic with ID: {SubTopicId}", id);
        await _subTopicRepository.DeleteAsync(id);
        _logger.LogInformation("SubTopic deleted with ID: {SubTopicId}", id);
    }

    public async Task MoveSubTopic(int subTopicId, int newTopicId)
    {
        _logger.LogDebug("Moving SubTopic ID: {SubTopicId} to Topic ID: {NewTopicId}", subTopicId, newTopicId);
        var subTopic = await _subTopicRepository.GetByIdAsync(subTopicId, q => q.Include(s => s.Lessons));
        if (subTopic == null)
        {
            _logger.LogError("SubTopic with ID {SubTopicId} not found", subTopicId);
            throw new ArgumentException("SubTopic not found");
        }
        if (subTopic.TopicId == newTopicId)
        {
            _logger.LogWarning("SubTopic ID {SubTopicId} is already in Topic ID {TopicId}", subTopicId, newTopicId);
            throw new InvalidOperationException("SubTopic is already in the specified Topic.");
        }
        var newTopic = await _topicRepository.GetByIdAsync(newTopicId, q => q.Include(t => t.SubTopics));
        if (newTopic == null)
        {
            _logger.LogError("Topic with ID {TopicId} not found", newTopicId);
            throw new ArgumentException("Topic not found");
        }

        // Simplified logic: always move the SubTopic directly to the new Topic
        subTopic.TopicId = newTopicId;
        await _subTopicRepository.UpdateAsync(subTopic);
        _logger.LogInformation("Moved SubTopic ID: {SubTopicId} to Topic ID: {NewTopicId}", subTopic.Id, newTopicId);
    }

    public async Task<SubTopicResource> CopySubTopicAsync(int subTopicId, int newTopicId, int userId)
    {
        _logger.LogDebug("Copying SubTopic ID: {SubTopicId} to Topic ID: {NewTopicId} for User ID: {UserId}", subTopicId, newTopicId, userId);
        var originalSubTopic = await _subTopicRepository.GetByIdAsync(subTopicId, q => q
            .Include(s => s.Lessons).ThenInclude(l => l.LessonAttachments).ThenInclude(ld => ld.Attachment)
            .Include(s => s.Lessons).ThenInclude(l => l.LessonStandards));
        if (originalSubTopic == null)
        {
            _logger.LogError("SubTopic with ID {SubTopicId} not found", subTopicId);
            throw new ArgumentException("SubTopic not found");
        }
        var newTopic = await _topicRepository.GetByIdAsync(newTopicId, q => q.Include(t => t.SubTopics));
        if (newTopic == null)
        {
            _logger.LogError("Topic with ID {TopicId} not found", newTopicId);
            throw new ArgumentException("Topic not found");
        }

        // Create a new SubTopic under the target Topic without lessons initially
        var newSubTopic = new SubTopic
        {
            Title = originalSubTopic.Title,
            Description = originalSubTopic.Description,
            TopicId = newTopicId,
            UserId = userId,
            Visibility = originalSubTopic.Visibility,
            Archived = false // Default to active on creation
        };

        await _subTopicRepository.AddAsync(newSubTopic);

        // Now populate lessons with the correct SubTopicId
        newSubTopic.Lessons = originalSubTopic.Lessons.Select(originalLesson => CopyLesson(newSubTopic.Id, originalLesson, userId)).ToList();
        await _subTopicRepository.UpdateAsync(newSubTopic);

        _logger.LogInformation("Copied SubTopic ID: {OriginalSubTopicId} to new SubTopic ID: {NewSubTopicId} under Topic ID: {NewTopicId} by User ID: {UserId}",
            subTopicId, newSubTopic.Id, newTopicId, userId);

        return _mapper.Map<SubTopicResource>(newSubTopic);
    }

    private static Lesson CopyLesson(int defaultSubTopicId, Lesson originalLesson, int userId)
    {
        return new Lesson
        {
            Title = originalLesson.Title,
            Objective = originalLesson.Objective,
            Level = originalLesson.Level,
            Materials = originalLesson.Materials,
            ClassTime = originalLesson.ClassTime,
            Methods = originalLesson.Methods,
            SpecialNeeds = originalLesson.SpecialNeeds,
            Assessment = originalLesson.Assessment,
            SubTopicId = defaultSubTopicId,
            UserId = userId, // Set to copier’s UserId
            Visibility = originalLesson.Visibility,
            LessonAttachments = originalLesson.LessonAttachments.Select(ld => new LessonAttachment
            {
                AttachmentId = ld.AttachmentId
            }).ToList(),
            LessonStandards = originalLesson.LessonStandards.Select(ls => new LessonStandard
            {
                StandardId = ls.StandardId
            }).ToList()
        };
    }
}
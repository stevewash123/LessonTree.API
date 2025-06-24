// RESPONSIBILITY: Business logic for SubTopic operations, coordinates between controller and repository
// DOES NOT: Handle HTTP concerns or direct data access
// CALLED BY: SubTopicController
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
    public async Task UpdateSortOrderAsync(int subTopicId, int sortOrder, int userId)
    {
        _logger.LogDebug("Updating sort order for SubTopic ID: {SubTopicId} to {SortOrder} for User ID: {UserId}", subTopicId, sortOrder, userId);

        var subTopic = await _subTopicRepository.GetByIdAsync(subTopicId);
        if (subTopic == null)
        {
            _logger.LogError("SubTopic with ID {SubTopicId} not found", subTopicId);
            throw new ArgumentException("SubTopic not found");
        }

        // Ownership validation - moved from controller to service
        if (subTopic.UserId != userId)
        {
            _logger.LogWarning("User ID {UserId} attempted to update sort order for subtopic ID {SubTopicId} owned by another user", userId, subTopicId);
            throw new UnauthorizedAccessException("SubTopic not owned by user");
        }

        subTopic.SortOrder = sortOrder;
        await _subTopicRepository.UpdateAsync(subTopic);
        _logger.LogInformation("Sort order updated for SubTopic ID: {SubTopicId} to {SortOrder} by User ID: {UserId}", subTopicId, sortOrder, userId);
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

    
    public async Task<SubTopicResource> UpdateAsync(SubTopicUpdateResource subTopicUpdateResource, int userId)
    {
        _logger.LogDebug("Attempting to update subtopic: {Title} for User ID: {UserId}", subTopicUpdateResource.Title, userId);

        // Fetch the existing SubTopic
        var existingSubTopic = await _subTopicRepository.GetByIdAsync(subTopicUpdateResource.Id);
        if (existingSubTopic == null)
        {
            throw new KeyNotFoundException($"SubTopic with ID {subTopicUpdateResource.Id} not found.");
        }

        // Verify ownership
        if (existingSubTopic.UserId != userId)
        {
            _logger.LogWarning("User ID {UserId} attempted to update subtopic ID {SubTopicId} owned by another user", userId, subTopicUpdateResource.Id);
            throw new UnauthorizedAccessException("SubTopic not owned by user");
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

        // Return the updated entity
        return await GetByIdAsync(existingSubTopic.Id, userId);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        _logger.LogDebug("Attempting to delete subtopic with ID: {SubTopicId} for User ID: {UserId}", id, userId);

        // Fetch the SubTopic to check its properties
        var subTopic = await _subTopicRepository.GetByIdAsync(id);
        if (subTopic == null)
        {
            _logger.LogWarning("SubTopic with ID {SubTopicId} not found for deletion", id);
            throw new ArgumentException($"SubTopic with ID {id} not found.");
        }

        // Ownership validation - moved from controller to service
        if (subTopic.UserId != userId)
        {
            _logger.LogWarning("User ID {UserId} attempted to delete subtopic ID {SubTopicId} owned by another user", userId, id);
            throw new UnauthorizedAccessException("SubTopic not owned by user");
        }

        // Check if the SubTopic is default
        if (subTopic.IsDefault)
        {
            _logger.LogWarning("Attempt to delete default SubTopic with ID {SubTopicId} by User ID {UserId}", id, userId);
            throw new InvalidOperationException("Cannot delete a default SubTopic.");
        }

        _logger.LogDebug("Deleting subtopic with ID: {SubTopicId} for User ID: {UserId}", id, userId);
        await _subTopicRepository.DeleteAsync(id);
        _logger.LogInformation("SubTopic deleted with ID: {SubTopicId} by User ID: {UserId}", id, userId);
    }

    public async Task<SubTopicResource> MoveSubTopicAsync(SubTopicMoveResource moveResource, int userId)
    {
        _logger.LogInformation($"MoveSubTopicAsync: Moving subtopic {moveResource.SubTopicId} for user {userId}");

        // Validate subtopic exists and user owns it
        var subTopic = await _subTopicRepository.GetByIdAsync(moveResource.SubTopicId);
        if (subTopic == null)
        {
            throw new ArgumentException($"SubTopic {moveResource.SubTopicId} not found");
        }

        if (subTopic.UserId != userId)
        {
            throw new UnauthorizedAccessException($"SubTopic {moveResource.SubTopicId} not owned by user {userId}");
        }

        // Validate target topic exists and user owns it
        var targetTopic = await _topicRepository.GetByIdAsync(moveResource.NewTopicId);
        if (targetTopic == null)
        {
            throw new ArgumentException($"Target topic {moveResource.NewTopicId} not found");
        }

        if (targetTopic.UserId != userId)
        {
            throw new UnauthorizedAccessException($"Target topic {moveResource.NewTopicId} not owned by user {userId}");
        }

        // Route operation: positional move vs simple move
        SubTopicResource result;

        if (moveResource.RelativeToId.HasValue)
        {
            // Positional move - delegate to repository for atomic operation
            result = await MoveSubTopicToPositionAsync(moveResource, userId);
        }
        else
        {
            // Simple move - update topic and append to end
            result = await MoveSubTopicSimpleAsync(moveResource, userId);
        }

        _logger.LogInformation($"MoveSubTopicAsync: Successfully moved subtopic {moveResource.SubTopicId}");
        return result;
    }

    private async Task<SubTopicResource> MoveSubTopicToPositionAsync(SubTopicMoveResource moveResource, int userId)
    {
        // Validate relative object exists and is in target topic
        if (moveResource.RelativeToType == "SubTopic")
        {
            var relativeSubTopic = await _subTopicRepository.GetByIdAsync(moveResource.RelativeToId.Value);
            if (relativeSubTopic == null)
            {
                throw new ArgumentException($"Relative subtopic {moveResource.RelativeToId.Value} not found");
            }

            if (relativeSubTopic.TopicId != moveResource.NewTopicId)
            {
                throw new ArgumentException("Relative subtopic must be in the target topic");
            }
        }
        else if (moveResource.RelativeToType == "Lesson")
        {
            var relativeLesson = await _lessonRepository.GetByIdAsync(moveResource.RelativeToId.Value);
            if (relativeLesson == null)
            {
                throw new ArgumentException($"Relative lesson {moveResource.RelativeToId.Value} not found");
            }

            // Check if lesson is in target topic (either direct or through subtopic)
            if (relativeLesson.TopicId != moveResource.NewTopicId &&
                relativeLesson.SubTopic?.TopicId != moveResource.NewTopicId)
            {
                throw new ArgumentException("Relative lesson must be in the target topic");
            }
        }
        else
        {
            throw new ArgumentException("RelativeToType must be 'SubTopic' or 'Lesson' for subtopic positioning");
        }

        // Delegate atomic positioning to repository
        var positionedSubTopic = await _subTopicRepository.MoveSubTopicToPositionAsync(
            moveResource.SubTopicId,
            moveResource.NewTopicId,
            moveResource.RelativeToId.Value,
            moveResource.Position ?? "after",
            moveResource.RelativeToType ?? "SubTopic"
        );

        return _mapper.Map<SubTopicResource>(positionedSubTopic);
    }

    private async Task<SubTopicResource> MoveSubTopicSimpleAsync(SubTopicMoveResource moveResource, int userId)
    {
        // Get subtopic and update topic
        var subTopic = await _subTopicRepository.GetByIdAsync(moveResource.SubTopicId);

        // If moving to different topic, get next sort order
        if (subTopic.TopicId != moveResource.NewTopicId)
        {
            var maxSortOrder = await _subTopicRepository.GetMaxSortOrderInTopicAsync(moveResource.NewTopicId);
            subTopic.SortOrder = maxSortOrder + 1;
        }

        subTopic.TopicId = moveResource.NewTopicId;

        await _subTopicRepository.UpdateAsync(subTopic);

        return _mapper.Map<SubTopicResource>(subTopic);
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
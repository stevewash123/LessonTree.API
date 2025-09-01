// RESPONSIBILITY: Business logic for SubTopic operations, coordinates between controller and repository
// DOES NOT: Handle HTTP concerns or direct data access
// CALLED BY: SubTopicController
using AutoMapper;
using LessonTree.BLL.Service;
using LessonTree.BLL.Services;
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
    private readonly IScheduleRepository _scheduleRepository;
    private readonly ILogger<SubTopicService> _logger;
    private readonly IMapper _mapper;
    private readonly IScheduleGenerationService _scheduleGenerationService;

    public SubTopicService(
        ISubTopicRepository subTopicRepository,
        ITopicRepository topicRepository,
        ILessonRepository lessonRepository,
        IScheduleRepository scheduleRepository,
        ILogger<SubTopicService> logger,
        IMapper mapper,
        IScheduleGenerationService scheduleGenerationService)
    {
        _subTopicRepository = subTopicRepository;
        _topicRepository = topicRepository;
        _lessonRepository = lessonRepository;
        _scheduleRepository = scheduleRepository;
        _logger = logger;
        _mapper = mapper;
        _scheduleGenerationService = scheduleGenerationService;
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
        subTopic.UserId = userId;
        subTopic.Archived = false;

        // ✅ FIXED: Use existing repository method
        subTopic.SortOrder = await _subTopicRepository.GetNextSortOrderForTopicAsync(subTopicCreateResource.TopicId);

        var createdId = await _subTopicRepository.AddAsync(subTopic);

        _logger.LogInformation("SubTopic added with ID: {SubTopicId} and sort order {SortOrder}", createdId, subTopic.SortOrder);
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
        _logger.LogInformation($"🔍 SUBTOPIC MOVE PARAMETERS: SubTopicId={moveResource.SubTopicId}, NewTopicId={moveResource.NewTopicId}, RelativeToId={moveResource.RelativeToId}, Position={moveResource.Position}, RelativeToType={moveResource.RelativeToType}, AfterSiblingId={moveResource.AfterSiblingId}");

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

        // ✅ UPDATED: Route operation based on positioning parameters
        SubTopicResource result;
        if (moveResource.RelativeToId.HasValue && !string.IsNullOrEmpty(moveResource.Position) && !string.IsNullOrEmpty(moveResource.RelativeToType))
        {
            _logger.LogInformation($"🎯 Using POSITIONING logic: RelativeToId={moveResource.RelativeToId}, Position={moveResource.Position}, RelativeToType={moveResource.RelativeToType}");
            // Positional move - use new positioning parameters
            result = await MoveSubTopicWithPositioningAsync(moveResource, userId);
        }
        else if (moveResource.AfterSiblingId.HasValue)  
        {
            _logger.LogInformation($"🎯 Using LEGACY logic: AfterSiblingId={moveResource.AfterSiblingId}");
            // Legacy positional move - delegate to repository for atomic operation
            result = await MoveSubTopicToPositionAsync(moveResource, userId);
        }
        else
        {
            _logger.LogInformation($"🎯 Using SIMPLE logic: No positioning parameters provided");
            // Simple move - update topic and append to end (first position in empty container)
            result = await MoveSubTopicSimpleAsync(moveResource, userId);
        }

        _logger.LogInformation($"MoveSubTopicAsync: Successfully moved subtopic {moveResource.SubTopicId}");
        
        // ✅ RESTORED: Update schedule events after SubTopic move
        await UpdateScheduleAfterSubTopicMoveAsync(moveResource.SubTopicId, userId);
        
        return result;
    }

    // **POSITIONING METHODS** - SubTopicService.cs - Handle positioning with new contract
    // RESPONSIBILITY: Process positioning parameters and delegate to repository
    // DOES NOT: Change business logic (just uses new positioning contract)
    // CALLED BY: MoveSubTopicAsync for positioned moves

    // ✅ NEW: Handle positioning with new contract (RelativeToId, Position, RelativeToType)
    private async Task<SubTopicResource> MoveSubTopicWithPositioningAsync(SubTopicMoveResource moveResource, int userId)
    {
        _logger.LogInformation($"🔍 MoveSubTopicWithPositioningAsync: Processing positioning parameters RelativeToId={moveResource.RelativeToId}, Position={moveResource.Position}, RelativeToType={moveResource.RelativeToType}");
        
        // Validate the relative entity exists and is accessible
        var relativeEntityType = moveResource.RelativeToType;
        var relativeEntityId = moveResource.RelativeToId.Value;
        
        if (relativeEntityType == "Lesson")
        {
            var relativeLesson = await _lessonRepository.GetByIdAsync(relativeEntityId);
            if (relativeLesson == null)
            {
                throw new ArgumentException($"Relative lesson {relativeEntityId} not found");
            }
            if (relativeLesson.UserId != userId)
            {
                throw new UnauthorizedAccessException($"Relative lesson {relativeEntityId} not owned by user {userId}");
            }
            _logger.LogInformation($"🎯 Validated relative Lesson {relativeEntityId} for positioning");
        }
        else if (relativeEntityType == "SubTopic")
        {
            var relativeSubTopic = await _subTopicRepository.GetByIdAsync(relativeEntityId);
            if (relativeSubTopic == null)
            {
                throw new ArgumentException($"Relative subtopic {relativeEntityId} not found");
            }
            if (relativeSubTopic.UserId != userId)
            {
                throw new UnauthorizedAccessException($"Relative subtopic {relativeEntityId} not owned by user {userId}");
            }
            _logger.LogInformation($"🎯 Validated relative SubTopic {relativeEntityId} for positioning");
        }
        else
        {
            throw new ArgumentException($"RelativeToType '{relativeEntityType}' is not supported. Must be 'Lesson' or 'SubTopic'");
        }

        // Call repository with positioning parameters  
        _logger.LogInformation($"🚀 Calling repository with positioning: SubTopicId={moveResource.SubTopicId}, NewTopicId={moveResource.NewTopicId}, RelativeToId={relativeEntityId}, Position={moveResource.Position}, RelativeToType={relativeEntityType}");
        
        var positionedSubTopic = await _subTopicRepository.MoveSubTopicWithPositioningAsync(
            moveResource.SubTopicId,
            moveResource.NewTopicId,
            relativeEntityId,
            moveResource.Position,
            relativeEntityType
        );

        _logger.LogInformation($"✅ Repository positioning completed for SubTopic {moveResource.SubTopicId}, new SortOrder: {positionedSubTopic.SortOrder}");
        return _mapper.Map<SubTopicResource>(positionedSubTopic);
    }

    // ✅ LEGACY: MoveSubTopicToPositionAsync method in SubTopicService
    private async Task<SubTopicResource> MoveSubTopicToPositionAsync(SubTopicMoveResource moveResource, int userId)
    {
        // ✅ NEW: Discover what type of entity the sibling is
        var siblingType = await DetermineSiblingTypeAsync(moveResource.AfterSiblingId.Value, userId);

        // ✅ CONVERTED: Same validation logic, just using discovered type
        if (siblingType == "SubTopic")
        {
            var relativeSubTopic = await _subTopicRepository.GetByIdAsync(moveResource.AfterSiblingId.Value);
            if (relativeSubTopic == null)
            {
                throw new ArgumentException($"Relative subtopic {moveResource.AfterSiblingId.Value} not found");
            }
            if (relativeSubTopic.TopicId != moveResource.NewTopicId)
            {
                throw new ArgumentException("Relative subtopic must be in the target topic");
            }
        }
        else if (siblingType == "Lesson")
        {
            var relativeLesson = await _lessonRepository.GetByIdAsync(moveResource.AfterSiblingId.Value);
            if (relativeLesson == null)
            {
                throw new ArgumentException($"Relative lesson {moveResource.AfterSiblingId.Value} not found");
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
            throw new ArgumentException($"Sibling type '{siblingType}' is not valid for subtopic positioning. Must be 'SubTopic' or 'Lesson'");
        }

        // ✅ FIXED: Updated call to match new repository signature
        var positionedSubTopic = await _subTopicRepository.MoveSubTopicToPositionAsync(
            moveResource.SubTopicId,
            moveResource.NewTopicId,
            moveResource.AfterSiblingId.Value,
            siblingType  // ✅ SIMPLIFIED: Just pass the discovered type
        );

        return _mapper.Map<SubTopicResource>(positionedSubTopic);
    }

    // ✅ REPLACE: MoveSubTopicSimpleAsync method (REMOVE DUPLICATE)
    // Only keep ONE version of this method
    private async Task<SubTopicResource> MoveSubTopicSimpleAsync(SubTopicMoveResource moveResource, int userId)
    {
        // Get subtopic and update topic
        var subTopic = await _subTopicRepository.GetByIdAsync(moveResource.SubTopicId);

        // Always assign new sort order to ensure proper positioning
        var maxSortOrder = await _subTopicRepository.GetMaxSortOrderInTopicAsync(moveResource.NewTopicId);
        subTopic.SortOrder = maxSortOrder + 1;
        subTopic.TopicId = moveResource.NewTopicId;

        await _subTopicRepository.UpdateAsync(subTopic);

        return _mapper.Map<SubTopicResource>(subTopic);
    }

    private async Task<string> DetermineSiblingTypeAsync(int siblingId, int userId)
    {
        // Check if it's a Lesson
        var lesson = await _lessonRepository.GetByIdAsync(siblingId);
        if (lesson != null && lesson.UserId == userId && !lesson.Archived)
        {
            return "Lesson";
        }

        // Check if it's a SubTopic  
        var subTopic = await _subTopicRepository.GetByIdAsync(siblingId);
        if (subTopic != null && subTopic.UserId == userId && !subTopic.Archived)
        {
            return "SubTopic";
        }

        // Check if it's a Topic
        var topic = await _topicRepository.GetByIdAsync(siblingId);
        if (topic != null && topic.UserId == userId && !topic.Archived)
        {
            return "Topic";
        }

        throw new ArgumentException($"Sibling entity {siblingId} not found or not accessible to user {userId}");
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
            UserId = userId, // Set to copier's UserId
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

    /// <summary>
    /// Update schedule events after SubTopic move
    /// Calls schedule generation service for each lesson in the moved SubTopic
    /// </summary>
    private async Task UpdateScheduleAfterSubTopicMoveAsync(int subTopicId, int userId)
    {
        try
        {
            _logger.LogInformation($"UpdateScheduleAfterSubTopicMoveAsync: Starting schedule update for SubTopic {subTopicId}");
            
            // Get all lessons in the moved SubTopic with Topic information
            var subTopic = await _subTopicRepository.GetByIdAsync(subTopicId, q => q.Include(s => s.Lessons).Include(s => s.Topic));
            if (subTopic?.Lessons == null || !subTopic.Lessons.Any())
            {
                _logger.LogInformation($"SubTopic {subTopicId} has no lessons, no schedule updates needed");
                return;
            }

            _logger.LogInformation($"Found {subTopic.Lessons.Count} lessons in SubTopic {subTopicId}, updating schedules");

            // Find all schedules that might contain lessons from this course
            // We need to get the course ID to find relevant schedules
            var courseId = GetCourseIdFromSubTopicLesson(subTopic);
            
            if (!courseId.HasValue)
            {
                _logger.LogWarning($"Could not determine course ID for SubTopic {subTopicId}, skipping schedule updates");
                return;
            }

            // Get all schedules that contain this course
            var schedules = await _scheduleRepository.FindAllSchedulesByCourseIdAsync(courseId.Value, userId);
            
            if (!schedules.Any())
            {
                _logger.LogInformation($"No schedules found for course {courseId.Value}, no updates needed");
                return;
            }

            // Update each schedule that might be affected
            // Use the first lesson as a representative - the UpdateScheduleAfterLessonMovedAsync method
            // will regenerate the entire schedule anyway, so we only need to call it once per schedule
            var firstLesson = subTopic.Lessons.First();
            
            foreach (var schedule in schedules)
            {
                try
                {
                    _logger.LogInformation($"Updating schedule {schedule.Id} after SubTopic {subTopicId} move (affecting {subTopic.Lessons.Count} lessons)");
                    
                    // Call once per schedule using any lesson from the SubTopic as a trigger
                    // The method will regenerate affected periods for the entire course anyway
                    await _scheduleGenerationService.UpdateScheduleAfterLessonMovedAsync(schedule.Id, firstLesson.Id, userId);
                    
                    _logger.LogInformation($"Successfully updated schedule {schedule.Id} for SubTopic move");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to update schedule {schedule.Id} after SubTopic {subTopicId} move");
                    // Continue with other schedules rather than failing the entire operation
                }
            }

            _logger.LogInformation($"UpdateScheduleAfterSubTopicMoveAsync: Completed schedule updates for SubTopic {subTopicId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to update schedules after SubTopic {subTopicId} move");
            // Don't rethrow - schedule updates are secondary to the move operation
        }
    }

    /// <summary>
    /// Get course ID from a SubTopic's lesson
    /// </summary>
    private int? GetCourseIdFromSubTopicLesson(SubTopic subTopic)
    {
        // SubTopic -> Topic -> Course
        if (subTopic.Topic?.CourseId != null)
        {
            return subTopic.Topic.CourseId;
        }

        _logger.LogWarning($"SubTopic {subTopic.Id} Topic not loaded or has no CourseId, cannot determine course ID");
        return null;
    }

}
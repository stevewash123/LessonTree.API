using AutoMapper;
using AutoMapper.QueryableExtensions;
using LessonTree.BLL.Service;
using LessonTree.BLL.Services;
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
    private readonly IScheduleService _scheduleService; // NEW DEPENDENCY
    private readonly IScheduleConfigurationService _scheduleConfigurationService; // NEW DEPENDENCY
    private readonly IScheduleGenerationService _scheduleGenerationService;
    private readonly IBackgroundScheduleService _backgroundScheduleService; // NEW DEPENDENCY FOR CALENDAR OPTIMIZATION

    public LessonService(
        ILessonRepository lessonRepository,
        ITopicRepository topicRepository,
        ISubTopicRepository subTopicRepository,
        IStandardRepository standardRepository,
        ILogger<LessonService> logger,
        IMapper mapper,
        IScheduleService scheduleService, // NEW PARAMETER
        IScheduleConfigurationService scheduleConfigurationService, // NEW PARAMETER
        IScheduleGenerationService scheduleGenerationService,
        IBackgroundScheduleService backgroundScheduleService) // NEW PARAMETER FOR CALENDAR OPTIMIZATION
    {
        _lessonRepository = lessonRepository;
        _topicRepository = topicRepository;
        _subTopicRepository = subTopicRepository;
        _standardRepository = standardRepository;
        _logger = logger;
        _mapper = mapper;
        _scheduleService = scheduleService; // NEW ASSIGNMENT
        _scheduleConfigurationService = scheduleConfigurationService; // NEW ASSIGNMENT
        _scheduleGenerationService = scheduleGenerationService;
        _backgroundScheduleService = backgroundScheduleService; // NEW ASSIGNMENT FOR CALENDAR OPTIMIZATION
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

    // **ENHANCED** - AddAsync with Schedule Regeneration Integration
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

        // ✅ FIXED: Calculate proper sort order using existing repository pattern
        if (lessonCreateResource.SubTopicId.HasValue)
        {
            lesson.SortOrder = (await _lessonRepository.GetBySubTopicId(lessonCreateResource.SubTopicId.Value, true)
                .MaxAsync(l => (int?)l.SortOrder) ?? -1) + 1;
        }
        else
        {
            lesson.SortOrder = (await _lessonRepository.GetByTopicId(lessonCreateResource.TopicId.Value, true)
                .MaxAsync(l => (int?)l.SortOrder) ?? -1) + 1;
        }

        // ✅ CORE LESSON CREATION - This must succeed regardless of schedule operations
        var createdLessonId = await _lessonRepository.AddAsync(lesson);
        _logger.LogInformation($"AddAsync: Created lesson {createdLessonId} '{lesson.Title}' with sort order {lesson.SortOrder} for user {userId}");

        // ✅ SCHEDULE REGENERATION INTEGRATION - NEW FUNCTIONALITY
        try
        {
            await TriggerScheduleRegenerationForLessonAddAsync(lesson, userId);
        }
        catch (Exception ex)
        {
            // Log regeneration failure but don't fail the lesson creation
            _logger.LogError(ex, $"AddAsync: Schedule regeneration failed for lesson {createdLessonId}, but lesson creation succeeded");
        }

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

    /// <summary>
    /// Trigger schedule regeneration for all schedules affected by lesson addition
    /// CORRECTED: Actually check if usable configurations exist before proceeding
    /// </summary>
    private async Task TriggerScheduleRegenerationForLessonAddAsync(Lesson lesson, int userId)
    {
        _logger.LogInformation($"TriggerScheduleRegenerationForLessonAddAsync: Starting regeneration check for lesson {lesson.Id}, user {userId}");

        // Step 1: Check if any current or future configurations exist
        var usableConfigurations = await GetCurrentAndFutureConfigurationsAsync(userId);

        if (!usableConfigurations.Any())
        {
            _logger.LogInformation($"TriggerScheduleRegenerationForLessonAddAsync: No current or future schedule configurations found for user {userId} - lesson addition complete, no schedule regeneration needed");
            return;
        }

        _logger.LogInformation($"TriggerScheduleRegenerationForLessonAddAsync: Found {usableConfigurations.Count} current/future configurations for user {userId}");

        // Step 2: Determine which course this lesson belongs to
        var courseId = await GetCourseIdForLessonAsync(lesson, userId);
        if (!courseId.HasValue)
        {
            _logger.LogWarning($"TriggerScheduleRegenerationForLessonAddAsync: Could not determine course for lesson {lesson.Id}");
            return;
        }

        _logger.LogInformation($"TriggerScheduleRegenerationForLessonAddAsync: Lesson {lesson.Id} belongs to course {courseId.Value}");

        // Step 3: Find configurations that use this course
        var affectedConfigurations = FilterConfigurationsUsingCourse(usableConfigurations, courseId.Value);

        if (!affectedConfigurations.Any())
        {
            _logger.LogInformation($"TriggerScheduleRegenerationForLessonAddAsync: None of the {usableConfigurations.Count} configurations use course {courseId.Value} - no regeneration needed");
            return;
        }

        _logger.LogInformation($"TriggerScheduleRegenerationForLessonAddAsync: {affectedConfigurations.Count} configurations use course {courseId.Value} and need regeneration");

        // Step 4: Regenerate schedules for affected configurations
        var successCount = 0;
        var failureCount = 0;

        foreach (var configuration in affectedConfigurations)
        {
            try
            {
                _logger.LogInformation($"TriggerScheduleRegenerationForLessonAddAsync: Regenerating schedule for configuration {configuration.Id} '{configuration.Title}'");

                await _scheduleService.RegenerateScheduleFromConfigurationAsync(configuration.Id, userId);
                successCount++;

                _logger.LogInformation($"TriggerScheduleRegenerationForLessonAddAsync: Successfully regenerated schedule for configuration {configuration.Id}");
            }
            catch (Exception ex)
            {
                failureCount++;
                _logger.LogError(ex, $"TriggerScheduleRegenerationForLessonAddAsync: Failed to regenerate schedule for configuration {configuration.Id} '{configuration.Title}'");
            }
        }

        _logger.LogInformation($"TriggerScheduleRegenerationForLessonAddAsync: Regeneration complete - {successCount} successful, {failureCount} failed out of {affectedConfigurations.Count} configurations");
    }

    /// <summary>
    /// Get all configurations that are current (include today) or future
    /// Business Rule: Only current and future schedules need regeneration
    /// </summary>
    private async Task<List<ScheduleConfigurationResource>> GetCurrentAndFutureConfigurationsAsync(int userId)
    {
        var today = DateTime.Today;

        // Get all configurations for the user
        var allConfigurations = await _scheduleConfigurationService.GetAllAsync(userId);

        // Filter to current or future configurations
        var currentAndFutureConfigurations = allConfigurations
            .Where(config => config.EndDate >= today) // EndDate >= TODAY means current or future
            .ToList();

        _logger.LogDebug($"GetCurrentAndFutureConfigurationsAsync: Found {currentAndFutureConfigurations.Count} current/future configurations out of {allConfigurations.Count} total for user {userId}");

        foreach (var config in currentAndFutureConfigurations)
        {
            var status = config.StartDate <= today && config.EndDate >= today ? "CURRENT" : "FUTURE";
            _logger.LogDebug($"  - Configuration {config.Id} '{config.Title}' ({config.StartDate:yyyy-MM-dd} to {config.EndDate:yyyy-MM-dd}) - {status}");
        }

        return currentAndFutureConfigurations;
    }

    /// <summary>
    /// Filter configurations to only those that have period assignments using the specified course
    /// </summary>
    private List<ScheduleConfigurationResource> FilterConfigurationsUsingCourse(List<ScheduleConfigurationResource> configurations, int courseId)
    {
        var affectedConfigurations = configurations
            .Where(config => config.PeriodAssignments.Any(pa => pa.CourseId == courseId))
            .ToList();

        _logger.LogDebug($"FilterConfigurationsUsingCourse: {affectedConfigurations.Count} out of {configurations.Count} configurations use course {courseId}");

        foreach (var config in affectedConfigurations)
        {
            var periodsUsingCourse = config.PeriodAssignments
                .Where(pa => pa.CourseId == courseId)
                .Select(pa => pa.Period)
                .ToList();

            _logger.LogDebug($"  - Configuration {config.Id} '{config.Title}' uses course {courseId} in period(s): {string.Join(", ", periodsUsingCourse)}");
        }

        return affectedConfigurations;
    }

    /// <summary>
    /// Determine which course a lesson belongs to through Topic hierarchy
    /// </summary>
    private async Task<int?> GetCourseIdForLessonAsync(Lesson lesson, int userId)
    {
        if (lesson.TopicId.HasValue)
        {
            // Direct topic assignment: Lesson → Topic → Course
            var topic = await _topicRepository.GetByIdAsync(lesson.TopicId.Value);
            if (topic?.UserId == userId)
            {
                return topic.CourseId;
            }
        }
        else if (lesson.SubTopicId.HasValue)
        {
            // SubTopic assignment: Lesson → SubTopic → Topic → Course
            var subTopic = await _subTopicRepository.GetByIdAsync(lesson.SubTopicId.Value, q => q.Include(st => st.Topic));
            if (subTopic?.Topic?.UserId == userId)
            {
                return subTopic.Topic.CourseId;
            }
        }

        return null;
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

    // **PARTIAL FILE** - LessonService.cs - Replace MoveLessonAsync method
    // INTEGRATION: Replace the existing MoveLessonAsync method with this enhanced version

    // ✅ URGENT FIX: Replace the MoveLessonAsync method in LessonService

    public async Task<LessonResource> MoveLessonAsync(LessonMoveResource moveResource, int userId)
    {
        _logger.LogInformation("=== LESSON MOVE DIAGNOSTICS START ===");
        _logger.LogInformation("MoveLessonAsync: LessonId={LessonId}, NewSubTopicId={NewSubTopicId}, NewTopicId={NewTopicId}, UserId={UserId}",
            moveResource.LessonId, moveResource.NewSubTopicId, moveResource.NewTopicId, userId);

        // ✅ UPDATED: Log new sibling-based positioning
        _logger.LogInformation("MoveLessonAsync: AfterSiblingId={AfterSiblingId}",
            moveResource.AfterSiblingId);

        // Validate input
        if (moveResource.NewSubTopicId.HasValue && moveResource.NewTopicId.HasValue)
        {
            throw new ArgumentException("Lesson cannot be moved to both SubTopic and Topic");
        }
        if (!moveResource.NewSubTopicId.HasValue && !moveResource.NewTopicId.HasValue)
        {
            throw new ArgumentException("Lesson must be moved to either SubTopic or Topic");
        }

        // ✅ UPDATED: Check if this is a positional move (has sibling)
        if (moveResource.AfterSiblingId.HasValue)
        {
            _logger.LogInformation("MoveLessonAsync: POSITIONAL MOVE detected - calling MoveLessonToPositionAsync");
            await _lessonRepository.MoveLessonToPositionAsync(moveResource, userId);
            _logger.LogInformation("MoveLessonAsync: POSITIONAL MOVE completed successfully");
            
            // ✅ RESTORED: Update schedule events after lesson move
            await UpdateScheduleAfterLessonMoveAsync(moveResource.LessonId, userId);
            
            _logger.LogInformation("=== LESSON MOVE DIAGNOSTICS END ===");
            
            // Get the updated lesson to return
            var positionedLesson = await GetByIdAsync(moveResource.LessonId, userId);
            return _mapper.Map<LessonResource>(positionedLesson);
        }

        _logger.LogInformation("MoveLessonAsync: SIMPLE MOVE (first position in empty container)");

        // Simple move (first position in empty container)
        var lesson = await _lessonRepository.GetByIdAsync(moveResource.LessonId, q => q.Include(l => l.SubTopic).Include(l => l.Topic));
        if (lesson == null)
        {
            _logger.LogError("Lesson with ID {LessonId} not found", moveResource.LessonId);
            throw new ArgumentException("Lesson not found");
        }

        if (lesson.UserId != userId)
        {
            _logger.LogWarning("User ID {UserId} attempted to move lesson ID {LessonId} owned by another user", userId, moveResource.LessonId);
            throw new UnauthorizedAccessException("Lesson not owned by user");
        }

        _logger.LogInformation("MoveLessonAsync: Current lesson state - SubTopicId={CurrentSubTopicId}, TopicId={CurrentTopicId}, SortOrder={CurrentSortOrder}",
            lesson.SubTopicId, lesson.TopicId, lesson.SortOrder);

        // ✅ SIMPLIFIED: No sibling = first position (SortOrder = 0)
        if (moveResource.NewSubTopicId.HasValue)
        {
            var newSubTopic = await _subTopicRepository.GetByIdAsync(moveResource.NewSubTopicId.Value);
            if (newSubTopic == null)
            {
                _logger.LogError("SubTopic with ID {SubTopicId} not found", moveResource.NewSubTopicId);
                throw new ArgumentException("SubTopic not found");
            }
            lesson.SubTopicId = moveResource.NewSubTopicId;
            lesson.TopicId = null;
            lesson.SortOrder = 0; // ✅ First position

            _logger.LogInformation("MoveLessonAsync: Moving to SubTopic - first position (SortOrder=0)");
        }
        else
        {
            var topic = await _topicRepository.GetByIdAsync(moveResource.NewTopicId.Value);
            if (topic == null)
            {
                _logger.LogError("Topic with ID {TopicId} not found", moveResource.NewTopicId);
                throw new ArgumentException("Topic not found");
            }
            lesson.TopicId = moveResource.NewTopicId;
            lesson.SubTopicId = null;
            lesson.SortOrder = 0; // ✅ First position

            _logger.LogInformation("MoveLessonAsync: Moving to Topic - first position (SortOrder=0)");
        }

        await _lessonRepository.UpdateAsync(lesson);

        _logger.LogInformation("MoveLessonAsync: SIMPLE MOVE completed - final SortOrder=0");
        
        // ✅ RESTORED: Update schedule events after lesson move
        await UpdateScheduleAfterLessonMoveAsync(moveResource.LessonId, userId);
        
        _logger.LogInformation("=== LESSON MOVE DIAGNOSTICS END ===");

        var updatedLesson = await GetByIdAsync(moveResource.LessonId, userId);
        return _mapper.Map<LessonResource>(updatedLesson);
    }

    // ✅ NEW: Enhanced lesson move with calendar optimization support
    public async Task<LessonPositioningResult> MoveLessonWithOptimizationAsync(LessonMoveResource moveResource, int userId)
    {
        _logger.LogInformation("=== OPTIMIZED LESSON MOVE START ===");
        _logger.LogInformation("MoveLessonWithOptimizationAsync: LessonId={LessonId}, RequestPartialScheduleUpdate={RequestPartialUpdate}, DateRange={Start}-{End}",
            moveResource.LessonId, moveResource.RequestPartialScheduleUpdate, moveResource.CalendarStartDate, moveResource.CalendarEndDate);

        // First, perform the standard lesson move
        var movedLesson = await MoveLessonAsync(moveResource, userId);

        // Create the enhanced result
        var result = new LessonPositioningResult
        {
            IsSuccess = true,
            LessonId = movedLesson.Id,
            NewSubTopicId = movedLesson.SubTopicId,
            NewTopicId = movedLesson.TopicId,
            TargetSortOrder = movedLesson.SortOrder,
            ModifiedEntities = new List<ModifiedEntityInfo>
            {
                new ModifiedEntityInfo
                {
                    EntityId = movedLesson.Id,
                    EntityType = "Lesson",
                    NewSortOrder = movedLesson.SortOrder,
                    ParentId = movedLesson.SubTopicId ?? movedLesson.TopicId,
                    ParentType = movedLesson.SubTopicId.HasValue ? "SubTopic" : "Topic"
                }
            }
        };

        // ✅ NEW: Check if partial schedule update was requested and is feasible
        // ✅ UPDATED: Handle null calendar ranges per user requirement
        var canOptimizeMove = moveResource.RequestPartialScheduleUpdate &&
                              moveResource.CalendarStartDate.HasValue &&
                              moveResource.CalendarEndDate.HasValue;

        // ✅ NEW: If calendar is not open/instantiated, pass null for range and skip optimization
        if (moveResource.RequestPartialScheduleUpdate &&
            (!moveResource.CalendarStartDate.HasValue || !moveResource.CalendarEndDate.HasValue))
        {
            _logger.LogInformation("MoveLessonWithOptimizationAsync: Calendar not open/instantiated - null date range provided, skipping partial generation and using full regeneration");
            canOptimizeMove = false;
        }

        if (canOptimizeMove)
        {
            try
            {
                _logger.LogInformation("Attempting partial schedule update for date range {Start} to {End}",
                    moveResource.CalendarStartDate, moveResource.CalendarEndDate);

                var partialEvents = await GeneratePartialScheduleEventsAsync(
                    movedLesson.Id,
                    userId,
                    moveResource.CalendarStartDate.Value,
                    moveResource.CalendarEndDate.Value);

                if (partialEvents?.Any() == true)
                {
                    result.HasPartialScheduleUpdates = true;
                    result.PartialScheduleEvents = partialEvents;
                    result.UpdatedDateRangeStart = moveResource.CalendarStartDate;
                    result.UpdatedDateRangeEnd = moveResource.CalendarEndDate;
                    result.RequiresFullScheduleRegeneration = false;
                    result.OptimizationReason = $"Generated {partialEvents.Count} events for date range optimization";

                    _logger.LogInformation("Partial schedule update successful: {EventCount} events generated", partialEvents.Count);
                }
                else
                {
                    result.RequiresFullScheduleRegeneration = true;
                    result.OptimizationReason = "No events generated for specified date range - falling back to full regeneration";
                    _logger.LogWarning("Partial schedule update failed - no events generated");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Partial schedule update failed - falling back to full regeneration");
                result.RequiresFullScheduleRegeneration = true;
                result.OptimizationReason = $"Partial update failed: {ex.Message} - falling back to full regeneration";
            }
        }
        else
        {
            result.RequiresFullScheduleRegeneration = true;
            // ✅ UPDATED: Better logging for null calendar scenarios
            if (moveResource.RequestPartialScheduleUpdate &&
                (!moveResource.CalendarStartDate.HasValue || !moveResource.CalendarEndDate.HasValue))
            {
                result.OptimizationReason = "Calendar not open/instantiated (null date range) - full schedule regeneration required";
                _logger.LogInformation("Calendar not open/instantiated - null date range provided, using standard full regeneration");
            }
            else
            {
                result.OptimizationReason = "Standard move operation - full schedule regeneration required";
                _logger.LogInformation("No partial schedule update requested - using standard full regeneration");
            }
        }

        _logger.LogInformation("=== OPTIMIZED LESSON MOVE END === Result: HasPartial={HasPartial}, RequiresFull={RequiresFull}",
            result.HasPartialScheduleUpdates, result.RequiresFullScheduleRegeneration);

        return result;
    }

    // ✅ NEW: Helper method to generate partial schedule events for date range
    private async Task<List<ScheduleEventResource>?> GeneratePartialScheduleEventsAsync(int lessonId, int userId, DateTime startDate, DateTime endDate)
    {
        try
        {
            // Get the lesson to determine its course
            var lesson = await _lessonRepository.GetByIdAsync(lessonId, q => q.Include(l => l.SubTopic).ThenInclude(s => s.Topic).Include(l => l.Topic));
            if (lesson == null) return null;

            var courseId = await GetCourseIdForLessonAsync(lesson, userId);
            if (!courseId.HasValue) return null;

            // Find schedules containing this course
            var schedules = await GetSchedulesForCourseAsync(courseId.Value, userId);
            if (!schedules.Any()) return null;

            var allPartialEvents = new List<ScheduleEventResource>();

            foreach (var schedule in schedules)
            {
                // Generate events for this schedule within the date range
                var scheduleEvents = await _scheduleGenerationService.GenerateEventsForDateRangeAsync(
                    schedule.Id, startDate, endDate, userId);

                if (scheduleEvents?.Any() == true)
                {
                    allPartialEvents.AddRange(scheduleEvents);
                }
            }

            _logger.LogInformation("Generated {EventCount} partial schedule events for lesson {LessonId} in date range {Start}-{End}",
                allPartialEvents.Count, lessonId, startDate, endDate);

            return allPartialEvents;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate partial schedule events for lesson {LessonId}", lessonId);
            return null;
        }
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

    /// <summary>
    /// Update schedule events after lesson move
    /// Calls schedule generation service to update all schedules containing this lesson
    /// </summary>
    private async Task UpdateScheduleAfterLessonMoveAsync(int lessonId, int userId)
    {
        try
        {
            _logger.LogInformation($"UpdateScheduleAfterLessonMoveAsync: Starting schedule update for lesson {lessonId}");
            
            // Get the moved lesson to find affected courses
            var lesson = await _lessonRepository.GetByIdAsync(lessonId, q => q.Include(l => l.SubTopic).ThenInclude(s => s.Topic).Include(l => l.Topic));
            if (lesson == null)
            {
                _logger.LogWarning($"Lesson {lessonId} not found, skipping schedule updates");
                return;
            }

            // Determine course ID
            var courseId = await GetCourseIdForLessonAsync(lesson, userId);
            if (!courseId.HasValue)
            {
                _logger.LogWarning($"Could not determine course ID for lesson {lessonId}, skipping schedule updates");
                return;
            }

            // Find all schedules that contain this course
            var schedules = await GetSchedulesForCourseAsync(courseId.Value, userId);
            
            if (!schedules.Any())
            {
                _logger.LogInformation($"No schedules found for course {courseId.Value}, no updates needed");
                return;
            }

            // Update each affected schedule
            foreach (var schedule in schedules)
            {
                try
                {
                    _logger.LogInformation($"Updating schedule {schedule.Id} after lesson {lessonId} move");
                    
                    await _scheduleGenerationService.UpdateScheduleAfterLessonMovedAsync(schedule.Id, lessonId, userId);
                    
                    _logger.LogInformation($"Successfully updated schedule {schedule.Id} for lesson move");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to update schedule {schedule.Id} after lesson {lessonId} move");
                    // Continue with other schedules rather than failing the entire operation
                }
            }

            _logger.LogInformation($"UpdateScheduleAfterLessonMoveAsync: Completed schedule updates for lesson {lessonId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to update schedules after lesson {lessonId} move");
            // Don't rethrow - schedule updates are secondary to the move operation
        }
    }

    /// <summary>
    /// Get all schedules that contain lessons from the specified course
    /// </summary>
    private async Task<List<Schedule>> GetSchedulesForCourseAsync(int courseId, int userId)
    {
        try
        {
            _logger.LogInformation($"GetSchedulesForCourseAsync: Looking for schedules containing course {courseId} for user {userId}");

            // Get all schedules that contain this course
            var schedules = await _scheduleService.FindAllSchedulesByCourseIdAsync(courseId, userId);

            _logger.LogInformation($"Found {schedules.Count} schedules for course {courseId}");

            // Convert to domain objects
            return schedules.Select(s => new Schedule { Id = s.Id, UserId = s.UserId }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get schedules for course {courseId}, user {userId}");
            return new List<Schedule>();
        }
    }

    // ✅ NEW: Calendar Update Optimization - Optimized lesson creation with partial schedule generation
    public async Task<LessonOptimizedResponse> CreateLessonOptimizedAsync(LessonCreateOptimizedResource createResource, int userId)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("CreateLessonOptimizedAsync: Starting optimized lesson creation for user {UserId}, Title: {Title}", userId, createResource.Title);

        var response = new LessonOptimizedResponse();

        try
        {
            // 1. Create the lesson using regular creation process
            var lessonId = await AddAsync(createResource, userId);
            var createdLesson = await GetByIdAsync(lessonId, userId);
            response.Lesson = _mapper.Map<LessonResource>(createdLesson);

            _logger.LogInformation("CreateLessonOptimizedAsync: Lesson {LessonId} created successfully", lessonId);

            // 2. Determine if partial schedule optimization can be applied
            // ✅ UPDATED: Handle null calendar ranges per user requirement
            var canOptimize = createResource.RequestPartialScheduleUpdate &&
                             createResource.CalendarStartDate.HasValue &&
                             createResource.CalendarEndDate.HasValue;

            // ✅ NEW: If calendar is not open/instantiated, pass null for range and skip optimization
            if (createResource.RequestPartialScheduleUpdate &&
                (!createResource.CalendarStartDate.HasValue || !createResource.CalendarEndDate.HasValue))
            {
                _logger.LogInformation("CreateLessonOptimizedAsync: Calendar not open/instantiated - null date range provided, skipping partial generation and using full regeneration");
                canOptimize = false;
            }

            if (canOptimize)
            {
                _logger.LogInformation("CreateLessonOptimizedAsync: Attempting partial schedule generation for lesson {LessonId} between {StartDate} and {EndDate}",
                    lessonId, createResource.CalendarStartDate.Value.ToString("yyyy-MM-dd"), createResource.CalendarEndDate.Value.ToString("yyyy-MM-dd"));

                // 3. Find schedules that contain this course for partial optimization
                var courseId = GetCourseIdFromLesson(createResource);
                if (courseId.HasValue)
                {
                    var schedules = await GetSchedulesForCourseAsync(courseId.Value, userId);

                    if (schedules.Any())
                    {
                        // 4. Apply partial schedule generation for the date range
                        var totalEventsGenerated = 0;
                        var optimizationSuccessful = true;

                        foreach (var schedule in schedules)
                        {
                            try
                            {
                                var partialEvents = await _scheduleGenerationService.GenerateEventsForDateRangeAsync(
                                    schedule.Id,
                                    createResource.CalendarStartDate.Value,
                                    createResource.CalendarEndDate.Value,
                                    userId);

                                if (partialEvents?.Count > 0)
                                {
                                    // Update schedule with partial events for the date range
                                    totalEventsGenerated += partialEvents.Count;
                                    _logger.LogInformation("CreateLessonOptimizedAsync: Generated {EventCount} partial events for schedule {ScheduleId}",
                                        partialEvents.Count, schedule.Id);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "CreateLessonOptimizedAsync: Partial generation failed for schedule {ScheduleId}, will use background job", schedule.Id);
                                optimizationSuccessful = false;
                            }
                        }

                        // 5. Set optimization results
                        response.IsOptimized = optimizationSuccessful;
                        response.HasPartialGeneration = totalEventsGenerated > 0;
                        response.PartialEventsGenerated = totalEventsGenerated;
                        response.PartialGenerationDateRange = createResource.CalendarStartDate.Value;

                        // 6. Trigger background job for full schedule rebuild (eventual consistency)
                        var backgroundJobId = _backgroundScheduleService.EnqueueScheduleRebuild(
                            schedules.First().Id,
                            await GetScheduleConfigurationIdAsync(schedules.First().Id, userId),
                            userId,
                            $"Lesson Create - {createResource.Title} on {DateTime.UtcNow:yyyy-MM-dd}"
                        );
                        response.BackgroundJobId = backgroundJobId;

                        _logger.LogInformation("CreateLessonOptimizedAsync: Optimization completed. IsOptimized: {IsOptimized}, EventsGenerated: {EventsGenerated}",
                            response.IsOptimized, response.PartialEventsGenerated);
                    }
                    else
                    {
                        _logger.LogInformation("CreateLessonOptimizedAsync: No schedules found for course {CourseId}, using fallback", courseId.Value);
                        response.IsOptimized = false;
                        response.HasPartialGeneration = false;
                    }
                }
            }
            else
            {
                _logger.LogInformation("CreateLessonOptimizedAsync: Calendar optimization not requested or date range not provided - using background full regeneration");

                // ✅ NEW: Even when optimization can't be applied, still trigger background rebuild
                var courseId = GetCourseIdFromLesson(createResource);
                if (courseId.HasValue)
                {
                    var schedules = await GetSchedulesForCourseAsync(courseId.Value, userId);
                    if (schedules.Any())
                    {
                        var backgroundJobId = _backgroundScheduleService.EnqueueScheduleRebuild(
                            schedules.First().Id,
                            await GetScheduleConfigurationIdAsync(schedules.First().Id, userId),
                            userId,
                            $"Lesson Create (Full Rebuild) - {createResource.Title} on {DateTime.UtcNow:yyyy-MM-dd}"
                        );
                        response.BackgroundJobId = backgroundJobId;
                        _logger.LogInformation("CreateLessonOptimizedAsync: Enqueued background full regeneration with job ID {JobId}", backgroundJobId);
                    }
                }

                response.IsOptimized = false;
                response.HasPartialGeneration = false;
            }

            // 7. Performance metrics
            var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            response.PerformanceMetrics = $"Total: {elapsedMs:F0}ms, Optimized: {response.IsOptimized}";

            _logger.LogInformation("CreateLessonOptimizedAsync: Completed for lesson {LessonId} in {ElapsedMs}ms", lessonId, elapsedMs);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateLessonOptimizedAsync: Failed for user {UserId}", userId);
            throw;
        }
    }

    // ✅ NEW: Calendar Update Optimization - Optimized lesson deletion with partial schedule generation
    public async Task<LessonOptimizedResponse> DeleteLessonOptimizedAsync(LessonDeleteOptimizedRequest deleteRequest, int userId)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("DeleteLessonOptimizedAsync: Starting optimized lesson deletion for lesson {LessonId}", deleteRequest.LessonId);

        var response = new LessonOptimizedResponse();

        try
        {
            // 1. Get lesson info before deletion for course identification
            var lessonToDelete = await GetByIdAsync(deleteRequest.LessonId, userId);
            if (lessonToDelete == null)
            {
                throw new ArgumentException($"Lesson {deleteRequest.LessonId} not found or not accessible by user {userId}");
            }

            var courseId = lessonToDelete.CourseId;
            _logger.LogInformation("DeleteLessonOptimizedAsync: Found lesson {LessonId} in course {CourseId}", deleteRequest.LessonId, courseId);

            // 2. Determine if partial schedule optimization can be applied
            // ✅ UPDATED: Handle null calendar ranges per user requirement
            var canOptimize = deleteRequest.RequestPartialScheduleUpdate &&
                             deleteRequest.CalendarStartDate.HasValue &&
                             deleteRequest.CalendarEndDate.HasValue;

            // ✅ NEW: If calendar is not open/instantiated, pass null for range and skip optimization
            if (deleteRequest.RequestPartialScheduleUpdate &&
                (!deleteRequest.CalendarStartDate.HasValue || !deleteRequest.CalendarEndDate.HasValue))
            {
                _logger.LogInformation("DeleteLessonOptimizedAsync: Calendar not open/instantiated - null date range provided, skipping partial generation and using full regeneration");
                canOptimize = false;
            }

            if (canOptimize)
            {
                _logger.LogInformation("DeleteLessonOptimizedAsync: Attempting partial schedule regeneration for date range {StartDate} to {EndDate}",
                    deleteRequest.CalendarStartDate.Value.ToString("yyyy-MM-dd"), deleteRequest.CalendarEndDate.Value.ToString("yyyy-MM-dd"));

                // 3. Find schedules that contain this course for partial optimization
                var schedules = await GetSchedulesForCourseAsync(courseId, userId);

                if (schedules.Any())
                {
                    // 4. Delete the lesson first
                    await DeleteAsync(deleteRequest.LessonId, userId);
                    response.Lesson.Id = deleteRequest.LessonId; // Set deleted lesson ID

                    // 5. Apply partial schedule regeneration for the date range (after deletion)
                    var totalEventsGenerated = 0;
                    var optimizationSuccessful = true;

                    foreach (var schedule in schedules)
                    {
                        try
                        {
                            var partialEvents = await _scheduleGenerationService.GenerateEventsForDateRangeAsync(
                                schedule.Id,
                                deleteRequest.CalendarStartDate.Value,
                                deleteRequest.CalendarEndDate.Value,
                                userId);

                            if (partialEvents?.Count > 0)
                            {
                                totalEventsGenerated += partialEvents.Count;
                                _logger.LogInformation("DeleteLessonOptimizedAsync: Regenerated {EventCount} partial events for schedule {ScheduleId} after deletion",
                                    partialEvents.Count, schedule.Id);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "DeleteLessonOptimizedAsync: Partial regeneration failed for schedule {ScheduleId}, will use background job", schedule.Id);
                            optimizationSuccessful = false;
                        }
                    }

                    // 6. Set optimization results
                    response.IsOptimized = optimizationSuccessful;
                    response.HasPartialGeneration = totalEventsGenerated > 0;
                    response.PartialEventsGenerated = totalEventsGenerated;
                    response.PartialGenerationDateRange = deleteRequest.CalendarStartDate.Value;

                    // 7. Trigger background job for full schedule rebuild (eventual consistency)
                    var backgroundJobId = _backgroundScheduleService.EnqueueScheduleRebuild(
                        schedules.First().Id,
                        await GetScheduleConfigurationIdAsync(schedules.First().Id, userId),
                        userId,
                        $"Lesson Delete - ID {deleteRequest.LessonId} on {DateTime.UtcNow:yyyy-MM-dd}"
                    );
                    response.BackgroundJobId = backgroundJobId;

                    _logger.LogInformation("DeleteLessonOptimizedAsync: Optimization completed. IsOptimized: {IsOptimized}, EventsGenerated: {EventsGenerated}",
                        response.IsOptimized, response.PartialEventsGenerated);
                }
                else
                {
                    // No schedules found, just delete the lesson
                    await DeleteAsync(deleteRequest.LessonId, userId);
                    response.Lesson.Id = deleteRequest.LessonId;
                    response.IsOptimized = false;
                    response.HasPartialGeneration = false;
                    _logger.LogInformation("DeleteLessonOptimizedAsync: No schedules found for course {CourseId}, used regular deletion", courseId);
                }
            }
            else
            {
                // Regular deletion without optimization - but still trigger background rebuild
                await DeleteAsync(deleteRequest.LessonId, userId);
                response.Lesson.Id = deleteRequest.LessonId;
                response.IsOptimized = false;
                response.HasPartialGeneration = false;

                // ✅ NEW: Even when optimization can't be applied, still trigger background rebuild
                var schedules = await GetSchedulesForCourseAsync(courseId, userId);
                if (schedules.Any())
                {
                    var backgroundJobId = _backgroundScheduleService.EnqueueScheduleRebuild(
                        schedules.First().Id,
                        await GetScheduleConfigurationIdAsync(schedules.First().Id, userId),
                        userId,
                        $"Lesson Delete (Full Rebuild) - ID {deleteRequest.LessonId} on {DateTime.UtcNow:yyyy-MM-dd}"
                    );
                    response.BackgroundJobId = backgroundJobId;
                    _logger.LogInformation("DeleteLessonOptimizedAsync: Enqueued background full regeneration with job ID {JobId}", backgroundJobId);
                }

                _logger.LogInformation("DeleteLessonOptimizedAsync: Calendar optimization not requested or calendar not open - used regular deletion with background rebuild");
            }

            // 8. Performance metrics
            var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            response.PerformanceMetrics = $"Total: {elapsedMs:F0}ms, Optimized: {response.IsOptimized}";

            _logger.LogInformation("DeleteLessonOptimizedAsync: Completed for lesson {LessonId} in {ElapsedMs}ms", deleteRequest.LessonId, elapsedMs);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteLessonOptimizedAsync: Failed for lesson {LessonId}", deleteRequest.LessonId);
            throw;
        }
    }

    /// <summary>
    /// Extract course ID from lesson creation resource
    /// </summary>
    private int? GetCourseIdFromLesson(LessonCreateOptimizedResource createResource)
    {
        // For lesson creation, we need to determine course ID from the parent (SubTopic or Topic)
        if (createResource.SubTopicId.HasValue)
        {
            // Get course ID from SubTopic
            var subTopic = _subTopicRepository.GetByIdAsync(createResource.SubTopicId.Value, q => q.Include(s => s.Topic)).Result;
            return subTopic?.Topic?.CourseId;
        }
        else if (createResource.TopicId.HasValue)
        {
            // Get course ID from Topic
            var topic = _topicRepository.GetByIdAsync(createResource.TopicId.Value).Result;
            return topic?.CourseId;
        }
        return null;
    }

    /// <summary>
    /// Get schedule configuration ID for a schedule
    /// </summary>
    private async Task<int> GetScheduleConfigurationIdAsync(int scheduleId, int userId)
    {
        try
        {
            var schedule = await _scheduleService.GetByIdAsync(scheduleId, userId);
            return schedule?.ScheduleConfiguration?.Id ?? 0;
        }
        catch
        {
            return 0; // Fallback value if configuration cannot be retrieved
        }
    }
}
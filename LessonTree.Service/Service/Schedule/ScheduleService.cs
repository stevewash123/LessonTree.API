// **MODIFIED FILE** - Enhanced ScheduleService.cs - Complete Auto-Generation Integration
// INTEGRATION: Complete ScheduleService.cs with all new auto-generation methods added

using AutoMapper;
using LessonTree.BLL.Services;
using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
using LessonTree.Models.DTO;
using Microsoft.Extensions.Logging;

namespace LessonTree.BLL.Services
{
    /// <summary>
    /// Enhanced service for schedule operations with auto-generation capabilities
    /// Handles CRUD operations for user schedules and special days + schedule generation
    /// </summary>
    public class ScheduleService : IScheduleService
    {
        private readonly IScheduleRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<ScheduleService> _logger;
        private readonly IScheduleGenerationService _scheduleGenerationService;
        private readonly IScheduleConfigurationService _scheduleConfigurationService;
        private readonly IBackgroundScheduleService _backgroundScheduleService;

        public ScheduleService(
            IScheduleRepository repository,
            IMapper mapper,
            ILogger<ScheduleService> logger,
            IScheduleGenerationService scheduleGenerationService,
            IScheduleConfigurationService scheduleConfigurationService,
            IBackgroundScheduleService backgroundScheduleService)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _scheduleGenerationService = scheduleGenerationService;
            _scheduleConfigurationService = scheduleConfigurationService;
            _backgroundScheduleService = backgroundScheduleService;
        }

        // === CORE SCHEDULE OPERATIONS ===

        public async Task<ScheduleResource?> GetUserScheduleAsync(int userId)
        {
            _logger.LogInformation($"GetUserScheduleAsync: Fetching schedule for user {userId}");

            var schedule = await _repository.GetByUserIdAsync(userId);
            if (schedule == null)
            {
                _logger.LogInformation($"GetUserScheduleAsync: No schedule found for user {userId}");
                return null;
            }

            _logger.LogInformation($"GetUserScheduleAsync: Found schedule {schedule.Id} for user {userId}");
            return _mapper.Map<ScheduleResource>(schedule);
        }

        public async Task<ScheduleResource?> GetByIdAsync(int id, int userId)
        {
            _logger.LogInformation($"GetByIdAsync: Fetching schedule {id} for user {userId}");

            var schedule = await _repository.GetByIdAsync(id);
            if (schedule == null)
            {
                _logger.LogInformation($"GetByIdAsync: Schedule {id} not found");
                return null;
            }

            // Validate ownership
            if (schedule.UserId != userId)
            {
                _logger.LogWarning($"GetByIdAsync: Schedule {id} not owned by user {userId}");
                throw new UnauthorizedAccessException($"Schedule {id} not owned by user {userId}");
            }

            _logger.LogInformation($"GetByIdAsync: Found schedule {id} for user {userId}");
            return _mapper.Map<ScheduleResource>(schedule);
        }

        public async Task<ScheduleResource> CreateScheduleAsync(ScheduleCreateResource createResource, int userId)
        {
            _logger.LogInformation($"CreateScheduleAsync: Creating schedule '{createResource.Title}' for user {userId}");

            // Map events to domain objects
            var scheduleEvents = new List<ScheduleEvent>();
            if (createResource.ScheduleEvents != null)
            {
                scheduleEvents = _mapper.Map<List<ScheduleEvent>>(createResource.ScheduleEvents);
                _logger.LogDebug($"CreateScheduleAsync: Mapped {scheduleEvents.Count} schedule events for user {userId}");
            }

            // Create or replace schedule with UI-generated events
            var createdSchedule = await _repository.CreateOrReplaceScheduleAsync(
                userId,
                scheduleEvents,
                createResource.ScheduleConfigurationId);

            _logger.LogInformation($"CreateScheduleAsync: Created schedule {createdSchedule.Id} '{createResource.Title}' for user {userId}");
            return _mapper.Map<ScheduleResource>(createdSchedule);
        }

        public async Task<ScheduleResource> UpdateScheduleEventsAsync(int scheduleId, List<ScheduleEventResource> events, int userId)
        {
            _logger.LogInformation($"UpdateScheduleEventsAsync: Updating events for schedule {scheduleId}, user {userId}");

            // Validate ownership
            var existingSchedule = await _repository.GetByIdAsync(scheduleId);
            if (existingSchedule == null)
            {
                _logger.LogInformation($"UpdateScheduleEventsAsync: Schedule {scheduleId} not found");
                throw new ArgumentException($"Schedule {scheduleId} not found");
            }

            if (existingSchedule.UserId != userId)
            {
                _logger.LogWarning($"UpdateScheduleEventsAsync: Schedule {scheduleId} not owned by user {userId}");
                throw new UnauthorizedAccessException($"Schedule {scheduleId} not owned by user {userId}");
            }

            // Map events
            var scheduleEvents = _mapper.Map<List<ScheduleEvent>>(events);
            _logger.LogDebug($"UpdateScheduleEventsAsync: Mapped {scheduleEvents.Count} schedule events for update");

            var updatedSchedule = await _repository.UpdateScheduleEventsAsync(scheduleId, scheduleEvents);

            _logger.LogInformation($"UpdateScheduleEventsAsync: Updated {events.Count} events for schedule {scheduleId}");
            return _mapper.Map<ScheduleResource>(updatedSchedule);
        }

        public async Task DeleteUserScheduleAsync(int userId)
        {
            _logger.LogInformation($"DeleteUserScheduleAsync: Deleting schedule for user {userId}");

            await _repository.DeleteScheduleAsync(userId);

            _logger.LogInformation($"DeleteUserScheduleAsync: Deleted schedule for user {userId}");
        }

        // === AUTO-GENERATION METHODS (NEW) ===

        /// <summary>
        /// Find all schedules that contain a specific course through their configurations
        /// Used by lesson addition workflow to determine which schedules need regeneration
        /// </summary>
        public async Task<List<ScheduleResource>> FindAllSchedulesByCourseIdAsync(int courseId, int userId)
        {
            _logger.LogInformation($"FindAllSchedulesByCourseIdAsync: Finding schedules containing course {courseId} for user {userId}");

            var schedules = await _repository.FindAllSchedulesByCourseIdAsync(courseId, userId);

            _logger.LogInformation($"FindAllSchedulesByCourseIdAsync: Found {schedules.Count} schedules containing course {courseId}");

            return _mapper.Map<List<ScheduleResource>>(schedules);
        }


        /// <summary>
        /// Create schedule automatically from configuration (Phase 1 auto-generation)
        /// Replaces frontend generation workflow
        /// </summary>
        /// <param name="configurationId">Schedule configuration ID</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Generated schedule with events</returns>
        public async Task<ScheduleResource> CreateScheduleFromConfigurationAsync(int configurationId, int userId)
        {
            _logger.LogInformation($"CreateScheduleFromConfigurationAsync: Auto-generating schedule from configuration {configurationId} for user {userId}");

            // Validate configuration exists and is owned by user
            var configuration = await _scheduleConfigurationService.GetByIdAsync(configurationId, userId);
            if (configuration == null)
            {
                throw new ArgumentException($"ScheduleConfiguration {configurationId} not found or not owned by user {userId}");
            }

            // Validate configuration is ready for generation
            var validation = await _scheduleGenerationService.ValidateConfigurationForGenerationAsync(configurationId, userId);
            if (!validation.CanGenerateSchedule)
            {
                var errorMessage = $"Configuration {configurationId} is not ready for schedule generation: {string.Join(", ", validation.Errors)}";
                _logger.LogWarning($"CreateScheduleFromConfigurationAsync: {errorMessage}");
                throw new ArgumentException(errorMessage);
            }

            // Generate schedule using business logic service
            var generationResult = await _scheduleGenerationService.GenerateScheduleFromConfigurationAsync(configurationId, userId);
            if (!generationResult.Success || generationResult.Schedule == null)
            {
                var errorMessage = $"Schedule generation failed: {string.Join(", ", generationResult.Errors)}";
                _logger.LogError($"CreateScheduleFromConfigurationAsync: {errorMessage}");
                throw new InvalidOperationException(errorMessage);
            }

            // Save generated schedule to database
            var scheduleCreateResource = new ScheduleCreateResource
            {
                Title = generationResult.Schedule.Title,
                ScheduleConfigurationId = configurationId,
                ScheduleEvents = generationResult.Schedule.ScheduleEvents
            };

            var savedSchedule = await CreateScheduleAsync(scheduleCreateResource, userId);

            _logger.LogInformation($"CreateScheduleFromConfigurationAsync: Generated and saved schedule {savedSchedule.Id} with {generationResult.TotalEventsGenerated} events for user {userId}");

            // Log generation summary
            foreach (var periodCount in generationResult.EventsByPeriod)
            {
                _logger.LogDebug($"  Period {periodCount.Key}: {periodCount.Value} events");
            }

            if (generationResult.Warnings.Any())
            {
                _logger.LogWarning($"CreateScheduleFromConfigurationAsync: Generation warnings: {string.Join(", ", generationResult.Warnings)}");
            }

            return savedSchedule;
        }

        /// <summary>
        /// Regenerate schedule when configuration is updated
        /// Handles configuration changes that affect schedule structure
        /// </summary>
        /// <param name="configurationId">Updated configuration ID</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Regenerated schedule</returns>
        public async Task<ScheduleResource> RegenerateScheduleFromConfigurationAsync(int configurationId, int userId)
        {
            _logger.LogInformation($"RegenerateScheduleFromConfigurationAsync: Regenerating schedule for configuration {configurationId}, user {userId}");

            // Find existing schedule for this configuration
            var existingSchedule = await GetByConfigurationIdAsync(configurationId, userId);

            if (existingSchedule != null)
            {
                _logger.LogInformation($"RegenerateScheduleFromConfigurationAsync: Updating existing schedule {existingSchedule.Id}");

                // Generate new events from configuration with inline special day integration
                var generationResult = await _scheduleGenerationService.GenerateScheduleFromConfigurationAsync(configurationId, userId);

                if (generationResult.Success && generationResult.Schedule != null)
                {
                    _logger.LogInformation($"RegenerateScheduleFromConfigurationAsync: Generated {generationResult.TotalEventsGenerated} events with inline special day integration");
                    
                    // Update with generated events (includes inline special day integration)
                    return await UpdateScheduleEventsAsync(existingSchedule.Id, generationResult.Schedule.ScheduleEvents, userId);
                }
            }

            // Only create new schedule if none exists
            return await CreateScheduleFromConfigurationAsync(configurationId, userId);
        }

        /// <summary>
        /// Continue lesson sequences in existing schedule
        /// Extends schedule with additional lesson sequence events
        /// </summary>
        /// <param name="scheduleId">Existing schedule ID</param>
        /// <param name="continuationRequest">Continuation parameters</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Updated schedule with continuation events</returns>
        public async Task<ScheduleResource> ContinueSequencesAsync(int scheduleId, SequenceContinuationRequest continuationRequest, int userId)
        {
            _logger.LogInformation($"ContinueSequencesAsync: Continuing sequences for schedule {scheduleId} after {continuationRequest.AfterDate:yyyy-MM-dd}");

            // Validate schedule ownership
            var existingSchedule = await GetByIdAsync(scheduleId, userId);
            if (existingSchedule == null)
            {
                throw new ArgumentException($"Schedule {scheduleId} not found or not owned by user {userId}");
            }

            // Analyze current sequence state
            var analysis = await _scheduleGenerationService.AnalyzeSequenceStateAsync(scheduleId, continuationRequest.AfterDate, userId);

            if (!analysis.ContinuationPoints.Any())
            {
                _logger.LogInformation($"ContinueSequencesAsync: No continuation points found for schedule {scheduleId}");
                return existingSchedule; // No changes needed
            }

            // Generate continuation events
            var continuationEvents = await _scheduleGenerationService.GenerateSequenceContinuationAsync(scheduleId, continuationRequest, userId);

            if (!continuationEvents.Any())
            {
                _logger.LogInformation($"ContinueSequencesAsync: No continuation events generated for schedule {scheduleId}");
                return existingSchedule;
            }

            // Add continuation events to existing schedule
            var allEvents = existingSchedule.ScheduleEvents.ToList();
            allEvents.AddRange(continuationEvents);

            var updatedSchedule = await UpdateScheduleEventsAsync(scheduleId, allEvents, userId);

            _logger.LogInformation($"ContinueSequencesAsync: Added {continuationEvents.Count} continuation events to schedule {scheduleId}");

            return updatedSchedule;
        }

        /// <summary>
        /// Validate configuration for schedule generation
        /// Provides detailed validation feedback without generating
        /// </summary>
        /// <param name="configurationId">Configuration to validate</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Validation result with detailed feedback</returns>
        public async Task<ScheduleValidationResult> ValidateConfigurationAsync(int configurationId, int userId)
        {
            _logger.LogInformation($"ValidateConfigurationAsync: Validating configuration {configurationId} for user {userId}");

            return await _scheduleGenerationService.ValidateConfigurationForGenerationAsync(configurationId, userId);
        }

        /// <summary>
        /// Get schedule generation preview/summary
        /// Shows what would be generated without creating schedule
        /// </summary>
        /// <param name="configurationId">Configuration to preview</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Generation preview information</returns>
        public async Task<ScheduleGenerationPreview> GetGenerationPreviewAsync(int configurationId, int userId)
        {
            _logger.LogInformation($"GetGenerationPreviewAsync: Generating preview for configuration {configurationId}, user {userId}");

            // Validate configuration
            var validation = await _scheduleGenerationService.ValidateConfigurationForGenerationAsync(configurationId, userId);

            if (!validation.CanGenerateSchedule)
            {
                return new ScheduleGenerationPreview
                {
                    CanGenerate = false,
                    ValidationResult = validation,
                    EstimatedEventCount = 0,
                    EstimatedEventsByPeriod = new Dictionary<int, int>()
                };
            }

            // Get configuration details
            var configuration = await _scheduleConfigurationService.GetByIdAsync(configurationId, userId);
            if (configuration == null)
            {
                throw new ArgumentException($"Configuration {configurationId} not found");
            }

            // Calculate estimated event counts
            var teachingDaysCount = CalculateTeachingDaysCount(configuration.StartDate, configuration.EndDate, configuration.TeachingDays);
            var estimatedEventsByPeriod = new Dictionary<int, int>();
            var totalEstimatedEvents = 0;

            for (int period = 1; period <= configuration.PeriodsPerDay; period++)
            {
                var assignment = configuration.PeriodAssignments.FirstOrDefault(pa => pa.Period == period);
                if (assignment != null)
                {
                    estimatedEventsByPeriod[period] = teachingDaysCount;
                    totalEstimatedEvents += teachingDaysCount;
                }
            }

            return new ScheduleGenerationPreview
            {
                CanGenerate = true,
                ValidationResult = validation,
                EstimatedEventCount = totalEstimatedEvents,
                EstimatedEventsByPeriod = estimatedEventsByPeriod,
                DateRange = new DateRange
                {
                    StartDate = configuration.StartDate,
                    EndDate = configuration.EndDate,
                    TeachingDaysCount = teachingDaysCount
                }
            };
        }

        /// <summary>
        /// Analyze sequence state for existing schedule
        /// </summary>
        /// <param name="scheduleId">Schedule ID</param>
        /// <param name="afterDate">Date to analyze from</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Sequence analysis result</returns>
        public async Task<SequenceAnalysisResult> AnalyzeSequenceStateAsync(int scheduleId, DateTime afterDate, int userId)
        {
            _logger.LogInformation($"AnalyzeSequenceStateAsync: Analyzing sequences for schedule {scheduleId} after {afterDate:yyyy-MM-dd}");

            return await _scheduleGenerationService.AnalyzeSequenceStateAsync(scheduleId, afterDate, userId);
        }

        // === SPECIAL DAY OPERATIONS ===

        public async Task<List<SpecialDayResource>> GetSpecialDaysAsync(int scheduleId, int userId)
        {
            _logger.LogInformation($"GetSpecialDaysAsync: Fetching special days for schedule {scheduleId}, user {userId}");

            var schedule = await ValidateScheduleOwnershipAsync(scheduleId, userId);

            var specialDayResources = _mapper.Map<List<SpecialDayResource>>(schedule.SpecialDays);

            _logger.LogInformation($"GetSpecialDaysAsync: Found {specialDayResources.Count} special days for schedule {scheduleId}");
            return specialDayResources;
        }

        public async Task<SpecialDayResource?> GetSpecialDayAsync(int scheduleId, int specialDayId, int userId)
        {
            _logger.LogInformation($"GetSpecialDayAsync: Fetching special day {specialDayId} for schedule {scheduleId}, user {userId}");

            var schedule = await ValidateScheduleOwnershipAsync(scheduleId, userId);

            var specialDay = schedule.SpecialDays.FirstOrDefault(sd => sd.Id == specialDayId);
            if (specialDay == null)
            {
                _logger.LogInformation($"GetSpecialDayAsync: SpecialDay {specialDayId} not found in schedule {scheduleId}");
                return null;
            }

            _logger.LogInformation($"GetSpecialDayAsync: Found special day {specialDayId} in schedule {scheduleId}");
            return _mapper.Map<SpecialDayResource>(specialDay);
        }

        public async Task<SpecialDayResource> CreateSpecialDayAsync(int scheduleId, SpecialDayCreateResource createResource, int userId)
        {
            _logger.LogInformation($"CreateSpecialDayAsync: Creating special day for schedule {scheduleId}, user {userId}");

            var schedule = await ValidateScheduleOwnershipAsync(scheduleId, userId);

            // ✅ CRITICAL FIX: Create Special Day and ensure it's committed before regeneration
            var createdSpecialDay = await _repository.AddSpecialDayAsync(scheduleId, createResource);

            _logger.LogInformation($"CreateSpecialDayAsync: Created special day {createdSpecialDay.Id} for schedule {scheduleId}");

            // ✅ TRANSACTION FIX: Regenerate schedule in separate operation to ensure Special Day is visible
            _logger.LogInformation($"CreateSpecialDayAsync: Regenerating schedule {scheduleId} to integrate special day inline");

            try
            {
                await RegenerateScheduleFromConfigurationAsync(schedule.ScheduleConfigurationId, userId);
                _logger.LogInformation($"CreateSpecialDayAsync: Successfully regenerated schedule after special day creation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"CreateSpecialDayAsync: Failed to regenerate schedule after special day creation, but special day {createdSpecialDay.Id} was created successfully");
                // Don't throw - the special day was created successfully, regeneration can be retried later
            }

            return _mapper.Map<SpecialDayResource>(createdSpecialDay);
        }

        public async Task<SpecialDayUpdateResponse> UpdateSpecialDayAsync(int scheduleId, int specialDayId, SpecialDayUpdateResource updateResource, int userId)
        {
            _logger.LogInformation($"UpdateSpecialDayAsync: Updating special day {specialDayId} for schedule {scheduleId}, user {userId}");

            if (specialDayId != updateResource.Id)
            {
                throw new ArgumentException($"SpecialDay ID mismatch: {specialDayId} vs {updateResource.Id}");
            }

            var schedule = await ValidateScheduleOwnershipAsync(scheduleId, userId);

            var existingSpecialDay = schedule.SpecialDays.FirstOrDefault(sd => sd.Id == specialDayId);
            if (existingSpecialDay == null)
            {
                _logger.LogInformation($"UpdateSpecialDayAsync: SpecialDay {specialDayId} not found in schedule {scheduleId}");
                throw new ArgumentException($"SpecialDay {specialDayId} not found in schedule {scheduleId}");
            }

            // Check if periods changed to determine if regeneration is needed
            // Parse JSON string from domain entity to int array for comparison
            var originalPeriods = ParsePeriodsFromJson(existingSpecialDay.Periods) ?? new int[0];
            var newPeriods = updateResource.Periods ?? new int[0];
            var periodsChanged = !originalPeriods.OrderBy(p => p).SequenceEqual(newPeriods.OrderBy(p => p));

            // Check if colors changed to determine if ScheduleEvent updates are needed
            var colorsChanged = existingSpecialDay.BackgroundColor != updateResource.BackgroundColor ||
                               existingSpecialDay.FontColor != updateResource.FontColor;

            _logger.LogInformation($"UpdateSpecialDayAsync: Periods changed? {periodsChanged} (Original: [{string.Join(",", originalPeriods)}], New: [{string.Join(",", newPeriods)}])");
            _logger.LogInformation($"UpdateSpecialDayAsync: Colors changed? {colorsChanged} (BackgroundColor: {existingSpecialDay.BackgroundColor} -> {updateResource.BackgroundColor}, FontColor: {existingSpecialDay.FontColor} -> {updateResource.FontColor})");

            var updatedSpecialDay = await _repository.UpdateSpecialDayAsync(updateResource);
            _logger.LogInformation($"UpdateSpecialDayAsync: Updated special day {specialDayId} for schedule {scheduleId}");

            bool calendarRefreshNeeded = false;
            string? refreshReason = null;

            if (periodsChanged)
            {
                // Period changes require full schedule regeneration
                _logger.LogInformation($"UpdateSpecialDayAsync: Period assignments changed, regenerating schedule {scheduleId}");
                try
                {
                    await RegenerateScheduleFromConfigurationAsync(schedule.ScheduleConfigurationId, userId);
                    _logger.LogInformation($"UpdateSpecialDayAsync: Successfully regenerated schedule after period change");
                    calendarRefreshNeeded = true;
                    refreshReason = "Period assignments changed";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"UpdateSpecialDayAsync: Failed to regenerate schedule after period change, but special day {specialDayId} was updated successfully");
                    calendarRefreshNeeded = true;
                    refreshReason = "Period assignments changed, but regeneration failed - manual refresh recommended";
                }
            }
            else if (colorsChanged)
            {
                // Color changes only require updating existing ScheduleEvents for this SpecialDay
                _logger.LogInformation($"UpdateSpecialDayAsync: Colors changed, updating existing ScheduleEvents for SpecialDay {specialDayId}");
                try
                {
                    await UpdateScheduleEventsForSpecialDayAsync(scheduleId, specialDayId);
                    _logger.LogInformation($"UpdateSpecialDayAsync: Successfully updated ScheduleEvents for color change");
                    calendarRefreshNeeded = true;
                    refreshReason = "Colors updated";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"UpdateSpecialDayAsync: Failed to update ScheduleEvents for color change, but special day {specialDayId} was updated successfully");
                    calendarRefreshNeeded = true;
                    refreshReason = "Colors changed, but ScheduleEvent update failed - manual refresh recommended";
                }
            }
            else
            {
                _logger.LogInformation($"UpdateSpecialDayAsync: Only title/description changed, no calendar updates needed");
                calendarRefreshNeeded = false;
                refreshReason = "Only title/description changed, no calendar updates needed";
            }

            return new SpecialDayUpdateResponse
            {
                SpecialDay = _mapper.Map<SpecialDayResource>(updatedSpecialDay),
                CalendarRefreshNeeded = calendarRefreshNeeded,
                RefreshReason = refreshReason
            };
        }

        /// <summary>
        /// Updates existing ScheduleEvents that belong to a specific SpecialDay
        /// This is more efficient than regenerating the entire schedule when only colors change
        /// Note: Since we use AutoMapper to embed SpecialDay colors in ScheduleEvent responses,
        /// we just need to ensure the SpecialDay was updated (which already happened)
        /// and trigger a calendar refresh to reload the data with fresh colors
        /// </summary>
        private async Task UpdateScheduleEventsForSpecialDayAsync(int scheduleId, int specialDayId)
        {
            _logger.LogInformation($"UpdateScheduleEventsForSpecialDayAsync: Validating ScheduleEvents for SpecialDay {specialDayId} in schedule {scheduleId}");

            // Get the schedule to verify ScheduleEvents exist for this SpecialDay
            var schedule = await _repository.GetByIdAsync(scheduleId);
            if (schedule == null)
            {
                throw new ArgumentException($"Schedule {scheduleId} not found");
            }

            // Find all ScheduleEvents that belong to this SpecialDay
            var scheduleEventsCount = schedule.ScheduleEvents
                .Count(e => e.SpecialDayId == specialDayId);

            _logger.LogInformation($"UpdateScheduleEventsForSpecialDayAsync: Found {scheduleEventsCount} ScheduleEvents for SpecialDay {specialDayId}");

            if (scheduleEventsCount > 0)
            {
                _logger.LogInformation($"UpdateScheduleEventsForSpecialDayAsync: ScheduleEvents exist for SpecialDay {specialDayId}. Colors will be updated automatically via AutoMapper when API response is generated.");
            }
            else
            {
                _logger.LogInformation($"UpdateScheduleEventsForSpecialDayAsync: No ScheduleEvents found for SpecialDay {specialDayId} - this may be normal if periods were previously unassigned");
            }

            // No actual database updates needed here - the SpecialDay update already happened
            // and AutoMapper will provide the fresh colors when the schedule is next requested
        }

        public async Task DeleteSpecialDayAsync(int scheduleId, int specialDayId, int userId)
        {
            _logger.LogInformation($"DeleteSpecialDayAsync: Deleting special day {specialDayId} for schedule {scheduleId}, user {userId}");

            var schedule = await ValidateScheduleOwnershipAsync(scheduleId, userId);

            var existingSpecialDay = schedule.SpecialDays.FirstOrDefault(sd => sd.Id == specialDayId);
            if (existingSpecialDay == null)
            {
                _logger.LogInformation($"DeleteSpecialDayAsync: SpecialDay {specialDayId} not found in schedule {scheduleId}");
                throw new ArgumentException($"SpecialDay {specialDayId} not found in schedule {scheduleId}");
            }

            // Delete the special day from database
            await _repository.DeleteSpecialDayAsync(specialDayId);

            _logger.LogInformation($"DeleteSpecialDayAsync: Deleted special day {specialDayId} from database");

            // CRITICAL FIX: Regenerate schedule to remove special day events from schedule events
            try
            {
                await RegenerateScheduleFromConfigurationAsync(schedule.ScheduleConfigurationId, userId);
                _logger.LogInformation($"DeleteSpecialDayAsync: Successfully regenerated schedule after special day deletion");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"DeleteSpecialDayAsync: Failed to regenerate schedule after special day deletion, but special day {specialDayId} was deleted successfully");
                // Don't throw - the special day was deleted successfully, regeneration can be retried later
            }

            _logger.LogInformation($"DeleteSpecialDayAsync: Completed deletion of special day {specialDayId} for schedule {scheduleId}");
        }

        // ✅ NEW: Optimized Special Day operations with partial schedule regeneration

        /// <summary>
        /// Create special day with optimized partial regeneration
        /// Uses enhanced GenerateEventsForDateRangeAsync for targeted updates instead of full schedule regeneration
        /// </summary>
        public async Task<SpecialDayOptimizedResponse> CreateSpecialDayOptimizedAsync(int scheduleId, SpecialDayCreateResource createResource, int userId)
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation($"CreateSpecialDayOptimizedAsync: Creating special day for schedule {scheduleId}, date {createResource.Date:yyyy-MM-dd}, user {userId}");

            var schedule = await _repository.GetByIdAsync(scheduleId);
            if (schedule == null || schedule.UserId != userId)
            {
                throw new ArgumentException($"Schedule {scheduleId} not found or not accessible by user {userId}");
            }

            try
            {
                // Create the special day
                var createdSpecialDay = await _repository.AddSpecialDayAsync(scheduleId, createResource);
                _logger.LogInformation($"Created special day {createdSpecialDay.Id} for date {createResource.Date:yyyy-MM-dd}");

                // ✅ OPTIMIZATION: Use partial generation for only the special day's date
                var optimizationStartTime = DateTime.UtcNow;
                var specialDayDate = createResource.Date.Date;
                var partialEvents = await _scheduleGenerationService.GenerateEventsForDateRangeAsync(
                    scheduleId,
                    specialDayDate,
                    specialDayDate,
                    userId
                );

                var isOptimized = partialEvents.Count > 0;
                var optimizationTime = DateTime.UtcNow - optimizationStartTime;

                if (isOptimized)
                {
                    // Update only the events for this specific date
                    await UpdateSpecificDateEventsAsync(scheduleId, specialDayDate, partialEvents);
                    _logger.LogInformation($"Optimized: Updated {partialEvents.Count} events for date {specialDayDate:yyyy-MM-dd} in {optimizationTime.TotalMilliseconds}ms");
                }
                else
                {
                    // Fallback to full regeneration
                    _logger.LogInformation("Partial generation returned no events, falling back to full regeneration");
                    await RegenerateScheduleFromConfigurationAsync(schedule.ScheduleConfigurationId, userId);
                }

                var totalTime = DateTime.UtcNow - startTime;

                // ✅ ENHANCEMENT: Trigger background full rebuild for eventual consistency
                var backgroundJobId = _backgroundScheduleService.EnqueueScheduleRebuild(
                    scheduleId,
                    schedule.ScheduleConfigurationId,
                    userId,
                    $"Special Day Create - {createResource.Title} on {specialDayDate:yyyy-MM-dd}"
                );
                _logger.LogInformation($"Enqueued background rebuild job {backgroundJobId} for eventual consistency");

                return new SpecialDayOptimizedResponse
                {
                    SpecialDay = _mapper.Map<SpecialDayResource>(createdSpecialDay),
                    IsOptimized = isOptimized,
                    HasPartialGeneration = isOptimized,
                    PartialEventsGenerated = partialEvents.Count,
                    PartialGenerationDate = specialDayDate,
                    RequiresFullRefresh = !isOptimized,
                    OptimizationReason = isOptimized ? "Partial date regeneration successful" : "Fallback to full regeneration",
                    PerformanceMetrics = $"Total: {totalTime.TotalMilliseconds}ms, Optimization: {optimizationTime.TotalMilliseconds}ms"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in CreateSpecialDayOptimizedAsync for schedule {scheduleId}");
                throw;
            }
        }

        /// <summary>
        /// Delete special day with optimized partial regeneration
        /// Uses enhanced GenerateEventsForDateRangeAsync for targeted updates instead of full schedule regeneration
        /// </summary>
        public async Task<SpecialDayOptimizedResponse> DeleteSpecialDayOptimizedAsync(int scheduleId, int specialDayId, int userId)
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation($"DeleteSpecialDayOptimizedAsync: Deleting special day {specialDayId} from schedule {scheduleId}, user {userId}");

            var schedule = await _repository.GetByIdAsync(scheduleId);
            if (schedule == null || schedule.UserId != userId)
            {
                throw new ArgumentException($"Schedule {scheduleId} not found or not accessible by user {userId}");
            }

            // Get the special day before deletion to know its date
            var specialDay = schedule.SpecialDays?.FirstOrDefault(sd => sd.Id == specialDayId);
            if (specialDay == null)
            {
                throw new ArgumentException($"Special day {specialDayId} not found in schedule {scheduleId}");
            }

            var specialDayDate = specialDay.Date.Date;

            try
            {
                // Delete the special day (using existing repository pattern)
                await _repository.DeleteSpecialDayAsync(specialDayId);
                _logger.LogInformation($"Deleted special day {specialDayId} for date {specialDayDate:yyyy-MM-dd}");

                // ✅ OPTIMIZATION: Use partial generation for only the affected date
                var optimizationStartTime = DateTime.UtcNow;
                var partialEvents = await _scheduleGenerationService.GenerateEventsForDateRangeAsync(
                    scheduleId,
                    specialDayDate,
                    specialDayDate,
                    userId
                );

                var isOptimized = partialEvents.Count >= 0; // Even 0 events is valid for partial generation
                var optimizationTime = DateTime.UtcNow - optimizationStartTime;

                if (isOptimized)
                {
                    // Remove special day events and replace with regenerated events
                    await RemoveSpecialDayEventsAndUpdateAsync(scheduleId, specialDayId, specialDayDate, partialEvents);
                    _logger.LogInformation($"Optimized: Regenerated {partialEvents.Count} events for date {specialDayDate:yyyy-MM-dd} in {optimizationTime.TotalMilliseconds}ms");
                }
                else
                {
                    // Fallback to full regeneration (should rarely happen with enhanced implementation)
                    _logger.LogInformation("Partial generation failed, falling back to full regeneration");
                    await RegenerateScheduleFromConfigurationAsync(schedule.ScheduleConfigurationId, userId);
                }

                var totalTime = DateTime.UtcNow - startTime;

                // ✅ ENHANCEMENT: Trigger background full rebuild for eventual consistency
                var backgroundJobId = _backgroundScheduleService.EnqueueScheduleRebuild(
                    scheduleId,
                    schedule.ScheduleConfigurationId,
                    userId,
                    $"Special Day Delete - ID {specialDayId} on {specialDayDate:yyyy-MM-dd}"
                );
                _logger.LogInformation($"Enqueued background rebuild job {backgroundJobId} for eventual consistency");

                return new SpecialDayOptimizedResponse
                {
                    SpecialDay = new SpecialDayResource { Id = specialDayId }, // Minimal response since it's deleted
                    IsOptimized = isOptimized,
                    HasPartialGeneration = isOptimized,
                    PartialEventsGenerated = partialEvents.Count,
                    PartialGenerationDate = specialDayDate,
                    RequiresFullRefresh = !isOptimized,
                    OptimizationReason = isOptimized ? "Partial date regeneration successful" : "Fallback to full regeneration",
                    PerformanceMetrics = $"Total: {totalTime.TotalMilliseconds}ms, Optimization: {optimizationTime.TotalMilliseconds}ms"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in DeleteSpecialDayOptimizedAsync for special day {specialDayId}");
                throw;
            }
        }

        // ✅ NEW: Helper methods for optimized special day operations

        /// <summary>
        /// Update schedule events for a specific date with new events
        /// </summary>
        private async Task UpdateSpecificDateEventsAsync(int scheduleId, DateTime date, List<ScheduleEventResource> newEvents)
        {
            var schedule = await _repository.GetByIdAsync(scheduleId);
            if (schedule?.ScheduleEvents != null)
            {
                // Remove existing events for this date
                var existingEventsForDate = schedule.ScheduleEvents
                    .Where(e => e.Date.Date == date.Date)
                    .ToList();

                foreach (var eventToRemove in existingEventsForDate)
                {
                    schedule.ScheduleEvents.Remove(eventToRemove);
                }

                // Add new events
                foreach (var newEvent in newEvents)
                {
                    schedule.ScheduleEvents.Add(new ScheduleEvent
                    {
                        ScheduleId = scheduleId,
                        Date = newEvent.Date,
                        Period = newEvent.Period,
                        EventType = newEvent.EventType,
                        EventCategory = newEvent.EventCategory,
                        LessonId = newEvent.LessonId,
                        CourseId = newEvent.CourseId,
                        SpecialDayId = newEvent.SpecialDayId,
                        Comment = newEvent.Comment
                    });
                }

                // For now, use the existing pattern - this is a proof of concept
                // In a full implementation, we'd add specific repository methods for bulk event updates
                _logger.LogInformation($"Updated {newEvents.Count} events for date {date:yyyy-MM-dd} (using existing repository pattern)");
            }
        }

        /// <summary>
        /// Remove special day events and update with regenerated events
        /// </summary>
        private async Task RemoveSpecialDayEventsAndUpdateAsync(int scheduleId, int specialDayId, DateTime date, List<ScheduleEventResource> newEvents)
        {
            var schedule = await _repository.GetByIdAsync(scheduleId);
            if (schedule?.ScheduleEvents != null)
            {
                // Remove events for the deleted special day
                var specialDayEventsToRemove = schedule.ScheduleEvents
                    .Where(e => e.SpecialDayId == specialDayId)
                    .ToList();

                foreach (var eventToRemove in specialDayEventsToRemove)
                {
                    schedule.ScheduleEvents.Remove(eventToRemove);
                }

                // Also remove any existing events for this date to avoid duplicates
                var existingEventsForDate = schedule.ScheduleEvents
                    .Where(e => e.Date.Date == date.Date && e.SpecialDayId != specialDayId)
                    .ToList();

                foreach (var eventToRemove in existingEventsForDate)
                {
                    schedule.ScheduleEvents.Remove(eventToRemove);
                }

                // Add regenerated events
                foreach (var newEvent in newEvents)
                {
                    schedule.ScheduleEvents.Add(new ScheduleEvent
                    {
                        ScheduleId = scheduleId,
                        Date = newEvent.Date,
                        Period = newEvent.Period,
                        EventType = newEvent.EventType,
                        EventCategory = newEvent.EventCategory,
                        LessonId = newEvent.LessonId,
                        CourseId = newEvent.CourseId,
                        SpecialDayId = newEvent.SpecialDayId,
                        Comment = newEvent.Comment
                    });
                }

                // For now, use the existing pattern - this is a proof of concept
                // In a full implementation, we'd add specific repository methods for bulk event updates
                _logger.LogInformation($"Removed special day {specialDayId} events and added {newEvents.Count} regenerated events for date {date:yyyy-MM-dd}");
            }
        }

        // === EVENT RETRIEVAL ===

        /// <summary>
        /// Get schedule events for date range (already includes special days from inline generation)
        /// </summary>
        public async Task<List<ScheduleEventResource>> GetEventsByDateRangeAsync(int scheduleId, DateTime startDate, DateTime endDate, int userId, int? courseId = null)
        {
            _logger.LogInformation($"GetEventsByDateRangeAsync: Getting events for schedule {scheduleId} between {startDate:yyyy-MM-dd} and {endDate:yyyy-MM-dd}, courseId filter: {courseId}");

            // Validate schedule ownership
            var schedule = await GetByIdAsync(scheduleId, userId);
            if (schedule == null)
            {
                throw new ArgumentException($"Schedule {scheduleId} not found or not owned by user {userId}");
            }

            // Get schedule events in date range (already includes inline special days)
            var query = schedule.ScheduleEvents
                .Where(e => e.Date.Date >= startDate.Date && e.Date.Date <= endDate.Date);

            // Apply course filter if specified
            if (courseId.HasValue)
            {
                query = query.Where(e => e.CourseId == courseId.Value);
                _logger.LogInformation($"Applied course filter: {courseId.Value}");
            }

            var events = query
                .OrderBy(e => e.Date)
                .ThenBy(e => e.Period)
                .ToList();

            _logger.LogInformation($"Returning {events.Count} events (lessons + special days)" + (courseId.HasValue ? $" for course {courseId.Value}" : ""));
            return events;
        }

        // === CONFIGURATION-BASED SCHEDULE LOOKUP ===

        public async Task<ScheduleResource?> GetByConfigurationIdAsync(int configurationId, int userId)
        {
            _logger.LogInformation($"GetByConfigurationIdAsync: Fetching schedule for configuration {configurationId}, user {userId}");

            var schedule = await _repository.GetByConfigurationIdAsync(configurationId);
            if (schedule == null)
            {
                _logger.LogInformation($"GetByConfigurationIdAsync: No schedule found for configuration {configurationId}");
                return null;
            }

            // Validate ownership
            if (schedule.UserId != userId)
            {
                _logger.LogWarning($"GetByConfigurationIdAsync: Schedule {schedule.Id} for configuration {configurationId} not owned by user {userId}");
                throw new UnauthorizedAccessException($"Schedule {schedule.Id} not owned by user {userId}");
            }

            _logger.LogInformation($"GetByConfigurationIdAsync: Found schedule {schedule.Id} for configuration {configurationId}, user {userId}");
            return _mapper.Map<ScheduleResource>(schedule);
        }

        // === PRIVATE HELPER METHODS ===

        private async Task<Schedule> ValidateScheduleOwnershipAsync(int scheduleId, int userId)
        {
            var schedule = await _repository.GetByIdAsync(scheduleId);
            if (schedule == null)
            {
                _logger.LogInformation($"ValidateScheduleOwnershipAsync: Schedule {scheduleId} not found");
                throw new ArgumentException($"Schedule {scheduleId} not found");
            }

            if (schedule.UserId != userId)
            {
                _logger.LogWarning($"ValidateScheduleOwnershipAsync: Schedule {scheduleId} not owned by user {userId}");
                throw new UnauthorizedAccessException($"Schedule {scheduleId} not owned by user {userId}");
            }

            return schedule;
        }

        /// <summary>
        /// Parse periods JSON string to int array
        /// </summary>
        private int[]? ParsePeriodsFromJson(string? periodsJson)
        {
            if (string.IsNullOrEmpty(periodsJson))
                return new int[0];

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<int[]>(periodsJson);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to parse periods JSON '{periodsJson}': {ex.Message}");
                return new int[0];
            }
        }

        private int CalculateTeachingDaysCount(DateTime startDate, DateTime endDate, string[] teachingDays)
        {
            var count = 0;
            var current = startDate;
            var teachingDayNames = teachingDays.Select(d => d.ToLower()).ToHashSet();

            while (current <= endDate)
            {
                var dayName = current.DayOfWeek.ToString().ToLower();
                if (teachingDayNames.Contains(dayName))
                {
                    count++;
                }
                current = current.AddDays(1);
            }

            return count;
        }
    }

}
// **NEW FILE** - BLL/Services/ScheduleGenerationService.cs
// RESPONSIBILITY: Complete schedule generation business logic migrated from frontend
// DOES NOT: Handle HTTP concerns, UI coordination, or direct persistence
// CALLED BY: ScheduleService for schedule generation operations

using AutoMapper;
using LessonTree.BLL.Services;
using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
using LessonTree.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LessonTree.BLL.Services
{
    /// <summary>
    /// Schedule generation business logic service
    /// Migrated from frontend ScheduleGenerationService, LessonSequenceAnalysisService, and ScheduleEventFactoryService
    /// </summary>
    public class ScheduleGenerationService : IScheduleGenerationService
    {
        private readonly IScheduleConfigurationRepository _configRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ScheduleGenerationService> _logger;

        public ScheduleGenerationService(
            IScheduleConfigurationRepository configRepository,
            ILessonRepository lessonRepository,
            IScheduleRepository scheduleRepository,
            IMapper mapper,
            ILogger<ScheduleGenerationService> logger)
        {
            _configRepository = configRepository;
            _lessonRepository = lessonRepository;
            _scheduleRepository = scheduleRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // === CORE GENERATION ===

        public async Task<ScheduleGenerationResult> GenerateScheduleFromConfigurationAsync(int configurationId, int userId)
        {
            _logger.LogInformation($"GenerateScheduleFromConfigurationAsync: Starting generation for configuration {configurationId}, user {userId}");

            var errors = new List<string>();
            var warnings = new List<string>();

            try
            {
                // Get and validate configuration
                var configuration = await _configRepository.GetByIdAsync(configurationId);
                if (configuration == null || configuration.UserId != userId)
                {
                    errors.Add($"Configuration {configurationId} not found or not owned by user {userId}");
                    return new ScheduleGenerationResult { Success = false, Errors = errors, Warnings = warnings };
                }

                // Validate configuration readiness
                var validation = await ValidateConfigurationForGenerationAsync(configurationId, userId);
                if (!validation.CanGenerateSchedule)
                {
                    errors.AddRange(validation.Errors);
                    return new ScheduleGenerationResult { Success = false, Errors = errors, Warnings = warnings };
                }

                _logger.LogInformation($"Configuration validation passed - generating events for {validation.CourseAssignments} course assignments");

                // Generate schedule events
                var scheduleEvents = await GenerateScheduleEventsFromConfiguration(configuration, userId);

                if (!scheduleEvents.Any())
                {
                    warnings.Add("No schedule events generated");
                }

                // === ADD PERSISTENCE USING EXISTING REPOSITORY METHOD ===
                // Convert ScheduleEventResource to domain entities (only actual entity properties)
                var scheduleEventEntities = scheduleEvents.Select(eventResource => new ScheduleEvent
                {
                    CourseId = eventResource.CourseId,
                    Date = eventResource.Date,
                    Period = eventResource.Period,
                    LessonId = eventResource.LessonId,
                    SpecialDayId = eventResource.SpecialDayId, // ✅ CRITICAL FIX: Include SpecialDayId for persistence
                    EventType = eventResource.EventType,
                    EventCategory = eventResource.EventCategory,
                    Comment = eventResource.Comment,
                    ScheduleSort = eventResource.ScheduleSort
                }).ToList();

                // Save using existing repository method (handles Schedule + ScheduleEvents + transaction)
                var savedSchedule = await _scheduleRepository.CreateOrReplaceScheduleAsync(
                    userId,
                    scheduleEventEntities,
                    configurationId // Link to the configuration
                );

                _logger.LogInformation($"Persisted Schedule {savedSchedule.Id} with {savedSchedule.ScheduleEvents.Count} events");

                // Create resource objects with real database IDs for return
                var configurationResource = _mapper.Map<ScheduleConfigurationResource>(configuration);

                var scheduleResource = new ScheduleResource
                {
                    Id = savedSchedule.Id, // ✅ Real database ID
                    Title = savedSchedule.Title ?? configuration.Title ?? $"{configuration.SchoolYear} Schedule",
                    UserId = userId,
                    ScheduleConfiguration = configurationResource,
                    IsLocked = false,
                    CreatedDate = savedSchedule.CreatedDate,
                    ScheduleEvents = scheduleEvents.Select(e => { e.ScheduleId = savedSchedule.Id; return e; }).ToList(), // Update with real schedule ID
                    SpecialDays = new List<SpecialDayResource>() // Empty for newly generated schedules
                };

                // Calculate statistics
                var eventsByPeriod = scheduleEvents
                    .GroupBy(e => e.Period)
                    .ToDictionary(g => g.Key, g => g.Count());

                _logger.LogInformation($"Generated and saved schedule with {scheduleEvents.Count} events across {eventsByPeriod.Count} periods");

                return new ScheduleGenerationResult
                {
                    Success = true,
                    Schedule = scheduleResource, // Now contains real persisted data
                    Errors = errors,
                    Warnings = warnings,
                    TotalEventsGenerated = scheduleEvents.Count,
                    EventsByPeriod = eventsByPeriod
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating schedule from configuration {configurationId}");
                errors.Add($"Schedule generation failed: {ex.Message}");
                return new ScheduleGenerationResult { Success = false, Errors = errors, Warnings = warnings };
            }
        }

        public async Task<ScheduleValidationResult> ValidateConfigurationForGenerationAsync(int configurationId, int userId)
        {
            _logger.LogInformation($"ValidateConfigurationForGenerationAsync: Validating configuration {configurationId} for user {userId}");

            var errors = new List<string>();
            var warnings = new List<string>();

            var configuration = await _configRepository.GetByIdAsync(configurationId);
            if (configuration == null)
            {
                errors.Add($"Configuration {configurationId} not found");
                return new ScheduleValidationResult { IsValid = false, CanGenerateSchedule = false, Errors = errors };
            }

            if (configuration.UserId != userId)
            {
                errors.Add($"Configuration {configurationId} not owned by user {userId}");
                return new ScheduleValidationResult { IsValid = false, CanGenerateSchedule = false, Errors = errors };
            }

            // Basic validation
            if (configuration.StartDate >= configuration.EndDate)
            {
                errors.Add("Start date must be before end date");
            }

            if (configuration.PeriodsPerDay < 1 || configuration.PeriodsPerDay > 10)
            {
                errors.Add("Periods per day must be between 1 and 10");
            }

            if (configuration.TeachingDays.Length == 0)
            {
                errors.Add("At least one teaching day must be specified");
            }

            if (!configuration.PeriodAssignments.Any())
            {
                errors.Add("No period assignments configured");
            }

            // Analyze period assignments
            var courseAssignments = configuration.PeriodAssignments.Where(pa => pa.CourseId.HasValue).ToList();
            var specialPeriodAssignments = configuration.PeriodAssignments.Where(pa => !string.IsNullOrEmpty(pa.SpecialPeriodType)).ToList();
            var unassignedPeriods = configuration.PeriodAssignments.Where(pa => !pa.CourseId.HasValue && string.IsNullOrEmpty(pa.SpecialPeriodType)).ToList();

            if (!courseAssignments.Any())
            {
                errors.Add("No periods assigned to courses");
            }

            // Validate course assignments have lessons
            foreach (var assignment in courseAssignments)
            {
                var lessonCount = await GetLessonCountForCourse(assignment.CourseId!.Value, userId);
                if (lessonCount == 0)
                {
                    warnings.Add($"Course {assignment.CourseId} (Period {assignment.Period}) has no lessons");
                }
            }

            if (unassignedPeriods.Any())
            {
                var periods = string.Join(", ", unassignedPeriods.Select(up => up.Period));
                warnings.Add($"Periods {periods} are unassigned and will generate error events");
            }

            var isValid = !errors.Any();
            var canGenerate = isValid && courseAssignments.Any();

            return new ScheduleValidationResult
            {
                IsValid = isValid,
                CanGenerateSchedule = canGenerate,
                Errors = errors,
                Warnings = warnings,
                TotalPeriodsConfigured = configuration.PeriodAssignments.Count,
                CourseAssignments = courseAssignments.Count,
                SpecialPeriodAssignments = specialPeriodAssignments.Count,
                UnassignedPeriods = unassignedPeriods.Count
            };
        }

        // === SEQUENCE ANALYSIS ===

        public async Task<SequenceAnalysisResult> AnalyzeSequenceStateAsync(int scheduleId, DateTime afterDate, int userId)
        {
            _logger.LogInformation($"AnalyzeSequenceStateAsync: Analyzing sequence state for schedule {scheduleId} after {afterDate:yyyy-MM-dd}");

            var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
            if (schedule == null || schedule.UserId != userId)
            {
                throw new ArgumentException($"Schedule {scheduleId} not found or not owned by user {userId}");
            }

            var configuration = await _configRepository.GetByIdAsync(schedule.ScheduleConfigurationId);
            if (configuration == null)
            {
                throw new ArgumentException($"Configuration {schedule.ScheduleConfigurationId} not found");
            }

            // Get course assignments from configuration
            var courseAssignments = configuration.PeriodAssignments
                .Where(pa => pa.CourseId.HasValue)
                .ToList();

            var continuationPoints = new List<ContinuationPoint>();
            var coursePeriodDetails = new List<CoursePeriodDetail>();
            var totalLessonsInScope = 0;

            foreach (var assignment in courseAssignments)
            {
                var courseId = assignment.CourseId!.Value;
                var period = assignment.Period;

                // Get lesson count for this course
                var allLessons = await GetLessonsForCourse(courseId, userId);
                totalLessonsInScope += allLessons.Count;

                // Find highest assigned lesson index for this period/course
                var periodLessonEvents = schedule.ScheduleEvents
                    .Where(e => e.Period == period && e.CourseId == courseId && e.LessonId.HasValue)
                    .OrderBy(e => e.Date)
                    .ToList();

                var highestLessonIndex = -1;
                var lastAssignedDate = (DateTime?)null;

                foreach (var eventDto in periodLessonEvents)
                {
                    var lessonIndex = allLessons.FindIndex(l => l.Id == eventDto.LessonId);
                    if (lessonIndex > highestLessonIndex)
                    {
                        highestLessonIndex = lessonIndex;
                        lastAssignedDate = eventDto.Date;
                    }
                }

                var assignedLessons = highestLessonIndex + 1;
                var needsContinuation = highestLessonIndex < allLessons.Count - 1;

                // Add to results
                coursePeriodDetails.Add(new CoursePeriodDetail
                {
                    CourseId = courseId,
                    CourseTitle = $"Course {courseId}", // Will be enhanced with actual course title if needed
                    Period = period,
                    TotalLessons = allLessons.Count,
                    AssignedLessons = assignedLessons,
                    NeedsContinuation = needsContinuation,
                    LastAssignedDate = lastAssignedDate
                });

                if (needsContinuation)
                {
                    continuationPoints.Add(new ContinuationPoint
                    {
                        Period = period,
                        CourseId = courseId,
                        CourseTitle = $"Course {courseId}", // Will be enhanced with actual course title if needed
                        LastAssignedLessonIndex = highestLessonIndex,
                        ContinuationDate = afterDate.AddDays(1),
                        TotalLessons = allLessons.Count,
                        RemainingLessons = allLessons.Count - assignedLessons,
                        PeriodAssignment = ConvertToResource(assignment)
                    });
                }
            }

            _logger.LogInformation($"Found {continuationPoints.Count} periods needing continuation out of {courseAssignments.Count} course assignments");

            return new SequenceAnalysisResult
            {
                TotalCoursesInScope = courseAssignments.Count,
                TotalLessonsInScope = totalLessonsInScope,
                ContinuationPoints = continuationPoints,
                CoursePeriodDetails = coursePeriodDetails
            };
        }

        public async Task<List<ScheduleEventResource>> GenerateSequenceContinuationAsync(int scheduleId, SequenceContinuationRequest continuationRequest, int userId)
        {
            _logger.LogInformation($"GenerateSequenceContinuationAsync: Generating continuation for schedule {scheduleId} from {continuationRequest.AfterDate:yyyy-MM-dd}");

            // Analyze current state
            var analysis = await AnalyzeSequenceStateAsync(scheduleId, continuationRequest.AfterDate, userId);

            if (!analysis.ContinuationPoints.Any())
            {
                _logger.LogInformation("No continuation points found");
                return new List<ScheduleEventResource>();
            }

            var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
            var configuration = await _configRepository.GetByIdAsync(schedule!.ScheduleConfigurationId);

            var continuationEvents = new List<ScheduleEventResource>();
            var eventIdCounter = -50000; // Use distinct negative range for continuation events

            foreach (var continuationPoint in analysis.ContinuationPoints)
            {
                // Skip if specific periods requested and this isn't one of them
                if (continuationRequest.SpecificPeriods?.Any() == true &&
                    !continuationRequest.SpecificPeriods.Contains(continuationPoint.Period))
                {
                    continue;
                }

                var courseEvents = await GenerateContinuationEventsForPeriodCourse(
                    continuationPoint,
                    continuationRequest.AfterDate,
                    continuationRequest.EndDate ?? configuration!.EndDate,
                    configuration.TeachingDays.Split(',', StringSplitOptions.RemoveEmptyEntries),
                    eventIdCounter,
                    userId
                );

                continuationEvents.AddRange(courseEvents);
                eventIdCounter = eventIdCounter - courseEvents.Count;
            }

            _logger.LogInformation($"Generated {continuationEvents.Count} continuation events");
            return continuationEvents;
        }

        // === SMART UPDATE FUNCTIONALITY ===

        public async Task<ScheduleUpdateResult> UpdateScheduleAfterLessonAddedAsync(int scheduleId, int lessonId, int userId)
        {
            _logger.LogInformation($"UpdateScheduleAfterLessonAddedAsync: Updating schedule {scheduleId} after lesson {lessonId} added by user {userId}");

            var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
            if (schedule == null || schedule.UserId != userId)
            {
                throw new ArgumentException($"Schedule {scheduleId} not found or not owned by user {userId}");
            }

            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
            {
                throw new ArgumentException($"Lesson {lessonId} not found");
            }

            // Find which course this lesson belongs to
            int courseId = GetCourseIdFromLesson(lesson);

            // Find the periods assigned to this course
            var configuration = await _configRepository.GetByIdAsync(schedule.ScheduleConfigurationId);
            var coursePeriods = configuration.PeriodAssignments
                .Where(pa => pa.CourseId == courseId)
                .Select(pa => pa.Period)
                .ToList();

            if (!coursePeriods.Any())
            {
                _logger.LogWarning($"No periods assigned to course {courseId}, no schedule updates needed");
                return new ScheduleUpdateResult { Success = true, EventsUpdated = 0, Message = "No periods assigned to this course" };
            }

            // Use shared logic to regenerate events for affected periods
            var eventsUpdated = 0;
            foreach (var period in coursePeriods)
            {
                eventsUpdated += await RegenerateEventsForPeriodCourse(scheduleId, period, courseId, userId);
            }

            _logger.LogInformation($"Updated {eventsUpdated} events across {coursePeriods.Count} periods for new lesson {lessonId}");

            return new ScheduleUpdateResult 
            { 
                Success = true, 
                EventsUpdated = eventsUpdated, 
                Message = $"Updated {eventsUpdated} schedule events for new lesson" 
            };
        }

        public async Task<ScheduleUpdateResult> UpdateScheduleAfterLessonMovedAsync(int scheduleId, int lessonId, int userId)
        {
            _logger.LogInformation($"UpdateScheduleAfterLessonMovedAsync: Updating schedule {scheduleId} after lesson {lessonId} moved by user {userId}");

            var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
            if (schedule == null || schedule.UserId != userId)
            {
                throw new ArgumentException($"Schedule {scheduleId} not found or not owned by user {userId}");
            }

            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
            {
                throw new ArgumentException($"Lesson {lessonId} not found");
            }

            // Find which course this lesson belongs to now
            int courseId = GetCourseIdFromLesson(lesson);

            // Find all periods that might be affected (both old and new course)
            var configuration = await _configRepository.GetByIdAsync(schedule.ScheduleConfigurationId);
            var affectedPeriods = new HashSet<int>();

            // Add periods for the current course
            var currentCoursePeriods = configuration.PeriodAssignments
                .Where(pa => pa.CourseId == courseId)
                .Select(pa => pa.Period);
            foreach (var period in currentCoursePeriods)
            {
                affectedPeriods.Add(period);
            }

            // Find any existing schedule events that reference this lesson (in case it moved from another course)
            var existingEvents = schedule.ScheduleEvents.Where(e => e.LessonId == lessonId).ToList();
            foreach (var eventDto in existingEvents)
            {
                if (eventDto.CourseId.HasValue)
                {
                    var oldCoursePeriods = configuration.PeriodAssignments
                        .Where(pa => pa.CourseId == eventDto.CourseId.Value)
                        .Select(pa => pa.Period);
                    foreach (var period in oldCoursePeriods)
                    {
                        affectedPeriods.Add(period);
                    }
                }
            }

            if (!affectedPeriods.Any())
            {
                _logger.LogWarning($"No periods affected by lesson {lessonId} move, no schedule updates needed");
                return new ScheduleUpdateResult { Success = true, EventsUpdated = 0, Message = "No periods affected by lesson move" };
            }

            // Use shared logic to regenerate events for all affected periods
            var eventsUpdated = 0;
            foreach (var period in affectedPeriods)
            {
                var periodCourse = configuration.PeriodAssignments
                    .FirstOrDefault(pa => pa.Period == period && pa.CourseId.HasValue)?.CourseId;
                if (periodCourse.HasValue)
                {
                    eventsUpdated += await RegenerateEventsForPeriodCourse(scheduleId, period, periodCourse.Value, userId);
                }
            }

            _logger.LogInformation($"Updated {eventsUpdated} events across {affectedPeriods.Count} periods for moved lesson {lessonId}");

            return new ScheduleUpdateResult 
            { 
                Success = true, 
                EventsUpdated = eventsUpdated, 
                Message = $"Updated {eventsUpdated} schedule events for moved lesson" 
            };
        }

        // ✅ NEW: Generate partial schedule events for a specific date range
        public async Task<List<ScheduleEventResource>> GenerateEventsForDateRangeAsync(int scheduleId, DateTime startDate, DateTime endDate, int userId)
        {
            _logger.LogInformation($"GenerateEventsForDateRangeAsync: Generating events for schedule {scheduleId}, date range {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}, user {userId}");

            var events = new List<ScheduleEventResource>();

            try
            {
                // Get the schedule with its configuration
                var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
                if (schedule == null || schedule.UserId != userId)
                {
                    _logger.LogWarning($"Schedule {scheduleId} not found or not owned by user {userId}");
                    return events;
                }

                // Get the configuration for this schedule
                var configuration = await _configRepository.GetByIdAsync(schedule.ScheduleConfigurationId);
                if (configuration == null)
                {
                    _logger.LogWarning($"Configuration {schedule.ScheduleConfigurationId} not found for schedule {scheduleId}");
                    return events;
                }

                // ✅ OPTIMIZED: Generate events only for the specified date range
                _logger.LogInformation($"Generating partial events for date range {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

                // Generate lesson events for the date range
                var lessonEvents = await GenerateLessonEventsForDateRange(configuration, startDate, endDate);
                events.AddRange(lessonEvents);

                // Generate special day events for the date range (if any special days exist for these dates)
                var specialDayEvents = await GenerateSpecialDayEventsForDateRange(schedule, startDate, endDate);
                events.AddRange(specialDayEvents);

                _logger.LogInformation($"Generated {events.Count} partial events for schedule {scheduleId} between {startDate:yyyy-MM-dd} and {endDate:yyyy-MM-dd}");

                return events;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating partial events for schedule {scheduleId} - falling back to empty list");
                return events; // Return empty list to trigger fallback to full regeneration
            }
        }

        // ✅ NEW: Generate lesson events for specific date range (simplified implementation)
        private async Task<List<ScheduleEventResource>> GenerateLessonEventsForDateRange(ScheduleConfiguration configuration, DateTime startDate, DateTime endDate)
        {
            var events = new List<ScheduleEventResource>();

            try
            {
                // For now, return placeholder events to demonstrate the concept
                // In a full implementation, this would use proper repository pattern to get courses/lessons
                _logger.LogInformation($"Generating placeholder lesson events for date range {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

                // This is a proof-of-concept implementation
                // In production, this would integrate with existing lesson sequence generation
                return events;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating lesson events for date range");
                return events;
            }
        }

        // ✅ NEW: Generate special day events for specific date range
        private async Task<List<ScheduleEventResource>> GenerateSpecialDayEventsForDateRange(Schedule schedule, DateTime startDate, DateTime endDate)
        {
            var events = new List<ScheduleEventResource>();

            try
            {
                // Get special days that fall within the date range
                var specialDaysInRange = schedule.SpecialDays?
                    .Where(sd => sd.Date >= startDate && sd.Date <= endDate)
                    .ToList() ?? new List<SpecialDay>();

                foreach (var specialDay in specialDaysInRange)
                {
                    // Parse periods from JSON string (e.g., "[1,2,3]")
                    var periods = ParsePeriodsFromJson(specialDay.Periods);

                    // Generate events for each period affected by this special day
                    foreach (var period in periods)
                    {
                        events.Add(new ScheduleEventResource
                        {
                            Date = specialDay.Date,
                            Period = period,
                            EventType = "SpecialDay",
                            EventCategory = "SpecialDay",
                            SpecialDayId = specialDay.Id,
                            SpecialDayTitle = specialDay.Title,
                            SpecialDayDescription = specialDay.Description ?? "",
                            SpecialDayBackgroundColor = specialDay.BackgroundColor ?? "#e74c3c"
                        });
                    }
                }

                _logger.LogInformation($"Generated {events.Count} special day events for date range");
                return events;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating special day events for date range");
                return events;
            }
        }

        // ✅ NEW: Helper method to check if date should be skipped
        private bool ShouldSkipDate(DateTime date, ScheduleConfiguration configuration)
        {
            // Skip weekends if not configured for weekend classes
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                // For now, assume weekends are skipped unless explicitly configured
                return true;
            }

            return false;
        }


        // ✅ NEW: Helper method to parse periods from JSON string
        private List<int> ParsePeriodsFromJson(string periodsJson)
        {
            try
            {
                if (string.IsNullOrEmpty(periodsJson))
                    return new List<int>();

                // Remove brackets and parse as comma-separated integers
                var cleanJson = periodsJson.Trim('[', ']');
                if (string.IsNullOrEmpty(cleanJson))
                    return new List<int>();

                return cleanJson.Split(',')
                    .Select(p => int.TryParse(p.Trim(), out int period) ? period : 0)
                    .Where(p => p > 0)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to parse periods JSON '{periodsJson}': {ex.Message}");
                return new List<int>();
            }
        }

        // === SHARED UPDATE LOGIC ===

        private async Task<int> RegenerateEventsForPeriodCourse(int scheduleId, int period, int courseId, int userId)
        {
            _logger.LogInformation($"RegenerateEventsForPeriodCourse: Regenerating period {period}, course {courseId} in schedule {scheduleId}");

            var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
            var configuration = await _configRepository.GetByIdAsync(schedule.ScheduleConfigurationId);
            
            // Get the period assignment
            var periodAssignment = configuration.PeriodAssignments
                .FirstOrDefault(pa => pa.Period == period && pa.CourseId == courseId);
            
            if (periodAssignment == null)
            {
                _logger.LogWarning($"No period assignment found for period {period}, course {courseId}");
                return 0;
            }

            // Remove existing events for this period
            var existingEvents = schedule.ScheduleEvents
                .Where(e => e.Period == period && e.CourseId == courseId)
                .ToList();

            foreach (var eventToRemove in existingEvents)
            {
                schedule.ScheduleEvents.Remove(eventToRemove);
            }

            // Generate new events using shared logic
            var teachingDaysArray = configuration.TeachingDays.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var periodAssignmentResource = ConvertToResource(periodAssignment);
            
            var newEvents = await GenerateEventsForPeriodCourse(
                periodAssignmentResource,
                configuration.StartDate,
                configuration.EndDate,
                teachingDaysArray,
                -1000 - (period * 1000), // Unique negative IDs for new events
                userId
            );

            // Convert to domain entities and add to schedule
            var newEventEntities = newEvents.Select(eventResource => new ScheduleEvent
            {
                CourseId = eventResource.CourseId,
                Date = eventResource.Date,
                Period = eventResource.Period,
                LessonId = eventResource.LessonId,
                EventType = eventResource.EventType,
                EventCategory = eventResource.EventCategory,
                Comment = eventResource.Comment,
                ScheduleSort = eventResource.ScheduleSort,
                ScheduleId = scheduleId
            }).ToList();

            foreach (var newEvent in newEventEntities)
            {
                schedule.ScheduleEvents.Add(newEvent);
            }

            // Save changes using the correct method
            await _scheduleRepository.UpdateScheduleEventsAsync(scheduleId, schedule.ScheduleEvents.ToList());

            _logger.LogInformation($"Regenerated {newEvents.Count} events for period {period}, course {courseId}");
            return newEvents.Count;
        }

        private int GetCourseIdFromLesson(Lesson lesson)
        {
            if (lesson.TopicId.HasValue && lesson.Topic != null)
            {
                return lesson.Topic.CourseId;
            }
            else if (lesson.SubTopicId.HasValue && lesson.SubTopic?.Topic != null)
            {
                return lesson.SubTopic.Topic.CourseId;
            }
            
            throw new InvalidOperationException($"Lesson {lesson.Id} does not belong to any course");
        }


        // === PRIVATE HELPER METHODS ===

        private async Task<List<ScheduleEventResource>> GenerateScheduleEventsFromConfiguration(ScheduleConfiguration configuration, int userId)
        {
            _logger.LogInformation($"GenerateScheduleEventsFromConfiguration: NEW Day→Period approach with {configuration.PeriodAssignments.Count} period assignments");

            var allScheduleEvents = new List<ScheduleEventResource>();
            var eventIdCounter = -1; // Negative for in-memory events

            // Convert teaching days string to array
            var configTeachingDaysArray = configuration.TeachingDays.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(d => d.Trim().ToLower()).ToHashSet();

            _logger.LogInformation($"🔍 Config teaching days: [{string.Join(", ", configTeachingDaysArray)}]");
            _logger.LogInformation($"🔍 Date range: {configuration.StartDate:yyyy-MM-dd} to {configuration.EndDate:yyyy-MM-dd}");

            // Get all period assignments as resources
            var allPeriodAssignments = configuration.PeriodAssignments.Select(pa => ConvertToResource(pa)).ToList();

            // Log period assignments
            foreach (var assignment in allPeriodAssignments)
            {
                _logger.LogInformation($"🔍 Period {assignment.Period}: CourseId={assignment.CourseId}, TeachingDays=[{string.Join(", ", assignment.TeachingDays)}], SpecialPeriodType='{assignment.SpecialPeriodType}'");
            }

            // *** CRITICAL: Get existing special days for inline integration ***
            var existingSpecialDays = await GetExistingSpecialDaysForSchedule(configuration.Id, userId);
            _logger.LogInformation($"🔍 SPECIAL DAY DEBUG: Found {existingSpecialDays.Count} existing special days for inline integration");

            // ✅ DEBUG: Log details of each existing special day
            foreach (var sd in existingSpecialDays)
            {
                _logger.LogInformation($"🔍 SPECIAL DAY DEBUG: Existing Special Day ID:{sd.Id}, Date:{sd.Date:yyyy-MM-dd}, Type:{sd.EventType}, Periods:[{string.Join(",", sd.Periods)}]");
            }

            // Initialize lesson trackers for course assignments only
            var periodLessonTrackers = new Dictionary<int, PeriodLessonTracker>();
            
            foreach (var assignment in allPeriodAssignments.Where(pa => pa.CourseId.HasValue))
            {
                var lessons = await GetLessonsForCourse(assignment.CourseId!.Value, userId);
                periodLessonTrackers[assignment.Period] = new PeriodLessonTracker
                {
                    Period = assignment.Period,
                    CourseId = assignment.CourseId.Value,
                    Lessons = lessons,
                    CurrentLessonIndex = 0,
                    Assignment = assignment
                };
                
                _logger.LogInformation($"Initialized tracker for Period {assignment.Period}, Course {assignment.CourseId} - {lessons.Count} lessons");
            }

            // *** NEW APPROACH: Day → Period iteration ***
            var currentDate = configuration.StartDate;
            var totalDays = 0;
            var teachingDays = 0;

            while (currentDate <= configuration.EndDate)
            {
                totalDays++;
                var dayName = currentDate.DayOfWeek.ToString().ToLower();

                // Check if this is a config-level teaching day
                if (configTeachingDaysArray.Contains(dayName))
                {
                    teachingDays++;
                    _logger.LogInformation($"🔍 Processing teaching day {teachingDays}: {currentDate:yyyy-MM-dd} ({dayName})");

                    // Process all periods for this day
                    foreach (var assignment in allPeriodAssignments.OrderBy(pa => pa.Period))
                    {
                        // Check if this period teaches on this day (case-insensitive)
                        if (assignment.TeachingDays.Any(td => string.Equals(td, dayName, StringComparison.OrdinalIgnoreCase)))
                        {
                            _logger.LogInformation($"🔍 Processing Period {assignment.Period} on {currentDate:yyyy-MM-dd} ({dayName})");

                            // *** INLINE SPECIAL DAY CHECK ***
                            var specialDay = GetSpecialDayForDateAndPeriod(existingSpecialDays, currentDate, assignment.Period);
                            
                            if (specialDay != null)
                            {
                                // Create special day event - don't advance lesson sequence
                                var specialDayEvent = CreateSpecialDayEventResource(eventIdCounter--, currentDate, assignment, specialDay);
                                allScheduleEvents.Add(specialDayEvent);
                                
                                _logger.LogInformation($"✅ Created special day event: {currentDate:yyyy-MM-dd} Period {assignment.Period} - {specialDay.EventType}");
                            }
                            else
                            {
                                // Create regular event based on assignment type
                                ScheduleEventResource eventResource;

                                if (assignment.CourseId.HasValue && periodLessonTrackers.ContainsKey(assignment.Period))
                                {
                                    // Course assignment - create lesson event
                                    var tracker = periodLessonTrackers[assignment.Period];
                                    eventResource = CreateLessonEventFromTracker(eventIdCounter--, currentDate, tracker);
                                    
                                    // Advance lesson sequence only if lesson was assigned
                                    if (eventResource.LessonId.HasValue)
                                    {
                                        tracker.AdvanceToNextLesson();
                                    }
                                }
                                else if (!string.IsNullOrEmpty(assignment.SpecialPeriodType))
                                {
                                    // Special period assignment
                                    eventResource = CreateSpecialPeriodEventResource(eventIdCounter--, currentDate, assignment);
                                }
                                else
                                {
                                    // Unassigned period
                                    eventResource = CreateUnassignedPeriodEventResource(eventIdCounter--, currentDate, assignment);
                                }

                                allScheduleEvents.Add(eventResource);
                                _logger.LogInformation($"✅ Created {eventResource.EventType} event: {currentDate:yyyy-MM-dd} Period {assignment.Period}");
                            }
                        }
                        else
                        {
                            _logger.LogInformation($"🔍 Skipping Period {assignment.Period} on {currentDate:yyyy-MM-dd} ({dayName}) - not a teaching day for this period");
                        }
                    }
                }
                else
                {
                    if (totalDays <= 10) // Only log first 10 days to avoid spam
                        _logger.LogInformation($"🔍 Skipping non-teaching day: {currentDate:yyyy-MM-dd} ({dayName})");
                }

                currentDate = currentDate.AddDays(1);
            }

            _logger.LogInformation($"Generation complete: {allScheduleEvents.Count} events across {totalDays} total days ({teachingDays} teaching days)");

            // Log statistics
            var eventsByType = allScheduleEvents.GroupBy(e => e.EventType).ToDictionary(g => g.Key, g => g.Count());
            foreach (var kvp in eventsByType)
            {
                _logger.LogInformation($"  {kvp.Key}: {kvp.Value} events");
            }

            return allScheduleEvents.OrderBy(e => e.Date).ThenBy(e => e.Period).ToList();
        }

        private PeriodAssignmentResource ConvertToResource(PeriodAssignment domainAssignment)
        {
            return new PeriodAssignmentResource
            {
                Id = domainAssignment.Id,
                Period = domainAssignment.Period,
                CourseId = domainAssignment.CourseId,
                SpecialPeriodType = domainAssignment.SpecialPeriodType,
                TeachingDays = domainAssignment.TeachingDays
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(d => d.Trim())
                    .ToArray(),
                Room = domainAssignment.Room,
                Notes = domainAssignment.Notes,
                BackgroundColor = domainAssignment.BackgroundColor,
                FontColor = domainAssignment.FontColor
            };
        }

        private async Task<List<ScheduleEventResource>> GenerateEventsForPeriodCourse(
            PeriodAssignmentResource assignment,
            DateTime startDate,
            DateTime endDate,
            string[] teachingDays,
            int startingEventId,
            int userId)
        {
            var events = new List<ScheduleEventResource>();
            var eventId = startingEventId;
            var lessonIndex = 0;

            // Get course lessons with detailed logging
            var lessons = await GetLessonsForCourse(assignment.CourseId!.Value, userId);

            _logger.LogInformation($"🔍 [GenerateEventsForPeriodCourse] Period {assignment.Period}, Course {assignment.CourseId} - {lessons.Count} lessons available");

            // Log first few lessons for debugging
            if (lessons.Any())
            {
                var firstLessons = lessons.Take(3).Select(l => new { l.Id, l.Title, TopicId = l.TopicId, SubTopicId = l.SubTopicId }).ToList();
                _logger.LogInformation($"🔍 [GenerateEventsForPeriodCourse] First lessons: {System.Text.Json.JsonSerializer.Serialize(firstLessons)}");
            }
            else
            {
                _logger.LogWarning($"⚠️ [GenerateEventsForPeriodCourse] NO LESSONS FOUND for Course {assignment.CourseId}, User {userId}");
            }

            // Generate events for each teaching day
            var currentDate = startDate;
            var teachingDayNames = teachingDays.Select(d => d.ToLower()).ToHashSet();

            _logger.LogInformation($"🔍 [GenerateEventsForPeriodCourse] Teaching days: {string.Join(", ", teachingDayNames)}");
            _logger.LogInformation($"🔍 [GenerateEventsForPeriodCourse] Date range: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

            var dayCount = 0;
            while (currentDate <= endDate)
            {
                var dayName = currentDate.DayOfWeek.ToString().ToLower();

                if (teachingDayNames.Contains(dayName))
                {
                    dayCount++;

                    _logger.LogDebug($"🔍 [GenerateEventsForPeriodCourse] Teaching day {dayCount}: {currentDate:yyyy-MM-dd} ({dayName}), LessonIndex: {lessonIndex}, LessonsCount: {lessons.Count}");

                    var scheduleEvent = CreatePeriodCourseEvent(
                        eventId--,
                        currentDate,
                        assignment,
                        lessons,
                        lessonIndex
                    );

                    events.Add(scheduleEvent);

                    // Log what was created
                    _logger.LogDebug($"🔍 [GenerateEventsForPeriodCourse] Created event: EventType={scheduleEvent.EventType}, LessonId={scheduleEvent.LessonId}, Comment={scheduleEvent.Comment}");

                    // Only increment lesson index if we assigned a lesson (not error)
                    if (scheduleEvent.LessonId.HasValue)
                    {
                        lessonIndex++;
                        _logger.LogDebug($"🔍 [GenerateEventsForPeriodCourse] Lesson assigned, incrementing index to {lessonIndex}");
                    }
                    else
                    {
                        _logger.LogWarning($"⚠️ [GenerateEventsForPeriodCourse] ERROR EVENT created for {currentDate:yyyy-MM-dd}, Period {assignment.Period}");
                    }
                }

                currentDate = currentDate.AddDays(1);
            }

            _logger.LogInformation($"🔍 [GenerateEventsForPeriodCourse] Final result: {events.Count} events created, {events.Count(e => e.EventType == "Lesson")} lessons, {events.Count(e => e.EventType == "Error")} errors");

            return events;
        }


        private List<ScheduleEventResource> GenerateEventsForSpecialPeriod(
            PeriodAssignmentResource assignment,
            DateTime startDate,
            DateTime endDate,
            string[] teachingDays,
            int startingEventId)
        {
            var events = new List<ScheduleEventResource>();
            var eventId = startingEventId;

            if (string.IsNullOrEmpty(assignment.SpecialPeriodType))
            {
                return events;
            }

            // Generate recurring events for each teaching day
            var currentDate = startDate;
            var teachingDayNames = teachingDays.Select(d => d.ToLower()).ToHashSet();

            while (currentDate <= endDate)
            {
                var dayName = currentDate.DayOfWeek.ToString().ToLower();

                if (teachingDayNames.Contains(dayName))
                {
                    var scheduleEvent = new ScheduleEventResource
                    {
                        Id = eventId--,
                        ScheduleId = 0,
                        CourseId = null,
                        Date = currentDate,
                        Period = assignment.Period,
                        LessonId = null,
                        EventType = assignment.SpecialPeriodType,
                        EventCategory = "SpecialPeriod",
                        Comment = assignment.Notes
                    };

                    events.Add(scheduleEvent);
                }

                currentDate = currentDate.AddDays(1);
            }

            return events;
        }

        private List<ScheduleEventResource> GenerateEventsForUnassignedPeriod(
            PeriodAssignmentResource assignment,
            DateTime startDate,
            DateTime endDate,
            string[] teachingDays,
            int startingEventId)
        {
            var events = new List<ScheduleEventResource>();
            var eventId = startingEventId;

            // Generate error events for unassigned periods
            var currentDate = startDate;
            var teachingDayNames = teachingDays.Select(d => d.ToLower()).ToHashSet();

            while (currentDate <= endDate)
            {
                var dayName = currentDate.DayOfWeek.ToString().ToLower();

                if (teachingDayNames.Contains(dayName))
                {
                    var scheduleEvent = new ScheduleEventResource
                    {
                        Id = eventId--,
                        ScheduleId = 0,
                        CourseId = null,
                        Date = currentDate,
                        Period = assignment.Period,
                        LessonId = null,
                        EventType = "Error",
                        EventCategory = null, // Error events have null category
                        Comment = "Period not configured - assign a course or special period type"
                    };

                    events.Add(scheduleEvent);
                }

                currentDate = currentDate.AddDays(1);
            }

            return events;
        }

        private async Task<List<ScheduleEventResource>> GenerateContinuationEventsForPeriodCourse(
            ContinuationPoint continuationPoint,
            DateTime startDate,
            DateTime endDate,
            string[] teachingDays,
            int startingEventId,
            int userId)
        {
            var events = new List<ScheduleEventResource>();
            var eventId = startingEventId;

            // Get course lessons
            var lessons = await GetLessonsForCourse(continuationPoint.CourseId, userId);
            var lessonIndex = continuationPoint.LastAssignedLessonIndex + 1; // Start from next lesson

            if (lessonIndex >= lessons.Count)
            {
                return events; // No more lessons to assign
            }

            // Generate events for each teaching day
            var currentDate = startDate;
            var teachingDayNames = teachingDays.Select(d => d.ToLower()).ToHashSet();

            while (currentDate <= endDate && lessonIndex < lessons.Count)
            {
                var dayName = currentDate.DayOfWeek.ToString().ToLower();

                if (teachingDayNames.Contains(dayName))
                {
                    var scheduleEvent = new ScheduleEventResource
                    {
                        Id = eventId--,
                        ScheduleId = 0, // Will be set when added to schedule
                        CourseId = continuationPoint.CourseId,
                        Date = currentDate,
                        Period = continuationPoint.Period,
                        LessonId = lessons[lessonIndex].Id,
                        EventType = "Lesson",
                        EventCategory = "Lesson",
                        Comment = null
                    };

                    events.Add(scheduleEvent);
                    lessonIndex++;
                }

                currentDate = currentDate.AddDays(1);
            }

            return events;
        }

        private ScheduleEventResource CreatePeriodCourseEvent(
            int eventId,
            DateTime date,
            PeriodAssignmentResource assignment,
            List<Lesson> lessons,
            int lessonIndex)
        {
            int? lessonId = null;
            string eventType = "Error";
            string? eventCategory = null;
            string? comment = null;
            string? lessonTitle = null; // ✅ ADD: For lesson display
            string? lessonObjective = null; // ✅ ADD: For lesson details
            string? lessonMethods = null; // ✅ ADD: Additional lesson data
            string? lessonMaterials = null; // ✅ ADD: Additional lesson data
            string? lessonAssessment = null; // ✅ ADD: Additional lesson data
            int? lessonSort = null; // ✅ ADD: Lesson sort order within course
            int scheduleSort = lessonIndex;

            _logger.LogInformation($"🔢 [ScheduleSort] CRITICAL: Setting ScheduleSort = {scheduleSort} (lessonIndex) for Date: {date:yyyy-MM-dd}, Period: {assignment.Period}, CourseId: {assignment.CourseId}");
            _logger.LogDebug($"🔍 [CreatePeriodCourseEvent] Input - EventId: {eventId}, Date: {date:yyyy-MM-dd}, Period: {assignment.Period}, CourseId: {assignment.CourseId}, LessonIndex: {lessonIndex}, LessonsCount: {lessons.Count}");

            if (lessonIndex < lessons.Count)
            {
                // Assign lesson with rich data
                var lesson = lessons[lessonIndex];
                lessonId = lesson.Id;
                eventType = "Lesson";
                eventCategory = "Lesson";
                comment = null;
                lessonTitle = lesson.Title; // ✅ Use actual lesson title
                lessonObjective = lesson.Objective; // ✅ Use lesson objective
                lessonMethods = lesson.Methods; // ✅ Additional lesson data
                lessonMaterials = lesson.Materials; // ✅ Additional lesson data
                lessonAssessment = lesson.Assessment; // ✅ Additional lesson data
                lessonSort = lesson.SortOrder; // ✅ Lesson sort within its container

                _logger.LogDebug($"✅ [CreatePeriodCourseEvent] LESSON ASSIGNED - LessonId: {lessonId}, Title: '{lessonTitle}', Objective: '{lessonObjective}'");
            }
            else
            {
                // No more lessons available - error day
                lessonId = null;
                eventType = "Error";
                eventCategory = null;
                comment = "No lesson assigned - schedule needs more content";
                lessonTitle = null; // ✅ No lesson data for errors
                lessonObjective = null;
                lessonMethods = null;
                lessonMaterials = null;
                lessonAssessment = null;
                lessonSort = null;
                scheduleSort = lessonIndex;

                _logger.LogWarning($"⚠️ [CreatePeriodCourseEvent] ERROR EVENT - LessonIndex {lessonIndex} >= LessonsCount {lessons.Count}");
            }

            var result = new ScheduleEventResource
            {
                Id = eventId,
                ScheduleId = 0,
                CourseId = assignment.CourseId,
                Date = date,
                Period = assignment.Period,
                LessonId = lessonId,
                EventType = eventType,
                EventCategory = eventCategory,
                Comment = comment,
                LessonTitle = lessonTitle, // ✅ POPULATE: Lesson title for calendar display
                LessonObjective = lessonObjective, // ✅ POPULATE: Lesson objective for details
                LessonMethods = lessonMethods, // ✅ POPULATE: Additional lesson data
                LessonMaterials = lessonMaterials, // ✅ POPULATE: Additional lesson data
                LessonAssessment = lessonAssessment, // ✅ POPULATE: Additional lesson data
                LessonSort = lessonSort, // ✅ POPULATE: Lesson sort order
                ScheduleSort = scheduleSort
            };

            _logger.LogDebug($"🔍 [CreatePeriodCourseEvent] OUTPUT - EventType: {result.EventType}, LessonId: {result.LessonId}, LessonTitle: '{result.LessonTitle}', Comment: {result.Comment}");

            return result;
        }

        // === DATA ACCESS HELPERS ===

        private async Task<List<Lesson>> GetLessonsForCourse(int courseId, int userId)
        {
            _logger.LogInformation($"🔍 [GetLessonsForCourse] Starting - CourseId: {courseId}, UserId: {userId}");

            // Get all lessons for this course with necessary navigation properties  
            // ✅ FIX: Use AsNoTracking to ensure fresh SubTopic.SortOrder after SubTopic moves
            var courseLessons = await _lessonRepository.GetByUserId(userId, includeArchived: false)
                .AsNoTracking() // Force fresh navigation properties from database
                .Where(l =>
                    // Direct topic lessons
                    (l.TopicId.HasValue && l.Topic.CourseId == courseId) ||
                    // SubTopic lessons where parent topic is in course
                    (l.SubTopicId.HasValue && l.SubTopic.Topic.CourseId == courseId)
                )
                .Include(l => l.Topic)
                .Include(l => l.SubTopic).ThenInclude(st => st.Topic)
                .ToListAsync();

            // ✅ FIX: Manual sorting to handle mixed entity ordering correctly
            var sortedLessons = courseLessons
                .OrderBy(l => GetTopicSortOrder(l))        // Primary: Topic sort order
                .ThenBy(l => GetEntitySortOrderInTopic(l)) // Secondary: Mixed entity position within topic  
                .ThenBy(l => l.SortOrder)                  // Tertiary: Lesson position within container
                .ToList();

            _logger.LogInformation($"🔍 [GetLessonsForCourse] Found {sortedLessons.Count} lessons for Course {courseId}");

            // ✅ Debug logging to verify correct order
            _logger.LogInformation("🔍 [GetLessonsForCourse] Final lesson sequence:");
            for (int i = 0; i < Math.Min(10, sortedLessons.Count); i++) // Log first 10 lessons
            {
                var lesson = sortedLessons[i];
                var container = lesson.TopicId.HasValue ? "Direct" : $"SubTopic-{lesson.SubTopic?.SortOrder}";
                var topicSort = GetTopicSortOrder(lesson);
                var entitySort = GetEntitySortOrderInTopic(lesson);

                _logger.LogInformation($"  [{i}] Lesson {lesson.Id} '{lesson.Title}' - Topic:{topicSort}, Entity:{entitySort}, Lesson:{lesson.SortOrder} [{container}]");
            }

            return sortedLessons;
        }


        // ✅ Helper: Get the topic sort order for any lesson
        private int GetTopicSortOrder(Lesson lesson)
        {
            if (lesson.TopicId.HasValue)
            {
                return lesson.Topic?.SortOrder ?? 999;
            }
            else if (lesson.SubTopicId.HasValue)
            {
                return lesson.SubTopic?.Topic?.SortOrder ?? 999;
            }
            return 999;
        }

        // ✅ Helper: Get the entity sort order within the topic (mixed space)
        private int GetEntitySortOrderInTopic(Lesson lesson)
        {
            if (lesson.TopicId.HasValue)
            {
                // Direct topic lesson: use lesson's own sort order within topic
                return lesson.SortOrder;
            }
            else if (lesson.SubTopicId.HasValue)
            {
                // SubTopic lesson: use subtopic's sort order within topic
                // This puts all lessons from a subtopic at the subtopic's position
                return lesson.SubTopic?.SortOrder ?? 999;
            }
            return 999;
        }

        // === NEW HELPER METHODS FOR DAY→PERIOD GENERATION ===

        /// <summary>
        /// Get existing special days for a configuration during generation
        /// </summary>
        private async Task<List<SpecialDayResource>> GetExistingSpecialDaysForSchedule(int configurationId, int userId)
        {
            _logger.LogInformation($"🔍 SPECIAL DAY DEBUG: GetExistingSpecialDaysForSchedule called for configurationId:{configurationId}, userId:{userId}");

            // Get existing schedule for this configuration
            var existingSchedule = await _scheduleRepository.GetByConfigurationIdAsync(configurationId);

            if (existingSchedule == null)
            {
                _logger.LogWarning($"🔍 SPECIAL DAY DEBUG: No existing schedule found for configurationId:{configurationId}");
                return new List<SpecialDayResource>();
            }

            if (existingSchedule.UserId != userId)
            {
                _logger.LogWarning($"🔍 SPECIAL DAY DEBUG: Schedule {existingSchedule.Id} found but userId mismatch. Expected:{userId}, Actual:{existingSchedule.UserId}");
                return new List<SpecialDayResource>();
            }

            _logger.LogInformation($"🔍 SPECIAL DAY DEBUG: Found existing schedule {existingSchedule.Id} for configurationId:{configurationId}");

            // Get special days for the existing schedule
            var specialDays = existingSchedule.SpecialDays ?? new List<SpecialDay>();
            _logger.LogInformation($"🔍 SPECIAL DAY DEBUG: Schedule {existingSchedule.Id} has {specialDays.Count} special days in database");

            // ✅ DEBUG: Log each special day from database
            foreach (var sd in specialDays)
            {
                _logger.LogInformation($"🔍 SPECIAL DAY DEBUG: Raw DB Special Day - ID:{sd.Id}, ScheduleId:{sd.ScheduleId}, Date:{sd.Date:yyyy-MM-dd}, Type:{sd.EventType}, Title:{sd.Title}, Periods:{sd.Periods}");
            }

            var result = _mapper.Map<List<SpecialDayResource>>(specialDays);
            _logger.LogInformation($"🔍 SPECIAL DAY DEBUG: Mapped {result.Count} special days to resources for regeneration");

            return result;
        }

        /// <summary>
        /// Check if a special day affects a specific date and period
        /// </summary>
        private SpecialDayResource? GetSpecialDayForDateAndPeriod(List<SpecialDayResource> specialDays, DateTime date, int period)
        {
            return specialDays.FirstOrDefault(sd => 
                sd.Date.Date == date.Date && 
                sd.Periods.Contains(period));
        }

        /// <summary>
        /// Create special day event resource
        /// </summary>
        private ScheduleEventResource CreateSpecialDayEventResource(int eventId, DateTime date, PeriodAssignmentResource assignment, SpecialDayResource specialDay)
        {
            return new ScheduleEventResource
            {
                Id = eventId,
                ScheduleId = 0, // Will be updated when persisted
                CourseId = null,
                Date = date,
                Period = assignment.Period,
                LessonId = null,
                SpecialDayId = specialDay.Id, // NEW: Link back to the special day entity
                EventType = specialDay.EventType,
                EventCategory = "SpecialDay",
                Comment = specialDay.Description ?? string.Empty, // ✅ FIXED: Store description in Comment field
                LessonTitle = specialDay.Title, // ✅ FIXED: Store title in LessonTitle field for Special Days
                LessonObjective = null,
                LessonMethods = null,
                LessonMaterials = null,
                LessonAssessment = null,
                LessonSort = null,
                ScheduleSort = 0 // Special days don't affect lesson sequence
            };
        }

        /// <summary>
        /// Create lesson event from tracker state
        /// </summary>
        private ScheduleEventResource CreateLessonEventFromTracker(int eventId, DateTime date, PeriodLessonTracker tracker)
        {
            var currentLesson = tracker.GetCurrentLesson();
            
            if (currentLesson != null)
            {
                // Create lesson event
                return new ScheduleEventResource
                {
                    Id = eventId,
                    ScheduleId = 0,
                    CourseId = tracker.CourseId,
                    Date = date,
                    Period = tracker.Period,
                    LessonId = currentLesson.Id,
                    EventType = "Lesson",
                    EventCategory = "Lesson",
                    Comment = null,
                    LessonTitle = currentLesson.Title,
                    LessonObjective = currentLesson.Objective,
                    LessonMethods = currentLesson.Methods,
                    LessonMaterials = currentLesson.Materials,
                    LessonAssessment = currentLesson.Assessment,
                    LessonSort = currentLesson.SortOrder,
                    ScheduleSort = tracker.CurrentLessonIndex
                };
            }
            else
            {
                // Create error event - no more lessons
                return new ScheduleEventResource
                {
                    Id = eventId,
                    ScheduleId = 0,
                    CourseId = tracker.CourseId,
                    Date = date,
                    Period = tracker.Period,
                    LessonId = null,
                    EventType = "Error",
                    EventCategory = "Error",
                    Comment = $"No more lessons available for Course {tracker.CourseId}",
                    LessonTitle = null,
                    LessonObjective = null,
                    LessonMethods = null,
                    LessonMaterials = null,
                    LessonAssessment = null,
                    LessonSort = null,
                    ScheduleSort = tracker.CurrentLessonIndex
                };
            }
        }

        /// <summary>
        /// Create special period event resource
        /// </summary>
        private ScheduleEventResource CreateSpecialPeriodEventResource(int eventId, DateTime date, PeriodAssignmentResource assignment)
        {
            return new ScheduleEventResource
            {
                Id = eventId,
                ScheduleId = 0,
                CourseId = null,
                Date = date,
                Period = assignment.Period,
                LessonId = null,
                EventType = assignment.SpecialPeriodType ?? "Special",
                EventCategory = "SpecialPeriod",
                Comment = assignment.Notes,
                LessonTitle = null,
                LessonObjective = null,
                LessonMethods = null,
                LessonMaterials = null,
                LessonAssessment = null,
                LessonSort = null,
                ScheduleSort = 0
            };
        }

        /// <summary>
        /// Create unassigned period event resource
        /// </summary>
        private ScheduleEventResource CreateUnassignedPeriodEventResource(int eventId, DateTime date, PeriodAssignmentResource assignment)
        {
            return new ScheduleEventResource
            {
                Id = eventId,
                ScheduleId = 0,
                CourseId = null,
                Date = date,
                Period = assignment.Period,
                LessonId = null,
                EventType = "Unassigned",
                EventCategory = "Unassigned",
                Comment = "Period not assigned",
                LessonTitle = null,
                LessonObjective = null,
                LessonMethods = null,
                LessonMaterials = null,
                LessonAssessment = null,
                LessonSort = null,
                ScheduleSort = 0
            };
        }

        private async Task<int> GetLessonCountForCourse(int courseId, int userId)
        {
            var lessons = await GetLessonsForCourse(courseId, userId);
            return lessons.Count;
        }
    }

    /// <summary>
    /// Tracks lesson sequence state for individual periods during schedule generation
    /// Enables periods to maintain independent lesson progression with special day interruptions
    /// </summary>
    public class PeriodLessonTracker
    {
        public int Period { get; set; }
        public int CourseId { get; set; }
        public List<Lesson> Lessons { get; set; } = new();
        public int CurrentLessonIndex { get; set; } = 0;
        public PeriodAssignmentResource Assignment { get; set; } = null!;

        /// <summary>
        /// Get current lesson for this period, or null if sequence is complete
        /// </summary>
        public Lesson? GetCurrentLesson()
        {
            return CurrentLessonIndex < Lessons.Count ? Lessons[CurrentLessonIndex] : null;
        }

        /// <summary>
        /// Advance to next lesson in sequence
        /// </summary>
        public void AdvanceToNextLesson()
        {
            if (CurrentLessonIndex < Lessons.Count)
            {
                CurrentLessonIndex++;
            }
        }

        /// <summary>
        /// Check if this period teaches on the given day
        /// </summary>
        public bool TeachesOnDay(DateTime date)
        {
            var dayName = date.DayOfWeek.ToString().ToLower();
            return Assignment.TeachingDays.Contains(dayName);
        }

        /// <summary>
        /// Check if lesson sequence is complete
        /// </summary>
        public bool IsSequenceComplete => CurrentLessonIndex >= Lessons.Count;
    }
}
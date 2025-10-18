using LessonTree.Models.DTO;
using LessonTree.BLL.Services; // For the new result types

namespace LessonTree.BLL.Services
{
    /// <summary>
    /// Service interface for schedule operations with auto-generation capabilities
    /// Handles CRUD operations for user schedules and special days + schedule generation
    /// </summary>
    public interface IScheduleService
    {
        // === CORE SCHEDULE OPERATIONS ===

        /// <summary>
        /// Get schedule for user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Schedule resource or null if not found</returns>
        Task<ScheduleResource?> GetUserScheduleAsync(int userId);

        /// <summary>
        /// Get schedule by ID with user ownership validation
        /// </summary>
        /// <param name="id">Schedule ID</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Schedule resource or null if not found</returns>
        Task<ScheduleResource?> GetByIdAsync(int id, int userId);

        /// <summary>
        /// Get schedule by configuration ID with user ownership validation
        /// </summary>
        /// <param name="configurationId">Schedule configuration ID</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Schedule resource or null if not found</returns>
        Task<ScheduleResource?> GetByConfigurationIdAsync(int configurationId, int userId);

        /// <summary>
        /// Create new schedule
        /// </summary>
        /// <param name="createResource">Schedule creation data</param>
        /// <param name="userId">User ID for ownership assignment</param>
        /// <returns>Created schedule resource</returns>
        Task<ScheduleResource> CreateScheduleAsync(ScheduleCreateResource createResource, int userId);

        /// <summary>
        /// Update schedule events
        /// </summary>
        /// <param name="scheduleId">Schedule ID</param>
        /// <param name="events">Updated schedule events</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Updated schedule resource</returns>
        Task<ScheduleResource> UpdateScheduleEventsAsync(int scheduleId, List<ScheduleEventResource> events, int userId);

        /// <summary>
        /// Delete user's schedule
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Task completion</returns>
        Task DeleteUserScheduleAsync(int userId);

        // === AUTO-GENERATION METHODS (NEW) ===

        /// <summary>
        /// Find all schedules that contain a specific course
        /// Used by lesson addition workflow to trigger regeneration
        /// </summary>
        Task<List<ScheduleResource>> FindAllSchedulesByCourseIdAsync(int courseId, int userId);

        /// <summary>
        /// Create schedule automatically from configuration (Phase 1 auto-generation)
        /// Replaces frontend generation workflow
        /// </summary>
        /// <param name="configurationId">Schedule configuration ID</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Generated schedule with events</returns>
        Task<ScheduleResource> CreateScheduleFromConfigurationAsync(int configurationId, int userId);

        /// <summary>
        /// Regenerate schedule when configuration is updated
        /// Handles configuration changes that affect schedule structure
        /// </summary>
        /// <param name="configurationId">Updated configuration ID</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Regenerated schedule</returns>
        Task<ScheduleResource> RegenerateScheduleFromConfigurationAsync(int configurationId, int userId);

        /// <summary>
        /// Continue lesson sequences in existing schedule
        /// Extends schedule with additional lesson sequence events
        /// </summary>
        /// <param name="scheduleId">Existing schedule ID</param>
        /// <param name="continuationRequest">Continuation parameters</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Updated schedule with continuation events</returns>
        Task<ScheduleResource> ContinueSequencesAsync(int scheduleId, SequenceContinuationRequest continuationRequest, int userId);

        /// <summary>
        /// Validate configuration for schedule generation
        /// Provides detailed validation feedback without generating
        /// </summary>
        /// <param name="configurationId">Configuration to validate</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Validation result with detailed feedback</returns>
        Task<ScheduleValidationResult> ValidateConfigurationAsync(int configurationId, int userId);

        /// <summary>
        /// Get schedule generation preview/summary
        /// Shows what would be generated without creating schedule
        /// </summary>
        /// <param name="configurationId">Configuration to preview</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Generation preview information</returns>
        Task<ScheduleGenerationPreview> GetGenerationPreviewAsync(int configurationId, int userId);

        /// <summary>
        /// Analyze sequence state for existing schedule
        /// </summary>
        /// <param name="scheduleId">Schedule ID</param>
        /// <param name="afterDate">Date to analyze from</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Sequence analysis result</returns>
        Task<SequenceAnalysisResult> AnalyzeSequenceStateAsync(int scheduleId, DateTime afterDate, int userId);

        // === SPECIAL DAY OPERATIONS ===

        /// <summary>
        /// Get special days for schedule
        /// </summary>
        /// <param name="scheduleId">Schedule ID</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>List of special day resources</returns>
        Task<List<SpecialDayResource>> GetSpecialDaysAsync(int scheduleId, int userId);

        /// <summary>
        /// Get specific special day
        /// </summary>
        /// <param name="scheduleId">Schedule ID</param>
        /// <param name="specialDayId">Special day ID</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Special day resource or null if not found</returns>
        Task<SpecialDayResource?> GetSpecialDayAsync(int scheduleId, int specialDayId, int userId);

        /// <summary>
        /// Create special day
        /// </summary>
        /// <param name="scheduleId">Schedule ID</param>
        /// <param name="createResource">Special day creation data</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Created special day resource</returns>
        Task<SpecialDayResource> CreateSpecialDayAsync(int scheduleId, SpecialDayCreateResource createResource, int userId);

        /// <summary>
        /// Update special day
        /// </summary>
        /// <param name="scheduleId">Schedule ID</param>
        /// <param name="specialDayId">Special day ID</param>
        /// <param name="updateResource">Special day update data</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Updated special day response with refresh indication</returns>
        Task<SpecialDayUpdateResponse> UpdateSpecialDayAsync(int scheduleId, int specialDayId, SpecialDayUpdateResource updateResource, int userId);

        /// <summary>
        /// Delete special day
        /// </summary>
        /// <param name="scheduleId">Schedule ID</param>
        /// <param name="specialDayId">Special day ID</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Task completion</returns>
        Task DeleteSpecialDayAsync(int scheduleId, int specialDayId, int userId);

        // ✅ NEW: Optimized Special Day operations with partial schedule regeneration

        /// <summary>
        /// Create special day with optimized partial schedule regeneration
        /// Only regenerates events for the special day's date instead of full schedule
        /// </summary>
        /// <param name="scheduleId">Schedule ID</param>
        /// <param name="createResource">Special day creation data</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Created special day resource with optimization metadata</returns>
        Task<SpecialDayOptimizedResponse> CreateSpecialDayOptimizedAsync(int scheduleId, SpecialDayCreateResource createResource, int userId);

        /// <summary>
        /// Delete special day with optimized partial schedule regeneration
        /// Only regenerates events for the special day's date instead of full schedule
        /// </summary>
        /// <param name="scheduleId">Schedule ID</param>
        /// <param name="specialDayId">Special day ID</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Optimization result with performance metadata</returns>
        Task<SpecialDayOptimizedResponse> DeleteSpecialDayOptimizedAsync(int scheduleId, int specialDayId, int userId);

        // === INTEGRATED EVENT RETRIEVAL ===

        /// <summary>
        /// Get schedule events for date range (includes special days from inline generation)
        /// </summary>
        /// <param name="scheduleId">Schedule ID</param>
        /// <param name="startDate">Start date for range</param>
        /// <param name="endDate">End date for range</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <param name="courseId">Optional course ID to filter events</param>
        /// <returns>Schedule events (lessons + special days)</returns>
        Task<List<ScheduleEventResource>> GetEventsByDateRangeAsync(int scheduleId, DateTime startDate, DateTime endDate, int userId, int? courseId = null);

    }
}
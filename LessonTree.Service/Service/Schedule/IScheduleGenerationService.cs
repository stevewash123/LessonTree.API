using LessonTree.Models.DTO;

namespace LessonTree.BLL.Services
{
    /// <summary>
    /// Interface for schedule generation business logic
    /// Handles schedule generation, validation, and sequence analysis
    /// </summary>
    public interface IScheduleGenerationService
    {
        /// <summary>
        /// Generate complete schedule from configuration
        /// </summary>
        /// <param name="configurationId">Configuration ID</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Generated schedule result</returns>
        Task<ScheduleGenerationResult> GenerateScheduleFromConfigurationAsync(int configurationId, int userId);

        /// <summary>
        /// Validate configuration for schedule generation
        /// </summary>
        /// <param name="configurationId">Configuration ID</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Validation result with details</returns>
        Task<ScheduleValidationResult> ValidateConfigurationForGenerationAsync(int configurationId, int userId);

        /// <summary>
        /// Analyze sequence state for existing schedule
        /// </summary>
        /// <param name="scheduleId">Schedule ID</param>
        /// <param name="afterDate">Date to analyze from</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Sequence analysis result</returns>
        Task<SequenceAnalysisResult> AnalyzeSequenceStateAsync(int scheduleId, DateTime afterDate, int userId);

        /// <summary>
        /// Generate continuation events for existing schedule
        /// </summary>
        /// <param name="scheduleId">Schedule ID</param>
        /// <param name="continuationRequest">Continuation parameters</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>List of continuation events</returns>
        Task<List<ScheduleEventResource>> GenerateSequenceContinuationAsync(int scheduleId, SequenceContinuationRequest continuationRequest, int userId);

        /// <summary>
        /// Apply special day integration to schedule events
        /// </summary>
        /// <param name="baseEvents">Base schedule events</param>
        /// <param name="specialDays">Special days to integrate</param>
        /// <returns>Integrated events with special days</returns>
        Task<List<ScheduleEventResource>> ApplySpecialDayIntegrationAsync(List<ScheduleEventResource> baseEvents, List<SpecialDayResource> specialDays);

        /// <summary>
        /// Update schedule events after a lesson is added
        /// </summary>
        /// <param name="scheduleId">Schedule ID</param>
        /// <param name="lessonId">Lesson ID that was added</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Update result with statistics</returns>
        Task<ScheduleUpdateResult> UpdateScheduleAfterLessonAddedAsync(int scheduleId, int lessonId, int userId);

        /// <summary>
        /// Update schedule events after a lesson is moved
        /// </summary>
        /// <param name="scheduleId">Schedule ID</param>
        /// <param name="lessonId">Lesson ID that was moved</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Update result with statistics</returns>
        Task<ScheduleUpdateResult> UpdateScheduleAfterLessonMovedAsync(int scheduleId, int lessonId, int userId);
    }
}
using LessonTree.Models.DTO;

namespace LessonTree.BLL.Services
{
    /// <summary>
    /// Interface for background schedule processing operations
    /// Handles long-running schedule rebuilds without blocking API responses
    /// </summary>
    public interface IBackgroundScheduleService
    {
        /// <summary>
        /// Enqueue a full schedule rebuild in the background with deduplication
        /// Only one rebuild per schedule can be running at a time
        /// </summary>
        /// <param name="scheduleId">Schedule ID to rebuild</param>
        /// <param name="configurationId">Configuration ID for the rebuild</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <param name="reason">Reason for the rebuild (for logging)</param>
        /// <returns>Job ID for tracking</returns>
        string EnqueueScheduleRebuild(int scheduleId, int configurationId, int userId, string reason);

        /// <summary>
        /// Perform the actual background schedule rebuild
        /// This method is called by Hangfire in the background
        /// </summary>
        /// <param name="scheduleId">Schedule ID to rebuild</param>
        /// <param name="configurationId">Configuration ID for the rebuild</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <param name="reason">Reason for the rebuild (for logging)</param>
        /// <returns>Rebuild result with performance metrics</returns>
        Task<ScheduleResource> ExecuteScheduleRebuildAsync(int scheduleId, int configurationId, int userId, string reason);

        /// <summary>
        /// Check if a schedule rebuild is currently in progress
        /// </summary>
        /// <param name="scheduleId">Schedule ID to check</param>
        /// <returns>True if rebuild is in progress</returns>
        bool IsRebuildInProgress(int scheduleId);

        /// <summary>
        /// Get the status of a background job
        /// </summary>
        /// <param name="jobId">Job ID to check</param>
        /// <returns>Job status information</returns>
        BackgroundJobStatus GetJobStatus(string jobId);
    }

    /// <summary>
    /// Background job status information
    /// </summary>
    public class BackgroundJobStatus
    {
        public string JobId { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty; // Enqueued, Processing, Succeeded, Failed, etc.
        public DateTime? CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? Reason { get; set; }
        public string? ErrorMessage { get; set; }
        public bool IsCompleted => State == "Succeeded" || State == "Failed";
        public bool IsRunning => State == "Processing";
    }
}
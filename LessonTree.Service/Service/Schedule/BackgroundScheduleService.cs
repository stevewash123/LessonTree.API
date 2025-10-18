using Hangfire;
using Hangfire.Storage;
using LessonTree.Models.DTO;
using LessonTree.DAL.Repositories;
using Microsoft.Extensions.Logging;

namespace LessonTree.BLL.Services
{
    /// <summary>
    /// Background schedule processing service using Hangfire
    /// Provides deduplication and job tracking for long-running schedule rebuilds
    /// </summary>
    public class BackgroundScheduleService : IBackgroundScheduleService
    {
        private readonly IScheduleGenerationService _scheduleGenerationService;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly ILogger<BackgroundScheduleService> _logger;

        public BackgroundScheduleService(
            IScheduleGenerationService scheduleGenerationService,
            IScheduleRepository scheduleRepository,
            ILogger<BackgroundScheduleService> logger)
        {
            _scheduleGenerationService = scheduleGenerationService;
            _scheduleRepository = scheduleRepository;
            _logger = logger;
        }

        /// <summary>
        /// Enqueue a schedule rebuild with automatic deduplication
        /// Uses schedule-specific job IDs to prevent duplicate rebuilds
        /// </summary>
        public string EnqueueScheduleRebuild(int scheduleId, int configurationId, int userId, string reason)
        {
            var jobId = $"schedule-rebuild-{scheduleId}";

            _logger.LogInformation($"Enqueueing background schedule rebuild - Schedule: {scheduleId}, Config: {configurationId}, Reason: {reason}");

            // Delete any existing job for this schedule (provides deduplication + cancellation)
            try
            {
                BackgroundJob.Delete(jobId);
                _logger.LogInformation($"Cancelled existing rebuild job for schedule {scheduleId}");
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"No existing job to cancel for schedule {scheduleId}: {ex.Message}");
            }

            // Enqueue new job with unique ID
            var newJobId = BackgroundJob.Enqueue<IBackgroundScheduleService>(
                jobId,
                service => service.ExecuteScheduleRebuildAsync(scheduleId, configurationId, userId, reason)
            );

            _logger.LogInformation($"Enqueued schedule rebuild job {newJobId} for schedule {scheduleId}");
            return newJobId;
        }

        /// <summary>
        /// Execute the actual schedule rebuild in background
        /// Called by Hangfire background processor
        /// </summary>
        public async Task<ScheduleResource> ExecuteScheduleRebuildAsync(int scheduleId, int configurationId, int userId, string reason)
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation($"🔄 Starting background schedule rebuild - Schedule: {scheduleId}, Config: {configurationId}, Reason: {reason}");

            try
            {
                // Get the schedule and validate ownership
                var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
                if (schedule == null || schedule.UserId != userId)
                {
                    throw new ArgumentException($"Schedule {scheduleId} not found or not accessible by user {userId}");
                }

                // Generate new events for the entire schedule using the full generation service
                var generationResult = await _scheduleGenerationService.GenerateScheduleFromConfigurationAsync(configurationId, userId);

                if (!generationResult.Success || generationResult.Schedule == null)
                {
                    throw new InvalidOperationException($"Schedule generation failed: {string.Join(", ", generationResult.Errors)}");
                }

                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation($"✅ Completed background schedule rebuild in {duration.TotalSeconds:F2}s - Schedule: {scheduleId}");

                return generationResult.Schedule;
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(ex, $"❌ Failed background schedule rebuild after {duration.TotalSeconds:F2}s - Schedule: {scheduleId}, Error: {ex.Message}");
                throw; // Let Hangfire handle the retry logic
            }
        }

        /// <summary>
        /// Check if a rebuild is currently in progress for a schedule
        /// </summary>
        public bool IsRebuildInProgress(int scheduleId)
        {
            var jobId = $"schedule-rebuild-{scheduleId}";

            try
            {
                using var connection = JobStorage.Current.GetConnection();
                var jobData = connection.GetJobData(jobId);

                if (jobData == null) return false;

                // Check if job is in processing state
                return jobData.State == "Processing" || jobData.State == "Enqueued";
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Error checking rebuild status for schedule {scheduleId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get detailed status information for a background job
        /// </summary>
        public BackgroundJobStatus GetJobStatus(string jobId)
        {
            try
            {
                using var connection = JobStorage.Current.GetConnection();
                var jobData = connection.GetJobData(jobId);

                if (jobData == null)
                {
                    return new BackgroundJobStatus
                    {
                        JobId = jobId,
                        State = "NotFound"
                    };
                }

                // Note: GetStateHistory might not be available in all Hangfire versions
                // Simplified status reporting
                var createdAt = jobData.CreatedAt;
                var startedAt = jobData.State == "Processing" ? DateTime.UtcNow : (DateTime?)null;
                var completedAt = (jobData.State == "Succeeded" || jobData.State == "Failed") ? DateTime.UtcNow : (DateTime?)null;
                return new BackgroundJobStatus
                {
                    JobId = jobId,
                    State = jobData.State,
                    CreatedAt = createdAt,
                    StartedAt = startedAt,
                    CompletedAt = completedAt,
                    Reason = jobData.Job?.Args?.ElementAtOrDefault(3)?.ToString(), // Extract reason parameter
                    ErrorMessage = jobData.State == "Failed" ? "Job failed" : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting job status for {jobId}: {ex.Message}");
                return new BackgroundJobStatus
                {
                    JobId = jobId,
                    State = "Error",
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
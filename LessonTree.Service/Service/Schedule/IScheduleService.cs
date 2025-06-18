using LessonTree.Models.DTO;

namespace LessonTree.BLL.Services
{
    /// <summary>
    /// Service interface for schedule operations
    /// Handles CRUD operations for user schedules and special days
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
        /// <returns>Updated special day resource</returns>
        Task<SpecialDayResource> UpdateSpecialDayAsync(int scheduleId, int specialDayId, SpecialDayUpdateResource updateResource, int userId);

        /// <summary>
        /// Delete special day
        /// </summary>
        /// <param name="scheduleId">Schedule ID</param>
        /// <param name="specialDayId">Special day ID</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Task completion</returns>
        Task DeleteSpecialDayAsync(int scheduleId, int specialDayId, int userId);
    }
}
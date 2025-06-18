using LessonTree.Models.DTO;

namespace LessonTree.BLL.Services
{
    /// <summary>
    /// Service interface for schedule configuration operations
    /// Handles CRUD operations for user schedule configurations/templates
    /// </summary>
    public interface IScheduleConfigurationService
    {
        /// <summary>
        /// Get all schedule configurations for a user
        /// </summary>
        /// <param name="userId">User ID to filter configurations</param>
        /// <returns>List of schedule configuration resources</returns>
        Task<List<ScheduleConfigurationResource>> GetAllAsync(int userId);

        /// <summary>
        /// Get schedule configuration by ID with user ownership validation
        /// </summary>
        /// <param name="id">Schedule configuration ID</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Schedule configuration resource or null if not found/not owned</returns>
        Task<ScheduleConfigurationResource?> GetByIdAsync(int id, int userId);

        /// <summary>
        /// Get user's active schedule configuration
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Active schedule configuration or null if none active</returns>
        Task<ScheduleConfigurationResource?> GetActiveAsync(int userId);

        /// <summary>
        /// Get schedule configuration by user and school year
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="schoolYear">School year to find</param>
        /// <returns>Schedule configuration resource or null if not found</returns>
        Task<ScheduleConfigurationResource?> GetBySchoolYearAsync(int userId, string schoolYear);

        /// <summary>
        /// Get all templates for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of template schedule configurations</returns>
        Task<List<ScheduleConfigurationResource>> GetTemplatesAsync(int userId);

        /// <summary>
        /// Create new schedule configuration
        /// </summary>
        /// <param name="resource">Schedule configuration creation data</param>
        /// <param name="userId">User ID for ownership assignment</param>
        /// <returns>Created schedule configuration resource</returns>
        Task<ScheduleConfigurationResource> CreateAsync(ScheduleConfigurationCreateResource resource, int userId);

        /// <summary>
        /// Update existing schedule configuration
        /// </summary>
        /// <param name="id">Schedule configuration ID</param>
        /// <param name="resource">Schedule configuration update data</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Updated schedule configuration resource</returns>
        Task<ScheduleConfigurationResource> UpdateAsync(int id, ScheduleConfigurationUpdateResource resource, int userId);

        /// <summary>
        /// Delete schedule configuration
        /// </summary>
        /// <param name="id">Schedule configuration ID</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Task completion</returns>
        Task DeleteAsync(int id, int userId);

        /// <summary>
        /// Set schedule configuration as active for user
        /// </summary>
        /// <param name="id">Schedule configuration ID</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Updated schedule configuration resource</returns>
        Task<ScheduleConfigurationResource> SetActiveAsync(int id, int userId);

        /// <summary>
        /// Copy schedule configuration as template
        /// </summary>
        /// <param name="id">Source schedule configuration ID</param>
        /// <param name="request">Copy configuration request with new title</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>New template schedule configuration resource</returns>
        Task<ScheduleConfigurationResource> CopyAsTemplateAsync(int id, CopyConfigurationRequest request, int userId);

        /// <summary>
        /// Validate schedule configuration
        /// </summary>
        /// <param name="id">Schedule configuration ID</param>
        /// <param name="userId">User ID for ownership validation</param>
        /// <returns>Validation result resource</returns>
        Task<ScheduleConfigurationValidationResource> ValidateAsync(int id, int userId);

        /// <summary>
        /// Get schedule configuration summary list for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of schedule configuration summaries</returns>
        Task<List<ScheduleConfigurationSummaryResource>> GetSummariesAsync(int userId);
    }
}
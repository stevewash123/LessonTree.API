using LessonTree.DAL.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LessonTree.DAL.Repositories
{
    public interface IScheduleConfigurationRepository
    {
        // === BASIC CRUD ===
        Task<List<ScheduleConfiguration>> GetByUserIdAsync(int userId);
        Task<ScheduleConfiguration?> GetByIdAsync(int id);
        Task<ScheduleConfiguration?> GetActiveByUserIdAsync(int userId);
        Task<ScheduleConfiguration?> GetByUserIdAndSchoolYearAsync(int userId, string schoolYear);
        Task<ScheduleConfiguration> CreateAsync(ScheduleConfiguration configuration);
        Task<ScheduleConfiguration> UpdateAsync(ScheduleConfiguration configuration);
        Task DeleteAsync(int id);

        // === ACTIVE CONFIGURATION MANAGEMENT ===
        Task<ScheduleConfiguration> SetActiveConfigurationAsync(int userId, int configurationId);

        // === STATUS MANAGEMENT ===
        Task<ScheduleConfiguration> ArchiveConfigurationAsync(int userId, int configurationId);
        Task UpdateHistoricalStatusAsync();

        // === TEMPLATE OPERATIONS ===
        Task<ScheduleConfiguration> CopyAsTemplateAsync(int sourceConfigurationId, string newTitle);
        Task<List<ScheduleConfiguration>> GetTemplatesAsync(int userId);
    }
}

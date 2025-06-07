// **COMPLETE FILE** - JWT-aligned IScheduleRepository interface
// RESPONSIBILITY: Schedule data access contract with user filtering
// DOES NOT: Allow cross-user data access without explicit userId
// CALLED BY: ScheduleController and any schedule services

using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;

namespace LessonTree.DAL.Repositories
{
    public interface IScheduleRepository
    {
        // User-filtered methods (security-first approach)
        Task<Schedule?> GetByIdAsync(int scheduleId);
        Task<Schedule?> GetByIdAndUserAsync(int scheduleId, int userId);
        Task<List<Schedule>> GetByCourseAndUserAsync(int courseId, int userId);
        Task<List<Schedule>> GetByUserAsync(int userId);

        // CRUD operations
        Task<Schedule> CreateAsync(Schedule schedule);
        Task<Schedule?> UpdateConfigAsync(int scheduleId, ScheduleConfigUpdateResource config);
        Task<Schedule?> UpdateScheduleEventsAsync(int scheduleId, List<ScheduleEventResource> scheduleEventResources);
        Task<bool> DeleteAsync(int scheduleId);

        // Legacy method (marked for deprecation)
        [Obsolete("Use GetByCourseAndUserAsync for security")]
        Task<List<Schedule>> GetByCourseIdAsync(int courseId);
    }
}
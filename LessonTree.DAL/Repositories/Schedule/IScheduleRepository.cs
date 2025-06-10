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
        // Master schedule operations (user-based, not course-based)
        Task<Schedule?> GetByUserIdAsync(int userId);
        Task<Schedule?> GetByIdAsync(int id);
        Task<Schedule> CreateAsync(Schedule schedule);
        Task<Schedule> UpdateAsync(Schedule schedule);
        Task DeleteAsync(int id);

        // Schedule event operations
        Task<Schedule> UpdateScheduleEventsAsync(int scheduleId, List<ScheduleEvent> events);
        Task<ScheduleEvent> AddScheduleEventAsync(ScheduleEvent scheduleEvent);
        Task<ScheduleEvent> UpdateScheduleEventAsync(ScheduleEvent scheduleEvent);
        Task DeleteScheduleEventAsync(int scheduleEventId);

        // Query operations
        Task<List<Schedule>> GetSchedulesByUserIdAsync(int userId); // For potential multiple schedules
        Task<bool> UserHasScheduleAsync(int userId);
    }
}
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
        // === SCHEDULE CRUD ===
        Task<Schedule> CreateOrReplaceScheduleAsync(int userId, List<ScheduleEvent> events, int? scheduleConfigurationId = null);
        Task DeleteScheduleAsync(int userId);

        // === SCHEDULE RETRIEVAL ===
        Task<Schedule?> GetByUserIdAsync(int userId);
        Task<Schedule?> GetByIdAsync(int id);
        Task<List<Schedule>> GetSchedulesByUserIdAsync(int userId);
        Task<bool> UserHasScheduleAsync(int userId);
        Task<Schedule?> GetByConfigurationIdAsync(int configurationId);

        // === SCHEDULE EVENT OPERATIONS ===
        Task<Schedule> UpdateScheduleEventsAsync(int scheduleId, List<ScheduleEvent> events);
        Task<ScheduleEvent> AddScheduleEventAsync(ScheduleEvent scheduleEvent);
        Task<ScheduleEvent> UpdateScheduleEventAsync(ScheduleEvent scheduleEvent);
        Task DeleteScheduleEventAsync(int scheduleEventId);

        // === SPECIAL DAY OPERATIONS ===
        Task<SpecialDay> AddSpecialDayAsync(int scheduleId, SpecialDayCreateResource createResource);
        Task<SpecialDay> UpdateSpecialDayAsync(SpecialDayUpdateResource updateResource);
        Task DeleteSpecialDayAsync(int specialDayId);

    }
}
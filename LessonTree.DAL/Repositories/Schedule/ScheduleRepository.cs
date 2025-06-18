// **COMPLETE FILE** - ScheduleRepository.cs - Master schedule CRUD operations
// RESPONSIBILITY: Schedule data access for events and special days only
// DOES NOT: Handle schedule configuration CRUD (that's ScheduleConfigurationRepository)
// CALLED BY: ScheduleController for event operations, services for master schedule retrieval

using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LessonTree.DAL.Repositories
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly LessonTreeContext _context;
        private readonly ILogger<ScheduleRepository> _logger;

        public ScheduleRepository(LessonTreeContext context, ILogger<ScheduleRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // === SCHEDULE CRUD ===

        public async Task<Schedule> CreateOrReplaceScheduleAsync(int userId, List<ScheduleEvent> events, int? scheduleConfigurationId = null)
        {
            _logger.LogInformation($"CreateOrReplaceScheduleAsync: Creating/replacing schedule for user {userId} with {events.Count} events");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Remove existing master schedule if it exists
                var existingSchedule = await _context.Schedules
                    .Include(s => s.ScheduleEvents)
                    .Include(s => s.SpecialDays)
                    .FirstOrDefaultAsync(s => s.UserId == userId);

                if (existingSchedule != null)
                {
                    _logger.LogInformation($"CreateOrReplaceScheduleAsync: Removing existing schedule {existingSchedule.Id} for user {userId}");
                    _context.Schedules.Remove(existingSchedule);
                }

                // Create new schedule
                var newSchedule = new Schedule
                {
                    UserId = userId,
                    CreatedDate = DateTime.UtcNow,
                    ScheduleEvents = new List<ScheduleEvent>(),
                    SpecialDays = new List<SpecialDay>()
                };
                if (scheduleConfigurationId.HasValue)
                {
                    newSchedule.ScheduleConfigurationId = scheduleConfigurationId.Value;
                }

                // Add events
                foreach (var evt in events)
                {
                    evt.ScheduleId = 0; // Will be set when schedule is saved
                    evt.Id = 0; // Reset for new entity
                    newSchedule.ScheduleEvents.Add(evt);
                }

                _context.Schedules.Add(newSchedule);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation($"CreateOrReplaceScheduleAsync: Created schedule {newSchedule.Id} with {events.Count} events for user {userId}");

                // Return with includes
                return await GetByIdAsync(newSchedule.Id) ?? newSchedule;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"CreateOrReplaceScheduleAsync: Failed to create/replace schedule for user {userId}");
                throw;
            }
        }

        public async Task DeleteScheduleAsync(int userId)
        {
            _logger.LogInformation($"DeleteScheduleAsync: Deleting schedule for user {userId}");

            var schedule = await _context.Schedules
                .Include(s => s.ScheduleEvents)
                .Include(s => s.SpecialDays)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (schedule == null)
            {
                _logger.LogInformation($"DeleteScheduleAsync: No schedule found for user {userId}");
                return;
            }

            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"DeleteScheduleAsync: Deleted schedule {schedule.Id} for user {userId}");
        }

        // === SCHEDULE RETRIEVAL ===

        public async Task<Schedule?> GetByUserIdAsync(int userId)
        {
            _logger.LogInformation($"GetByUserIdAsync: Fetching schedule for user {userId}");

            var schedule = await _context.Schedules
                .Include(s => s.ScheduleConfiguration) // Include configuration for display
                    .ThenInclude(sc => sc.PeriodAssignments)
                .Include(s => s.ScheduleEvents.OrderBy(e => e.Date).ThenBy(e => e.Period))
                    .ThenInclude(e => e.Lesson) // Include lesson data for display
                .Include(s => s.SpecialDays.OrderBy(sd => sd.Date))
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (schedule != null)
            {
                _logger.LogInformation($"GetByUserIdAsync: Found schedule {schedule.Id} with {schedule.ScheduleEvents.Count} events and {schedule.SpecialDays.Count} special days for user {userId}");
            }
            else
            {
                _logger.LogInformation($"GetByUserIdAsync: No schedule found for user {userId}");
            }

            return schedule;
        }

        public async Task<Schedule?> GetByIdAsync(int id)
        {
            _logger.LogInformation($"GetByIdAsync: Fetching schedule {id}");

            var schedule = await _context.Schedules
                .Include(s => s.ScheduleConfiguration) // Include configuration for display
                    .ThenInclude(sc => sc.PeriodAssignments)
                .Include(s => s.ScheduleEvents.OrderBy(e => e.Date).ThenBy(e => e.Period))
                    .ThenInclude(e => e.Lesson)
                .Include(s => s.SpecialDays.OrderBy(sd => sd.Date))
                .Include(s => s.User) // Include user for validation
                .FirstOrDefaultAsync(s => s.Id == id);

            if (schedule != null)
            {
                _logger.LogInformation($"GetByIdAsync: Found schedule {id} for user {schedule.UserId} with {schedule.ScheduleEvents.Count} events and {schedule.SpecialDays.Count} special days");
            }
            else
            {
                _logger.LogWarning($"GetByIdAsync: Schedule {id} not found");
            }

            return schedule;
        }

        public async Task<List<Schedule>> GetSchedulesByUserIdAsync(int userId)
        {
            _logger.LogInformation($"GetSchedulesByUserIdAsync: Fetching all schedules for user {userId}");

            var schedules = await _context.Schedules
                .Include(s => s.ScheduleConfiguration)
                .Include(s => s.ScheduleEvents.OrderBy(e => e.Date).ThenBy(e => e.Period))
                .Include(s => s.SpecialDays.OrderBy(sd => sd.Date))
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();

            _logger.LogInformation($"GetSchedulesByUserIdAsync: Found {schedules.Count} schedules for user {userId}");

            return schedules;
        }

        public async Task<bool> UserHasScheduleAsync(int userId)
        {
            return await _context.Schedules.AnyAsync(s => s.UserId == userId);
        }

        // === SCHEDULE EVENT OPERATIONS ===

        public async Task<Schedule> UpdateScheduleEventsAsync(int scheduleId, List<ScheduleEvent> events)
        {
            _logger.LogInformation($"UpdateScheduleEventsAsync: Updating {events.Count} events for schedule {scheduleId}");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var schedule = await _context.Schedules
                    .Include(s => s.ScheduleEvents)
                    .FirstOrDefaultAsync(s => s.Id == scheduleId);

                if (schedule == null)
                {
                    throw new ArgumentException($"Schedule {scheduleId} not found");
                }

                // Remove existing events
                _context.ScheduleEvents.RemoveRange(schedule.ScheduleEvents);

                // Add new events
                foreach (var evt in events)
                {
                    evt.ScheduleId = scheduleId; // Ensure correct schedule ID
                    evt.Id = 0; // Reset ID for new entities
                }

                _context.ScheduleEvents.AddRange(events);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation($"UpdateScheduleEventsAsync: Updated {events.Count} events for schedule {scheduleId}");

                // Return updated schedule with events
                return await GetByIdAsync(scheduleId) ?? schedule;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"UpdateScheduleEventsAsync: Failed to update events for schedule {scheduleId}");
                throw;
            }
        }

        public async Task<ScheduleEvent> AddScheduleEventAsync(ScheduleEvent scheduleEvent)
        {
            _logger.LogInformation($"AddScheduleEventAsync: Adding event for schedule {scheduleEvent.ScheduleId} on {scheduleEvent.Date:yyyy-MM-dd} period {scheduleEvent.Period}");

            // Verify schedule exists
            var schedule = await _context.Schedules.FindAsync(scheduleEvent.ScheduleId);
            if (schedule == null)
            {
                throw new ArgumentException($"Schedule {scheduleEvent.ScheduleId} not found");
            }

            // Check for existing event at same date/period
            var existingEvent = await _context.ScheduleEvents
                .FirstOrDefaultAsync(e => e.ScheduleId == scheduleEvent.ScheduleId &&
                                       e.Date.Date == scheduleEvent.Date.Date &&
                                       e.Period == scheduleEvent.Period);

            if (existingEvent != null)
            {
                throw new InvalidOperationException($"Event already exists for schedule {scheduleEvent.ScheduleId} on {scheduleEvent.Date:yyyy-MM-dd} period {scheduleEvent.Period}");
            }

            _context.ScheduleEvents.Add(scheduleEvent);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"AddScheduleEventAsync: Added event {scheduleEvent.Id}");

            // Return with includes
            return await _context.ScheduleEvents
                .Include(e => e.Lesson)
                .FirstOrDefaultAsync(e => e.Id == scheduleEvent.Id) ?? scheduleEvent;
        }

        public async Task<ScheduleEvent> UpdateScheduleEventAsync(ScheduleEvent scheduleEvent)
        {
            _logger.LogInformation($"UpdateScheduleEventAsync: Updating event {scheduleEvent.Id}");

            var existingEvent = await _context.ScheduleEvents.FindAsync(scheduleEvent.Id);
            if (existingEvent == null)
            {
                throw new ArgumentException($"ScheduleEvent {scheduleEvent.Id} not found");
            }

            // Update fields
            existingEvent.CourseId = scheduleEvent.CourseId;
            existingEvent.Date = scheduleEvent.Date;
            existingEvent.Period = scheduleEvent.Period;
            existingEvent.LessonId = scheduleEvent.LessonId;
            existingEvent.EventType = scheduleEvent.EventType;
            existingEvent.EventCategory = scheduleEvent.EventCategory;
            existingEvent.Comment = scheduleEvent.Comment;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"UpdateScheduleEventAsync: Updated event {scheduleEvent.Id}");

            // Return with includes
            return await _context.ScheduleEvents
                .Include(e => e.Lesson)
                .FirstOrDefaultAsync(e => e.Id == scheduleEvent.Id) ?? existingEvent;
        }

        public async Task DeleteScheduleEventAsync(int scheduleEventId)
        {
            _logger.LogInformation($"DeleteScheduleEventAsync: Deleting event {scheduleEventId}");

            var scheduleEvent = await _context.ScheduleEvents.FindAsync(scheduleEventId);
            if (scheduleEvent == null)
            {
                throw new ArgumentException($"ScheduleEvent {scheduleEventId} not found");
            }

            _context.ScheduleEvents.Remove(scheduleEvent);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"DeleteScheduleEventAsync: Deleted event {scheduleEventId}");
        }

        // === SPECIAL DAY OPERATIONS ===

        public async Task<SpecialDay> AddSpecialDayAsync(int scheduleId, SpecialDayCreateResource createResource)
        {
            _logger.LogInformation($"AddSpecialDayAsync: Adding special day for schedule {scheduleId} on {createResource.Date:yyyy-MM-dd}");

            // Verify schedule exists
            var schedule = await _context.Schedules.FindAsync(scheduleId);
            if (schedule == null)
            {
                throw new ArgumentException($"Schedule {scheduleId} not found");
            }

            var specialDay = new SpecialDay
            {
                ScheduleId = scheduleId,
                Date = createResource.Date,
                Periods = System.Text.Json.JsonSerializer.Serialize(createResource.Periods),
                EventType = createResource.EventType,
                Title = createResource.Title
            };

            _context.SpecialDays.Add(specialDay);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"AddSpecialDayAsync: Added special day {specialDay.Id}");

            return specialDay;
        }

        public async Task<SpecialDay> UpdateSpecialDayAsync(SpecialDayUpdateResource updateResource)
        {
            _logger.LogInformation($"UpdateSpecialDayAsync: Updating special day {updateResource.Id}");

            var existingSpecialDay = await _context.SpecialDays.FindAsync(updateResource.Id);
            if (existingSpecialDay == null)
            {
                throw new ArgumentException($"SpecialDay {updateResource.Id} not found");
            }

            // Update fields
            existingSpecialDay.Date = updateResource.Date;
            existingSpecialDay.Periods = System.Text.Json.JsonSerializer.Serialize(updateResource.Periods);
            existingSpecialDay.EventType = updateResource.EventType;
            existingSpecialDay.Title = updateResource.Title;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"UpdateSpecialDayAsync: Updated special day {updateResource.Id}");

            return existingSpecialDay;
        }

        public async Task DeleteSpecialDayAsync(int specialDayId)
        {
            _logger.LogInformation($"DeleteSpecialDayAsync: Deleting special day {specialDayId}");

            var specialDay = await _context.SpecialDays.FindAsync(specialDayId);
            if (specialDay == null)
            {
                throw new ArgumentException($"SpecialDay {specialDayId} not found");
            }

            _context.SpecialDays.Remove(specialDay);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"DeleteSpecialDayAsync: Deleted special day {specialDayId}");
        }

        // === CONFIGURATION-BASED SCHEDULE LOOKUP ===

        public async Task<Schedule?> GetByConfigurationIdAsync(int configurationId)
        {
            _logger.LogInformation($"GetByConfigurationIdAsync: Fetching schedule for configuration {configurationId}");

            var schedule = await _context.Schedules
                .Include(s => s.ScheduleConfiguration) // Include configuration for display
                    .ThenInclude(sc => sc.PeriodAssignments)
                .Include(s => s.ScheduleEvents.OrderBy(e => e.Date).ThenBy(e => e.Period))
                    .ThenInclude(e => e.Lesson) // Include lesson data for display
                .Include(s => s.SpecialDays.OrderBy(sd => sd.Date))
                .Include(s => s.User) // Include user for validation
                .FirstOrDefaultAsync(s => s.ScheduleConfigurationId == configurationId);

            if (schedule != null)
            {
                _logger.LogInformation($"GetByConfigurationIdAsync: Found schedule {schedule.Id} for configuration {configurationId}, user {schedule.UserId} with {schedule.ScheduleEvents.Count} events and {schedule.SpecialDays.Count} special days");
            }
            else
            {
                _logger.LogInformation($"GetByConfigurationIdAsync: No schedule found for configuration {configurationId}");
            }

            return schedule;
        }
    }
}
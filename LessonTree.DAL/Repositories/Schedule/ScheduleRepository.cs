// **COMPLETE FILE** - JWT-aligned ScheduleRepository
// RESPONSIBILITY: Schedule data access with user filtering for security
// DOES NOT: Allow cross-user data access - always filters by userId
// CALLED BY: ScheduleController with userId from JWT claims

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

        // === MASTER SCHEDULE OPERATIONS ===

        public async Task<Schedule?> GetByUserIdAsync(int userId)
        {
            _logger.LogInformation($"GetByUserIdAsync: Fetching schedule for user {userId}");

            var schedule = await _context.Schedules
                .Include(s => s.ScheduleEvents.OrderBy(e => e.Date).ThenBy(e => e.Period))
                    .ThenInclude(e => e.Lesson) // Include lesson data for display
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (schedule != null)
            {
                _logger.LogInformation($"GetByUserIdAsync: Found schedule {schedule.Id} with {schedule.ScheduleEvents.Count} events for user {userId}");
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
                .Include(s => s.ScheduleEvents.OrderBy(e => e.Date).ThenBy(e => e.Period))
                    .ThenInclude(e => e.Lesson)
                .Include(s => s.User) // Include user for validation
                .FirstOrDefaultAsync(s => s.Id == id);

            if (schedule != null)
            {
                _logger.LogInformation($"GetByIdAsync: Found schedule {id} for user {schedule.UserId} with {schedule.ScheduleEvents.Count} events");
            }
            else
            {
                _logger.LogWarning($"GetByIdAsync: Schedule {id} not found");
            }

            return schedule;
        }

        public async Task<Schedule> CreateAsync(Schedule schedule)
        {
            _logger.LogInformation($"CreateAsync: Creating schedule '{schedule.Title}' for user {schedule.UserId}");

            // Verify user doesn't already have a schedule
            var existingSchedule = await GetByUserIdAsync(schedule.UserId);
            if (existingSchedule != null)
            {
                throw new InvalidOperationException($"User {schedule.UserId} already has a schedule (ID: {existingSchedule.Id})");
            }

            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"CreateAsync: Created schedule {schedule.Id} for user {schedule.UserId}");

            // Return with includes
            return await GetByIdAsync(schedule.Id) ?? schedule;
        }

        public async Task<Schedule> UpdateAsync(Schedule schedule)
        {
            _logger.LogInformation($"UpdateAsync: Updating schedule {schedule.Id}");

            var existingSchedule = await _context.Schedules.FindAsync(schedule.Id);
            if (existingSchedule == null)
            {
                throw new ArgumentException($"Schedule {schedule.Id} not found");
            }

            // Update configuration fields (don't update events here)
            existingSchedule.Title = schedule.Title;
            existingSchedule.StartDate = schedule.StartDate;
            existingSchedule.EndDate = schedule.EndDate;
            existingSchedule.TeachingDays = schedule.TeachingDays;
            existingSchedule.IsLocked = schedule.IsLocked;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"UpdateAsync: Updated schedule {schedule.Id}");

            // Return with includes
            return await GetByIdAsync(schedule.Id) ?? existingSchedule;
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation($"DeleteAsync: Deleting schedule {id}");

            var schedule = await _context.Schedules
                .Include(s => s.ScheduleEvents)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null)
            {
                throw new ArgumentException($"Schedule {id} not found");
            }

            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"DeleteAsync: Deleted schedule {id} and {schedule.ScheduleEvents.Count} associated events");
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

        // === QUERY OPERATIONS ===

        public async Task<List<Schedule>> GetSchedulesByUserIdAsync(int userId)
        {
            _logger.LogInformation($"GetSchedulesByUserIdAsync: Fetching all schedules for user {userId}");

            var schedules = await _context.Schedules
                .Include(s => s.ScheduleEvents.OrderBy(e => e.Date).ThenBy(e => e.Period))
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.StartDate)
                .ToListAsync();

            _logger.LogInformation($"GetSchedulesByUserIdAsync: Found {schedules.Count} schedules for user {userId}");

            return schedules;
        }

        public async Task<bool> UserHasScheduleAsync(int userId)
        {
            return await _context.Schedules.AnyAsync(s => s.UserId == userId);
        }
    }

}
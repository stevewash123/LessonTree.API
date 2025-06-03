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

        public async Task<Schedule?> GetByIdAsync(int scheduleId)
        {
            _logger.LogInformation("Fetching schedule with ID {ScheduleId}", scheduleId);
            return await _context.Schedules
                .Include(s => s.ScheduleDays)
                .ThenInclude(sd => sd.Lesson)
                .FirstOrDefaultAsync(s => s.Id == scheduleId);
        }

        public async Task<List<Schedule>> GetByCourseIdAsync(int courseId)
        {
            _logger.LogInformation("Fetching schedules for course ID {CourseId}", courseId);
            return await _context.Schedules
                .Where(s => s.CourseId == courseId)
                .Include(s => s.ScheduleDays)
                .ToListAsync();
        }

        public async Task<Schedule> CreateAsync(Schedule schedule)
        {
            _logger.LogInformation("Creating new schedule for course ID {CourseId} with title {Title}", schedule.CourseId, schedule.Title);
            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();
            return schedule;
        }

        public async Task<ScheduleDay> AddScheduleDayAsync(ScheduleDay scheduleDay)
        {
            _logger.LogInformation("Adding schedule day for schedule ID {ScheduleId} on {Date}", scheduleDay.ScheduleId, scheduleDay.Date);
            _context.ScheduleDays.Add(scheduleDay);
            await _context.SaveChangesAsync();
            return scheduleDay;
        }

        public async Task<Schedule?> UpdateConfigAsync(ScheduleConfigUpdateResource config)
        {
            _logger.LogInformation("Updating schedule config for ID {ScheduleId}", config.Id);

            var existing = await _context.Schedules.FindAsync(config.Id);
            if (existing == null)
                return null;

            // Update ONLY config fields
            existing.Title = config.Title;
            existing.StartDate = config.StartDate;
            existing.EndDate = config.EndDate;
            existing.TeachingDays = config.TeachingDays;
            existing.IsLocked = config.IsLocked;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<Schedule?> UpdateScheduleDaysAsync(int scheduleId, List<ScheduleDayResource> scheduleDayResources)
        {
            _logger.LogInformation("Updating schedule days for ID {ScheduleId}", scheduleId);

            var existing = await _context.Schedules
                .Include(s => s.ScheduleDays)
                .FirstOrDefaultAsync(s => s.Id == scheduleId);

            if (existing == null)
                return null;

            // Replace ONLY ScheduleDays - leave config untouched
            _context.ScheduleDays.RemoveRange(existing.ScheduleDays);

            // Map from ScheduleDayResource to ScheduleDay domain objects
            existing.ScheduleDays = scheduleDayResources.Select(sdr => new ScheduleDay
            {
                ScheduleId = scheduleId,
                Date = sdr.Date,
                LessonId = sdr.LessonId,
                SpecialCode = sdr.SpecialCode,
                Comment = sdr.Comment
            }).ToList();

            await _context.SaveChangesAsync();
            return existing;
        }

    }

    
}
using LessonTree.DAL.Domain;
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
            await GenerateScheduleAsync(schedule.Id);
            return schedule;
        }

        public async Task<ScheduleDay> AddScheduleDayAsync(ScheduleDay scheduleDay)
        {
            _logger.LogInformation("Adding schedule day for schedule ID {ScheduleId} on {Date}", scheduleDay.ScheduleId, scheduleDay.Date);
            _context.ScheduleDays.Add(scheduleDay);
            await _context.SaveChangesAsync();
            return scheduleDay;
        }

        public async Task<ScheduleDay?> UpdateScheduleDayAsync(ScheduleDay scheduleDay)
        {
            _logger.LogInformation("Updating schedule day ID {ScheduleDayId}", scheduleDay.Id);
            var existing = await _context.ScheduleDays.FindAsync(scheduleDay.Id);
            if (existing == null)
            {
                _logger.LogWarning("Schedule day ID {ScheduleDayId} not found", scheduleDay.Id);
                return null;
            }
            existing.Date = scheduleDay.Date;
            existing.LessonId = scheduleDay.LessonId;
            existing.SpecialCode = scheduleDay.SpecialCode;
            existing.Comment = scheduleDay.Comment;
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task GenerateScheduleAsync(int scheduleId)
        {
            _logger.LogInformation("Generating schedule for schedule ID {ScheduleId}", scheduleId);
            var schedule = await _context.Schedules
                .Include(s => s.Course)
                .ThenInclude(c => c.Topics)
                .ThenInclude(t => t.Lessons.Where(l => !l.Archived))
                .Include(s => s.Course)
                .ThenInclude(c => c.Topics)
                .ThenInclude(t => t.SubTopics)
                .ThenInclude(st => st.Lessons.Where(l => !l.Archived))
                .FirstOrDefaultAsync(s => s.Id == scheduleId);
            if (schedule == null)
            {
                _logger.LogError("Schedule ID {ScheduleId} not found", scheduleId);
                throw new InvalidOperationException("Schedule not found");
            }

            var lessons = new List<Lesson>();
            foreach (var topic in schedule.Course.Topics.OrderBy(t => t.SortOrder))
            {
                lessons.AddRange(topic.Lessons.OrderBy(l => l.SortOrder));
                foreach (var subTopic in topic.SubTopics.OrderBy(st => st.SortOrder))
                {
                    lessons.AddRange(subTopic.Lessons.OrderBy(l => l.SortOrder));
                }
            }

            if (!lessons.Any())
            {
                _logger.LogWarning("No non-archived lessons found for course ID {CourseId}", schedule.CourseId);
                return;
            }

            schedule.ScheduleDays = new List<ScheduleDay>();
            int lessonIndex = 0;
            for (int i = 0; i < schedule.NumSchoolDays; i++)
            {
                var currentDate = schedule.StartDate.AddDays(i);
                if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
                    continue; // Skip weekends

                var scheduleDay = new ScheduleDay
                {
                    Date = currentDate,
                    LessonId = lessonIndex < lessons.Count ? lessons[lessonIndex].Id : null,
                    ScheduleId = schedule.Id
                };
                schedule.ScheduleDays.Add(scheduleDay);
                lessonIndex = (lessonIndex + 1) % lessons.Count; // Cycle through lessons
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Schedule generated with ID {ScheduleId}", schedule.Id);
        }
    }

    
}
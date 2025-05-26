using LessonTree.DAL.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LessonTree.DAL.Repositories
{
    public interface IScheduleRepository
    {
        Task<Schedule?> GetByIdAsync(int scheduleId);
        Task<List<Schedule>> GetByCourseIdAsync(int courseId);
        Task<Schedule> CreateAsync(Schedule schedule);
        Task<ScheduleDay> AddScheduleDayAsync(ScheduleDay scheduleDay);
        Task<ScheduleDay?> UpdateScheduleDayAsync(ScheduleDay scheduleDay);
        Task GenerateScheduleAsync(int scheduleId);
    }
}

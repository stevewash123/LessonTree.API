using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LessonTree.Models.DTO
{
    public class ScheduleResource
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<ScheduleDayResource>? ScheduleDays { get; set; }
        public List<string>? TeachingDays { get; set; }  // Optional per TypeScript

        public bool IsLocked { get; set; }
    }
}

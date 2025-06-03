using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LessonTree.Models.DTO
{
    public class ScheduleDayResource
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public DateTime Date { get; set; }
        public int? LessonId { get; set; }
        public string? SpecialCode { get; set; }
        public string? Comment { get; set; }
    }


    public class ScheduleCreateResource
    {
        public string Title { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string>? TeachingDays { get; set; }
    }

    public class ScheduleConfigUpdateResource
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string TeachingDays { get; set; }
        public bool IsLocked { get; set; }
        // NO ScheduleDays
    }

    public class ScheduleDaysUpdateResource
    {
        public int ScheduleId { get; set; }
        public List<ScheduleDayResource> ScheduleDays { get; set; }
        // NO config fields
    }

}

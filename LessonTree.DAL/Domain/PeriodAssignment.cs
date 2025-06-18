using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LessonTree.DAL.Domain;

namespace LessonTree.DAL.Domain
{
    public class PeriodAssignment
    {
        public int Id { get; set; }
        public int ScheduleConfigurationId { get; set; }
        public virtual Domain.ScheduleConfiguration ScheduleConfiguration { get; set; }

        [Range(1, 10, ErrorMessage = "Period must be between 1 and 10")]
        public int Period { get; set; }

        public int? CourseId { get; set; }
        public string? SpecialPeriodType { get; set; } // "Lunch", "HallDuty", etc.

        // SUBSET OF SCHEDULE TEACHING DAYS - Must be subset of ScheduleConfiguration.TeachingDays
        [MaxLength(200)]
        public string TeachingDays { get; set; } = "Monday,Tuesday,Wednesday,Thursday,Friday";

        [MaxLength(50)]
        public string? Room { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [MaxLength(7)]
        public string BackgroundColor { get; set; } = "#2196F3";

        [MaxLength(7)]
        public string FontColor { get; set; } = "#FFFFFF";
    }
}


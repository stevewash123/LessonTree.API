using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LessonTree.DAL.Domain
{
    public class ScheduleConfiguration
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }

        [MaxLength(100)]
        public string Title { get; set; } = string.Empty; // e.g., "2024-2025 Teaching Schedule"

        [MaxLength(20)]
        public string SchoolYear { get; set; } = string.Empty; // e.g., "2024-2025"

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [Range(1, 10, ErrorMessage = "PeriodsPerDay must be between 1 and 10")]
        public int PeriodsPerDay { get; set; } = 6;

        // MASTER TEACHING DAYS - Controls which days are processed for entire schedule
        [MaxLength(200)]
        public string TeachingDays { get; set; } = "Monday,Tuesday,Wednesday,Thursday,Friday";

        // Configuration metadata
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true; // Current active configuration
        public bool IsTemplate { get; set; } = false; // Can be used as template for new years

        // Period assignments for this configuration
        public virtual List<PeriodAssignment> PeriodAssignments { get; set; } = new List<PeriodAssignment>();

        // Navigation to actual schedules using this configuration
        public virtual List<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}

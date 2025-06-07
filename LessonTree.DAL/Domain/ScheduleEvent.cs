using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LessonTree.DAL.Domain
{
    [Index(nameof(ScheduleId), nameof(Date), nameof(Period), IsUnique = true,
           Name = "IX_ScheduleEvents_Schedule_Date_Period")]
    public class ScheduleEvent
    {
        public int Id { get; set; }

        public int ScheduleId { get; set; }
        public virtual Schedule Schedule { get; set; }

        public DateTime Date { get; set; } // The specific day (e.g., 2025-09-01)

        [Range(1, 10, ErrorMessage = "Period must be between 1 and 10")]
        public int Period { get; set; } // NEW: Period number (1-6, etc.)

        public int? LessonId { get; set; } // Nullable if it's a non-teaching period
        public virtual Lesson? Lesson { get; set; } // Nullable for non-teaching periods

        [MaxLength(500)]
        public string? SpecialCode { get; set; } // e.g., "Holiday", "Testing Day", "Lab Day"

        [MaxLength(1000)]
        public string? Comment { get; set; } // User-entered comment for special periods
    }
}
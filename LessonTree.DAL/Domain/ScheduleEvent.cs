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

        public int? CourseId { get; set; }          // NEW: Track which course/duty
                                                    // NOTE: No Course navigation - CourseId can be negative for duties

        public DateTime Date { get; set; }

        [Range(1, 10, ErrorMessage = "Period must be between 1 and 10")]
        public int Period { get; set; }

        public int? LessonId { get; set; }
        public virtual Lesson? Lesson { get; set; }

        public int? SpecialDayId { get; set; }      // NEW: Link to SpecialDay that created this event
        public virtual SpecialDay? SpecialDay { get; set; }

        [MaxLength(50)]
        public string EventType { get; set; }       // RENAMED from SpecialCode, now required

        [MaxLength(50)]
        public string? EventCategory { get; set; }  // NEW: "Lesson", "SpecialPeriod", "SpecialDay", null

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public int ScheduleSort { get; set; }
    }
}
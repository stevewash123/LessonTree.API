// **COMPLETE FILE** - Updated Schedule with SpecialDays Navigation Property
// RESPONSIBILITY: Schedule domain entity with special day persistence
// DOES NOT: Handle special day generation logic (that's in services)
// CALLED BY: ScheduleRepository and EF Core

using System.ComponentModel.DataAnnotations;
using LessonTree.DAL.Domain;

namespace LessonTree.DAL.Domain
{
    public class Schedule
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }

        // Link to configuration used for this schedule
        public int ScheduleConfigurationId { get; set; }
        public virtual ScheduleConfiguration ScheduleConfiguration { get; set; }

        [MaxLength(100)]
        public string Title { get; set; } = string.Empty; // e.g., "Fall 2024 Schedule"

        public bool IsLocked { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Event data (the actual schedule) - NO PERIOD ASSIGNMENTS HERE
        public virtual List<ScheduleEvent> ScheduleEvents { get; set; } = new List<ScheduleEvent>();
        public virtual List<SpecialDay> SpecialDays { get; set; } = new List<SpecialDay>();
    }



}
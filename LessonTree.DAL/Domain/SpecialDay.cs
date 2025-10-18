// **COMPLETE FILE** - SpecialDay Domain Entity
// RESPONSIBILITY: Persists special day configurations for schedule generation
// DOES NOT: Handle schedule events (those are generated from this data)
// CALLED BY: Schedule entity as navigation property

using System.ComponentModel.DataAnnotations;

namespace LessonTree.DAL.Domain
{
    public class SpecialDay
    {
        public int Id { get; set; }

        public int ScheduleId { get; set; }
        public virtual Schedule Schedule { get; set; }

        public DateTime Date { get; set; }

        [MaxLength(50)]
        public string Periods { get; set; } // JSON array as string: "[1,2]" or "[9,10]"

        [MaxLength(100)]
        public string EventType { get; set; } // 'Assembly', 'Testing', 'Holiday', etc.

        [MaxLength(200)]
        public string Title { get; set; } // 'All School Assembly', 'State Testing', etc.

        [MaxLength(1000)]
        public string? Description { get; set; } // Optional detailed description for special day

        [MaxLength(20)]
        public string? BackgroundColor { get; set; } // Custom background color (e.g., "#FF5733")

        [MaxLength(20)]
        public string? FontColor { get; set; } // Custom font color (e.g., "#FFFFFF")
    }
}
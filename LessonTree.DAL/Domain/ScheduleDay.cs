namespace LessonTree.DAL.Domain
{
    public class ScheduleDay
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public virtual Schedule Schedule { get; set; }
        public DateTime Date { get; set; } // The specific day (e.g., 2025-09-01)
        public int? LessonId { get; set; } // Nullable if it’s a non-teaching day
        public virtual Lesson? Lesson { get; set; } // Nullable for non-teaching days
        public string? SpecialCode { get; set; } // e.g., "Holiday", "Testing Day", "Lab Day"
    }
}
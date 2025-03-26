namespace LessonTree.DAL.Domain
{
    public class Schedule
    {
        public int Id { get; set; }
        public string Title { get; set; } // e.g., "2025-2026 School Year Schedule"
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }
        public int UserId { get; set; } // The user who created the schedule
        public virtual User User { get; set; }
        public List<ScheduleDay> ScheduleDays { get; set; } = new List<ScheduleDay>();
    }
}
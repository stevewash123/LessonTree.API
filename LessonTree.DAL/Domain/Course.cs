namespace LessonTree.DAL.Domain
{
    public class Course
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public VisibilityType Visibility { get; set; } = VisibilityType.Private;
        public int? TeamId { get; set; } // New: Optional team association for Visibility = Team
        public virtual Team? Team { get; set; } // New
        public virtual List<Topic> Topics { get; set; } = new List<Topic>();
        public virtual List<Schedule> Schedules { get; set; } = new List<Schedule>();
        public virtual List<Note> Notes { get; set; } = new List<Note>();
        public bool Archived { get; set; } = false;
    }
}
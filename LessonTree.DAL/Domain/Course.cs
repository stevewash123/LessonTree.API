using LessonTree.Models.Enums;

namespace LessonTree.DAL.Domain
{
    using LessonTree.Models.Enums;
    public class Course
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public virtual List<Topic> Topics { get; set; } = new List<Topic>();
        public virtual List<Schedule> Schedules { get; set; } = new List<Schedule>();
        public virtual List<Note> Notes { get; set; } = new List<Note>();
        public bool Archived { get; set; } = false;
        public VisibilityType Visibility { get; set; } = VisibilityType.Private;
        public virtual List<Standard> Standards { get; set; } = new List<Standard>();
    }
}
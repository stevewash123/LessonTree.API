using LessonTree.Models.Enums;

namespace LessonTree.DAL.Domain
{
    public class Topic
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public virtual List<SubTopic> SubTopics { get; set; } = new List<SubTopic>();
        public virtual List<Lesson> Lessons { get; set; } = new List<Lesson>(); // New: Direct lessons
        public virtual List<Note> Notes { get; set; } = new List<Note>();
        public bool Archived { get; set; } = false;
        public VisibilityType Visibility { get; set; } = VisibilityType.Private;

        public virtual List<Standard> Standards { get; set; } = new List<Standard>();
    }
}
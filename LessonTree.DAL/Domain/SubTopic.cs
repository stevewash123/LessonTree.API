using LessonTree.Models.Enums;

namespace LessonTree.DAL.Domain
{
    public class SubTopic
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public int TopicId { get; set; }
        public virtual Topic Topic { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public VisibilityType Visibility { get; set; } = VisibilityType.Private;
        public virtual List<Lesson> Lessons { get; set; } = new List<Lesson>();
        public bool IsDefault { get; set; } = false;
        public virtual List<Note> Notes { get; set; } = new List<Note>();
        public bool Archived { get; set; } = false;
        public int SortOrder { get; set; } = 0; // Default to 0
    }
}
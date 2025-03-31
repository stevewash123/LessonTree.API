using LessonTree.Models.Enums;

namespace LessonTree.DAL.Domain
{
    public class Note
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public int UserId { get; set; }
        public virtual User CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public int? CourseId { get; set; }
        public virtual Course? Course { get; set; }
        public int? TopicId { get; set; }
        public virtual Topic? Topic { get; set; }
        public int? SubTopicId { get; set; }
        public virtual SubTopic? SubTopic { get; set; }
        public int? LessonId { get; set; }
        public virtual Lesson? Lesson { get; set; }
        public VisibilityType Visibility { get; set; } = VisibilityType.Private;
    }
}
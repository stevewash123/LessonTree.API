// File: NoteResource.cs
using LessonTree.Models.Enums;

namespace LessonTree.Models.DTO
{
    public class NoteResource
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Visibility { get; set; } = "Private";
        public DateTime CreatedDate { get; set; }
        public int? TeamId { get; set; }
        public int CreatedBy { get; set; }
        public string Author { get; set; }
        public int? CourseId { get; set; }
        public int? TopicId { get; set; }
        public int? SubTopicId { get; set; }
        public int? LessonId { get; set; }
    }

    public class NoteCreateResource
    {
        public string Content { get; set; } = string.Empty;
        public string Visibility { get; set; } = "Private";
        // Polymorphic fields
        public int? CourseId { get; set; }
        public int? TopicId { get; set; }
        public int? SubTopicId { get; set; }
        public int? LessonId { get; set; }
    }

    public class NoteUpdateResource
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Visibility { get; set; } = "Private";
    }
}
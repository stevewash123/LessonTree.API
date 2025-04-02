// File: CourseResource.cs
using LessonTree.Models.Enums;

namespace LessonTree.Models.DTO
{
    public class CourseResource
    {
        public int Id { get; set; }
        public string NodeId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool HasChildren { get; set; }
        public bool Archived { get; set; }
        public VisibilityType Visibility { get; set; }
        public List<TopicResource> Topics { get; set; } = new List<TopicResource>();
        public List<NoteResource> Notes { get; set; } = new List<NoteResource>(); 
        public List<StandardResource> Standards { get; set; } = new List<StandardResource>(); // Added
    }

    public class CourseCreateResource
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public VisibilityType Visibility { get; set; } = VisibilityType.Private;
    }

    public class CourseUpdateResource
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public VisibilityType Visibility { get; set; }
        public bool Archived { get; set; }
    }
}
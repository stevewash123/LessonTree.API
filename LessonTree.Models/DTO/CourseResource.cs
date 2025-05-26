// File: CourseResource.cs
using LessonTree.Models.Enums;

namespace LessonTree.Models.DTO
{
    public class CourseResource
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string NodeId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool HasChildren { get; set; }
        public bool Archived { get; set; }
        public int UserId { get; set; }
        public string Visibility { get; set; }
        public List<TopicResource> Topics { get; set; } = new List<TopicResource>();
        public List<NoteResource> Notes { get; set; } = new List<NoteResource>(); 
        public List<StandardResource> Standards { get; set; } = new List<StandardResource>(); // Added
        public string NodeType { get; set; } = "Course"; // Add nodeType
        // Note: No SortOrder - courses can't be sorted
    }

    public class CourseCreateResource
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Visibility { get; set; } = "Private";
    }

    public class CourseUpdateResource
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Visibility { get; set; }
        public bool Archived { get; set; }
    }
}
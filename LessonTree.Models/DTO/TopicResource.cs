// File: TopicResource.cs
using LessonTree.Models.Enums;

namespace LessonTree.Models.DTO
{
    public class TopicResource
    {
        public int Id { get; set; }
        public string NodeId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public bool HasChildren { get; set; }
        public bool Archived { get; set; }
        public int SortOrder { get; set; } 
        public VisibilityType Visibility { get; set; }
        public List<SubTopicResource> SubTopics { get; set; } = new List<SubTopicResource>();
        public List<LessonResource> Lessons { get; set; } = new List<LessonResource>();
        public List<NoteResource> Notes { get; set; } = new List<NoteResource>();
    }

    public class TopicCreateResource
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string Visibility { get; set; } = "Private"; 
        public int SortOrder { get; set; }
    }

    // no links, links can only be changed by move
    public class TopicUpdateResource
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Visibility { get; set; }
        public bool Archived { get; set; }
        public int SortOrder { get; set; }
    }

    public class TopicMoveResource
    {
        public int TopicId { get; set; }
        public int NewCourseId { get; set; }
    }
}
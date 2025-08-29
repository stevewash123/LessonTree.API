// ✅ COMPLETE UPDATE: TopicResource.cs - Simple sibling approach

using LessonTree.Models.Enums;
using System.ComponentModel.DataAnnotations;

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
        public int UserId { get; set; }
        public int SortOrder { get; set; }
        public string Visibility { get; set; }
        public string EntityType { get; set; } = "Topic";
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

    public class TopicUpdateResource
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Visibility { get; set; }
        public bool Archived { get; set; }
        public int SortOrder { get; set; }
    }

    // ✅ CLEAN: Simple sibling-based move resource
    public class TopicMoveResource
    {
        [Required]
        public int TopicId { get; set; }

        [Required]
        public int NewCourseId { get; set; }

        // ✅ SIMPLE: Which topic to position after (null = first position)
        public int? AfterSiblingId { get; set; }
    }
}
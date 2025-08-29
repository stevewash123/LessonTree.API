// ✅ COMPLETE UPDATE: SubTopicResource.cs - Simple sibling approach

using LessonTree.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace LessonTree.Models.DTO
{
    public class SubTopicResource
    {
        public int Id { get; set; }
        public string NodeId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TopicId { get; set; }
        public int CourseId { get; set; }
        public bool HasChildren { get; set; }
        public bool Archived { get; set; }
        public int UserId { get; set; }
        public int SortOrder { get; set; }
        public string Visibility { get; set; }
        public string EntityType { get; set; } = "SubTopic";
        public List<LessonResource> Lessons { get; set; } = new List<LessonResource>();
        public List<NoteResource> Notes { get; set; } = new List<NoteResource>();
    }

    public class SubTopicCreateResource
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TopicId { get; set; }
        public string Visibility { get; set; } = "Private";
        public int SortOrder { get; set; }
    }

    public class SubTopicUpdateResource
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Visibility { get; set; }
        public bool Archived { get; set; }
        public int SortOrder { get; set; }
    }

    // ✅ CLEAN: Simple sibling-based move resource
    public class SubTopicMoveResource
    {
        [Required]
        public int SubTopicId { get; set; }

        [Required]
        public int NewTopicId { get; set; }

        // ✅ SIMPLE: Which sibling to position after (null = first position)
        public int? AfterSiblingId { get; set; }
    }
}
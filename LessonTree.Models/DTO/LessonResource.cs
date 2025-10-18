// ✅ COMPLETE UPDATE: LessonResource.cs - Remove all complex validation, use simple sibling approach

using LessonTree.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace LessonTree.Models.DTO
{
    public class LessonResource
    {
        public int Id { get; set; }
        public int? SubTopicId { get; set; }
        public int? TopicId { get; set; }
        public int CourseId { get; set; }
        public string NodeId { get; set; }
        public int SortOrder { get; set; }
        public string Title { get; set; }
        public string Objective { get; set; }
        public bool Archived { get; set; }
        public string Visibility { get; set; } = "Private";
        public int UserId { get; set; }
        public string EntityType { get; set; } = "Lesson";
    }

    public class LessonCreateResource
    {
        public string Title { get; set; } = string.Empty;
        public int? SubTopicId { get; set; }
        public int? TopicId { get; set; }
        public string Visibility { get; set; } = "Private";
        public string? Level { get; set; }
        public string Objective { get; set; } = string.Empty;
        public string? Materials { get; set; }
        public string? ClassTime { get; set; }
        public string? Methods { get; set; }
        public string? SpecialNeeds { get; set; }
        public string? Assessment { get; set; }
        public int SortOrder { get; set; }
    }

    public class LessonUpdateResource
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Level { get; set; }
        public string Objective { get; set; }
        public string? Materials { get; set; }
        public string? ClassTime { get; set; }
        public string? Methods { get; set; }
        public string? SpecialNeeds { get; set; }
        public string? Assessment { get; set; }
        public string Visibility { get; set; }
        public bool Archived { get; set; }
        public int SortOrder { get; set; }
        public string NodeType { get; set; } = "Lesson";
    }

    // ✅ ENHANCED: Lesson move resource with calendar optimization support
    public class LessonMoveResource
    {
        [Required]
        public int LessonId { get; set; }

        public int? NewSubTopicId { get; set; }
        public int? NewTopicId { get; set; }

        // ✅ SIMPLE: Which sibling to position after (null = first position)
        public int? AfterSiblingId { get; set; }

        // ✅ NEW: Calendar optimization fields for partial schedule updates
        public DateTime? CalendarStartDate { get; set; }
        public DateTime? CalendarEndDate { get; set; }
        public bool RequestPartialScheduleUpdate { get; set; } = false;
    }

    public class LessonDetailResource
    {
        public int Id { get; set; }
        public int? SubTopicId { get; set; }
        public int? TopicId { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string Objective { get; set; }
        public string? Level { get; set; }
        public string? Materials { get; set; }
        public string? ClassTime { get; set; }
        public string? Methods { get; set; }
        public string? SpecialNeeds { get; set; }
        public string? Assessment { get; set; }
        public bool Archived { get; set; }
        public int SortOrder { get; set; }
        public string Visibility { get; set; }
        public string EntityType { get; set; } = "Lesson";
        public List<StandardResource> Standards { get; set; } = new List<StandardResource>();
        public List<AttachmentResource> Attachments { get; set; }
        public List<NoteResource> Notes { get; set; } = new List<NoteResource>();
    }
}
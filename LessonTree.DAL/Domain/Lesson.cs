using LessonTree.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace LessonTree.DAL.Domain
{
    public class Lesson
    {
        public int Id { get; set; }
        [MaxLength(200)]
        public string Title { get; set; }
        [MaxLength(500)]
        public string Objective { get; set; }
        [MaxLength(100)]
        public string? Level { get; set; }
        [MaxLength(200)]
        public string? Materials { get; set; }
        [MaxLength(100)]
        public string? ClassTime { get; set; }
        [MaxLength(500)]
        public string? Methods { get; set; }
        [MaxLength(500)]
        public string? SpecialNeeds { get; set; }
        [MaxLength(250)]
        public string? Assessment { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public virtual List<LessonStandard> LessonStandards { get; set; } = new List<LessonStandard>();
        public int? SubTopicId { get; set; } // Nullable
        public virtual SubTopic? SubTopic { get; set; } // Nullable
        public int? TopicId { get; set; } // New: Optional direct link to Topic
        public virtual Topic? Topic { get; set; } // New
        public virtual List<LessonAttachment> LessonAttachments { get; set; } = new List<LessonAttachment>();
        public virtual List<Note> Notes { get; set; } = new List<Note>();
        public bool Archived { get; set; } = false;
        public VisibilityType Visibility { get; set; } = VisibilityType.Private;
        public int SortOrder { get; set; } = 0; // Default to 0
    }
}
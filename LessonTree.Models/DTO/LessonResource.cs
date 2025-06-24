using LessonTree.Models.Enums;

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
        public string NodeType { get; set; } = "Lesson";
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
        public string NodeType { get; set; } = "Lesson"; // Add nodeType
    }

    public class LessonMoveResource
    {
        public int LessonId { get; set; }
        public int? NewSubTopicId { get; set; }
        public int? NewTopicId { get; set; }

        // Optional positioning parameters - if provided, enables precise positioning
        public int? RelativeToId { get; set; }
        public string? Position { get; set; } // "before" | "after"  
        public string? RelativeToType { get; set; } // "Lesson" | "SubTopic"
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
        public string Visibility { get; set; }
        public string NodeType { get; set; } = "Lesson"; // Add nodeType
        public List<StandardResource> Standards { get; set; } = new List<StandardResource>();
        public List<AttachmentResource> Attachments { get; set; }
        public List<NoteResource> Notes { get; set; } = new List<NoteResource>();
    }
}
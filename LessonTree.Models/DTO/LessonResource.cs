namespace LessonTree.Models.DTO
{
    public class LessonResource
    {
        public int Id { get; set; }
        public int? SubTopicId { get; set; } // Read-only, set via Move
        public int? TopicId { get; set; }    // Read-only, set via Move
        public int CourseId { get; set; }
        public string NodeId { get; set; }
        public string Title { get; set; }
        public string Objective { get; set; }
    }

    public class LessonCreateResource
    {
        public string Title { get; set; }
        public int? SubTopicId { get; set; } // Set at creation, not editable later
        public int? TopicId { get; set; }    // Set at creation, not editable later
        public string Visibility { get; set; } = "Private";
        public int? TeamId { get; set; }
        public string? Level { get; set; }
        public string Objective { get; set; }
        public string? Materials { get; set; }
        public string? ClassTime { get; set; }
        public string? Methods { get; set; }
        public string? SpecialNeeds { get; set; }
        public string? Assessment { get; set; }
    }

    public class LessonUpdateResource
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Visibility { get; set; }
        public int? TeamId { get; set; }
        public string? Level { get; set; }
        public string Objective { get; set; }
        public string? Materials { get; set; }
        public string? ClassTime { get; set; }
        public string? Methods { get; set; }
        public string? SpecialNeeds { get; set; }
        public string? Assessment { get; set; }
        // SubTopicId and TopicId already omitted, confirmed correct
    }

    public class LessonMoveResource
    {
        public int LessonId { get; set; }
        public int? NewSubTopicId { get; set; } // Nullable to support moving to Topic
        public int? NewTopicId { get; set; }    // Nullable to support moving to SubTopic
    }

    public class LessonDetailResource
    {
        public int Id { get; set; }
        public int? SubTopicId { get; set; } // Read-only
        public int? TopicId { get; set; }    // Read-only
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string Objective { get; set; }
        public string? Level { get; set; }
        public string? Materials { get; set; }
        public string? ClassTime { get; set; }
        public string? Methods { get; set; }
        public string? SpecialNeeds { get; set; }
        public string? Assessment { get; set; }
        public List<StandardResource> Standards { get; set; } = new List<StandardResource>();
        public List<AttachmentResource> Attachments { get; set; }
    }
}
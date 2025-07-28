// **NEW FILE** - Models/DTO/PositioningResults.cs
// INTEGRATION: Create result classes for EntityPositioningService

namespace LessonTree.Models.DTO
{
    /// <summary>
    /// Information about an entity affected by positioning
    /// </summary>
    public class ModifiedEntityInfo
    {
        public int EntityId { get; set; }
        public string EntityType { get; set; } = string.Empty; // "Lesson", "SubTopic", "Topic"
        public int NewSortOrder { get; set; }
        public int? ParentId { get; set; }
        public string? ParentType { get; set; } // "SubTopic", "Topic", "Course"
    }

    /// <summary>
    /// Base result for all positioning operations
    /// </summary>
    public class PositioningResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public List<ModifiedEntityInfo> ModifiedEntities { get; set; } = new List<ModifiedEntityInfo>();
        public int TargetSortOrder { get; set; }
    }

    /// <summary>
    /// Result for lesson positioning operations
    /// </summary>
    public class LessonPositioningResult : PositioningResult
    {
        public int LessonId { get; set; }
        public int? NewSubTopicId { get; set; }
        public int? NewTopicId { get; set; }
        public string NewParentType => NewSubTopicId.HasValue ? "SubTopic" : "Topic";
        public int NewParentId => NewSubTopicId ?? NewTopicId ?? 0;
    }

    /// <summary>
    /// Result for subtopic positioning operations
    /// </summary>
    public class SubTopicPositioningResult : PositioningResult
    {
        public int SubTopicId { get; set; }
        public int NewTopicId { get; set; }
    }

    /// <summary>
    /// Result for topic positioning operations
    /// </summary>
    public class TopicPositioningResult : PositioningResult
    {
        public int TopicId { get; set; }
        public int NewCourseId { get; set; }
    }
}
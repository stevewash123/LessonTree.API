// **NEW FILE** - Models/DTO/PositioningResults.cs
// INTEGRATION: Create result classes for EntityPositioningService

namespace LessonTree.Models.DTO
{

    public class EntityPositionResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public List<EntityStateInfo> ModifiedEntities { get; set; } = new();

        public static EntityPositionResult Success(List<EntityStateInfo> entities) =>
            new() { IsSuccess = true, ModifiedEntities = entities };

        public static EntityPositionResult Failure(string error) =>
            new() { IsSuccess = false, ErrorMessage = error };
    }


    // ✅ ENTITY STATE: Complete state information for UI updates
    public class EntityStateInfo
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public int? TopicId { get; set; }
        public int? SubTopicId { get; set; }
        public bool IsMovedEntity { get; set; }
    }

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
    /// Enhanced result for lesson positioning operations with calendar optimization support
    /// </summary>
    public class LessonPositioningResult : PositioningResult
    {
        public int LessonId { get; set; }
        public int? NewSubTopicId { get; set; }
        public int? NewTopicId { get; set; }
        public string NewParentType => NewSubTopicId.HasValue ? "SubTopic" : "Topic";
        public int NewParentId => NewSubTopicId ?? NewTopicId ?? 0;

        // ✅ NEW: Calendar optimization fields for efficient frontend updates
        public bool HasPartialScheduleUpdates { get; set; } = false;
        public List<ScheduleEventResource>? PartialScheduleEvents { get; set; }
        public DateTime? UpdatedDateRangeStart { get; set; }
        public DateTime? UpdatedDateRangeEnd { get; set; }
        public bool RequiresFullScheduleRegeneration { get; set; } = true;
        public string? OptimizationReason { get; set; }
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
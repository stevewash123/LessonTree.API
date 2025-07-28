// **NEW FILE** - Models/DTO/PositioningModels.cs
// INTEGRATION: Add these DTOs to support the robust positioning engine

using LessonTree.Models.DTO;
using System.ComponentModel.DataAnnotations;

namespace LessonTree.Models
{
    // ✅ UNIFIED REQUEST: Handle all entity positioning scenarios
    public class PositionMoveRequest
    {
        [Required]
        public int EntityId { get; set; }

        [Required]
        public string EntityType { get; set; } = string.Empty; // "Lesson", "SubTopic"

        public int? NewTopicId { get; set; }
        public int? NewSubTopicId { get; set; }

        public int? RelativeToId { get; set; }
        public string RelativeToType { get; set; } = string.Empty; // "Lesson", "SubTopic"
        public string Position { get; set; } = string.Empty; // "before", "after"
    }

    // ✅ COMPREHENSIVE RESULT: All modified entities with state info
    public class PositionMoveResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public List<EntityStateInfo> ModifiedEntities { get; set; } = new();
        public List<EntityArrangementInfo> FinalArrangement { get; set; } = new();
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

    // ✅ ARRANGEMENT INFO: Final positioning details
    public class EntityArrangementInfo
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsMovedEntity { get; set; }
    }

    // ✅ VALIDATION RESULT: Move validation details
    public class MoveValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    // ✅ BACKWARD COMPATIBILITY: Adapter for existing LessonMoveResource
    public static class PositionMoveExtensions
    {
        public static PositionMoveRequest ToPositionMoveRequest(this LessonMoveResource lessonMove)
        {
            return new PositionMoveRequest
            {
                EntityId = lessonMove.LessonId,
                EntityType = "Lesson",
                NewTopicId = lessonMove.NewTopicId,
                NewSubTopicId = lessonMove.NewSubTopicId,
                RelativeToId = lessonMove.RelativeToId,
                RelativeToType = lessonMove.RelativeToType,
                Position = lessonMove.Position
            };
        }
    }

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

}
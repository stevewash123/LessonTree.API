// **COMPLETE FILE** - SpecialDay DTOs for API
// RESPONSIBILITY: Data transfer objects for SpecialDay CRUD operations
// DOES NOT: Contain business logic or validation
// CALLED BY: ScheduleController for API serialization

namespace LessonTree.Models.DTO
{
    public class SpecialDayResource
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public DateTime Date { get; set; }
        public int[] Periods { get; set; } = new int[0]; // Deserialized from JSON
        public string EventType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; } // ✅ ADD: Description field for Special Day details
        public string? BackgroundColor { get; set; } // ✅ ADD: Custom background color for Special Day
        public string? FontColor { get; set; } // ✅ ADD: Custom font color for Special Day
    }

    public class SpecialDayCreateResource
    {
        public DateTime Date { get; set; }
        public int[] Periods { get; set; } = new int[0];
        public string EventType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; } // ✅ ADD: Description field for Special Day details
        public string? BackgroundColor { get; set; } // ✅ ADD: Custom background color for Special Day
        public string? FontColor { get; set; } // ✅ ADD: Custom font color for Special Day
    }

    public class SpecialDayUpdateResource
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int[] Periods { get; set; } = new int[0];
        public string EventType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; } // ✅ ADD: Description field for Special Day details
        public string? BackgroundColor { get; set; } // ✅ ADD: Custom background color for Special Day
        public string? FontColor { get; set; } // ✅ ADD: Custom font color for Special Day
    }

    public class SpecialDayUpdateResponse
    {
        public SpecialDayResource SpecialDay { get; set; } = new SpecialDayResource();
        public bool CalendarRefreshNeeded { get; set; }
        public string? RefreshReason { get; set; }
    }

    // ✅ NEW: Optimized Special Day response with partial generation metadata
    public class SpecialDayOptimizedResponse
    {
        public SpecialDayResource SpecialDay { get; set; } = new SpecialDayResource();
        public bool IsOptimized { get; set; } = false;
        public bool HasPartialGeneration { get; set; } = false;
        public int PartialEventsGenerated { get; set; } = 0;
        public DateTime? PartialGenerationDate { get; set; }
        public bool RequiresFullRefresh { get; set; } = false;
        public string? OptimizationReason { get; set; }
        public string? PerformanceMetrics { get; set; }
    }
}
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
    }

    public class SpecialDayCreateResource
    {
        public DateTime Date { get; set; }
        public int[] Periods { get; set; } = new int[0];
        public string EventType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; } // ✅ ADD: Description field for Special Day details
    }

    public class SpecialDayUpdateResource
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int[] Periods { get; set; } = new int[0];
        public string EventType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; } // ✅ ADD: Description field for Special Day details
    }

    public class SpecialDayUpdateResponse
    {
        public SpecialDayResource SpecialDay { get; set; } = new SpecialDayResource();
        public bool CalendarRefreshNeeded { get; set; }
        public string? RefreshReason { get; set; }
    }
}
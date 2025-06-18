// **UPDATED** - Schedule DTOs (Simplified - References ScheduleConfiguration)
// RESPONSIBILITY: Schedule event management only
// DOES NOT: Handle configuration data (that's ScheduleConfiguration)
// CALLED BY: ScheduleController for schedule event operations

namespace LessonTree.Models.DTO
{
    // Simplified schedule resource - references configuration
    public class ScheduleResource
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int UserId { get; set; }
        public int ScheduleConfigurationId { get; set; } // NEW: Reference to configuration
        public bool IsLocked { get; set; }
        public DateTime CreatedDate { get; set; }

        // Event data only
        public List<ScheduleEventResource> ScheduleEvents { get; set; } = new List<ScheduleEventResource>();
    }

    // Create schedule from configuration
    public class ScheduleCreateResource
    {
        public string Title { get; set; } = string.Empty;
        public int ScheduleConfigurationId { get; set; } // Reference to configuration to use

        // Optional: Include initial events
        public List<ScheduleEventResource>? ScheduleEvents { get; set; }
    }

    // Update schedule (basic properties only)
    public class ScheduleUpdateResource
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
    }

}
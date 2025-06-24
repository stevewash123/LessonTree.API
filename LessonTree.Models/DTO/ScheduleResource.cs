// **UPDATED** - Schedule DTOs (Simplified - References ScheduleConfiguration)
// RESPONSIBILITY: Schedule event management AND special day data
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
        public int ScheduleConfigurationId { get; set; } // Reference to configuration
        public bool IsLocked { get; set; }
        public DateTime CreatedDate { get; set; }

        // Event data AND special days
        public List<ScheduleEventResource> ScheduleEvents { get; set; } = new List<ScheduleEventResource>();
        public List<SpecialDayResource> SpecialDays { get; set; } = new List<SpecialDayResource>();
    }

    // Create schedule from configuration
    public class ScheduleCreateResource
    {
        public string Title { get; set; } = string.Empty;
        public int ScheduleConfigurationId { get; set; } // Reference to configuration to use

        // Optional: Include initial events and special days
        public List<ScheduleEventResource>? ScheduleEvents { get; set; }
        public List<SpecialDayResource>? SpecialDays { get; set; }
    }

    // Update schedule (basic properties only)
    public class ScheduleUpdateResource
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
    }
}
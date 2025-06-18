// **ScheduleConfiguration DTOs Only** - No PeriodAssignment classes
// RESPONSIBILITY: Schedule configuration DTOs without period assignment resources
// DOES NOT: Include PeriodAssignment DTOs (moved to separate file)
// CALLED BY: ScheduleConfigurationController for configuration operations

namespace LessonTree.Models.DTO
{
    // Complete configuration resource (matches Angular ScheduleConfiguration interface)
    public class ScheduleConfigurationResource
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string SchoolYear { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int PeriodsPerDay { get; set; }
        public string[] TeachingDays { get; set; } = Array.Empty<string>();

        // Status flags
        public bool IsActive { get; set; }

        // Period assignments (references PeriodAssignmentResource from separate file)
        public List<PeriodAssignmentResource> PeriodAssignments { get; set; } = new List<PeriodAssignmentResource>();
    }

    // Create new configuration
    public class ScheduleConfigurationCreateResource
    {
        public string Title { get; set; } = string.Empty;
        // SchoolYear removed - auto-computed from StartDate/EndDate
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int PeriodsPerDay { get; set; } = 6;
        public string[] TeachingDays { get; set; } = Array.Empty<string>();

        public List<PeriodAssignmentResource> PeriodAssignments { get; set; } = new List<PeriodAssignmentResource>();
    }

    // Update existing configuration - SchoolYear auto-computed
    public class ScheduleConfigurationUpdateResource
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        // SchoolYear removed - auto-computed from StartDate/EndDate
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int PeriodsPerDay { get; set; }
        public string[] TeachingDays { get; set; } = Array.Empty<string>();
        public bool IsActive { get; set; }

        public List<PeriodAssignmentResource> PeriodAssignments { get; set; } = new List<PeriodAssignmentResource>();
    }

    // Configuration validation result
    public class ScheduleConfigurationValidationResource
    {
        public bool IsValid { get; set; }
        public bool CanGenerateSchedule { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }

    // Configuration summary for lists
    public class ScheduleConfigurationSummaryResource
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string SchoolYear { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int PeriodCount { get; set; }
        public int AssignedPeriods { get; set; }
    }

    // Copy configuration request
    public class CopyConfigurationRequest
    {
        public string NewTitle { get; set; } = string.Empty;
    }
}
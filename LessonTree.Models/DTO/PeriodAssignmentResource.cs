// **COMPLETE FILE** - Period Assignment DTOs with standardized string[] TeachingDays
// RESPONSIBILITY: Period assignment operations within user configuration
// DOES NOT: Handle user configuration directly (see UserConfigurationResource.cs) or business logic
// CALLED BY: Controllers and UserConfiguration operations

namespace LessonTree.Models.DTO
{
    // Period assignment within configuration
    public class PeriodAssignmentResource
    {
        public int Id { get; set; }
        public int Period { get; set; }
        public int? CourseId { get; set; }
        public string? SpecialPeriodType { get; set; }
        public string[] TeachingDays { get; set; } = Array.Empty<string>();
        public string? Room { get; set; }
        public string? Notes { get; set; }
        public string BackgroundColor { get; set; } = "#2196F3";
        public string FontColor { get; set; } = "#FFFFFF";
    }

    // Create new period assignment
    public class PeriodAssignmentCreateResource
    {
        public int Period { get; set; }
        public int? CourseId { get; set; }
        public string? SpecialPeriodType { get; set; }
        public string[] TeachingDays { get; set; } = Array.Empty<string>();
        public string? Room { get; set; }
        public string? Notes { get; set; }
        public string BackgroundColor { get; set; } = "#2196F3";
        public string FontColor { get; set; } = "#FFFFFF";
    }

    // Update period assignment
    public class PeriodAssignmentUpdateResource
    {
        public int Id { get; set; }
        public int Period { get; set; }
        public int? CourseId { get; set; }
        public string? SpecialPeriodType { get; set; }
        public string[] TeachingDays { get; set; } = Array.Empty<string>();
        public string? Room { get; set; }
        public string? Notes { get; set; }
        public string BackgroundColor { get; set; } = "#2196F3";
        public string FontColor { get; set; } = "#FFFFFF";
    }

}
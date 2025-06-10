// **COMPLETE FILE** - Period Assignment DTOs with standardized string[] TeachingDays
// RESPONSIBILITY: Period assignment operations within user configuration
// DOES NOT: Handle user configuration directly (see UserConfigurationResource.cs) or business logic
// CALLED BY: Controllers and UserConfiguration operations

namespace LessonTree.Models.DTO
{
    public class PeriodAssignmentResource
    {
        public int Id { get; set; }
        public int Period { get; set; }
        public int? CourseId { get; set; }
        public string? SpecialPeriodType { get; set; }
        public string[] TeachingDays { get; set; } = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
        public string? Room { get; set; }
        public string? Notes { get; set; }
        public string BackgroundColor { get; set; } = "#FFFFFF";
        public string FontColor { get; set; } = "#000000";
    }
}
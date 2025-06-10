// **COMPLETE FILE** - User Configuration and Teaching Settings Resources
// RESPONSIBILITY: User teaching configuration, school settings, and period management
// DOES NOT: Handle user identity (see UserResource.cs) or authentication (see AuthenticationResource.cs)
// CALLED BY: Controllers for user configuration operations

namespace LessonTree.Models.DTO
{
    // Clean UserConfiguration without redundant IDs
    public class UserConfigurationResource
    {
        public DateTime LastUpdated { get; set; }
        public string? SchoolYear { get; set; }
        public int PeriodsPerDay { get; set; }
        public List<PeriodAssignmentResource>? PeriodAssignments { get; set; }
    }

    // Configuration updates - backend gets user from JWT
    public class UserConfigurationUpdate
    {
        public string SchoolYear { get; set; } = string.Empty;
        public int PeriodsPerDay { get; set; }
        public List<PeriodAssignmentResource> PeriodAssignments { get; set; } = new();
    }
}
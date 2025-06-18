// **UPDATED** - Simplified UserConfiguration DTOs
// RESPONSIBILITY: Basic user account settings only (no schedule configuration)
// DOES NOT: Handle period assignments or schedule settings (moved to ScheduleConfiguration)
// CALLED BY: UserController for account management

namespace LessonTree.Models.DTO
{
    // Simplified user configuration resource
    public class UserConfigurationResource
    {
        public DateTime LastUpdated { get; set; }

        // Future: Can add user preference settings here
        // public bool EmailNotifications { get; set; }
        // public string Theme { get; set; }
        // public string TimeZone { get; set; }

        // REMOVED: All schedule-related properties moved to ScheduleConfigurationResource
    }

    // Update user configuration
    public class UserConfigurationUpdate
    {
        // Basic user preferences only
        // Future expansion for user settings

    }

}
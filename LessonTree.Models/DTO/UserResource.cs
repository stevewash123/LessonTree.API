// **COMPLETE FILE** - Transitional User Models for JWT Migration
// RESPONSIBILITY: Bridge between old ID-based and new JWT-based architecture
// DOES NOT: Represent final state - includes duplicate data during transition
// CALLED BY: Controllers during migration period

namespace LessonTree.Models.DTO
{
    // Transitional UserResource - contains both JWT and application data during migration
    public class UserResource
    {
        // JWT DATA (duplicate during transition - will be removed when all controllers are JWT-aligned)
        public int Id { get; set; }                          // TODO: Remove when JWT complete
        public string Username { get; set; } = string.Empty; // TODO: Remove when JWT complete  
        public string Password { get; set; } = string.Empty; // TODO: Remove when JWT complete (security)
        public string? FirstName { get; set; }               // TODO: Remove when JWT complete
        public string? LastName { get; set; }                // TODO: Remove when JWT complete
        public string? Email { get; set; }                   // TODO: Remove when JWT complete
        public string? Phone { get; set; }                   // TODO: Remove when JWT complete

        // APPLICATION DATA (permanent - business logic)
        public int? District { get; set; }                   // Business data
        public UserConfigurationResource? Configuration { get; set; }  // Business data
    }

    // Login request (auth only)
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

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

    // Application data updates only (for JWT endpoints)
    public class UserApplicationUpdate
    {
        public int? District { get; set; }
    }

    // Still needed for initial user creation (before JWT exists)
    public class UserCreateResource
    {
        // Identity data (will become JWT claims)
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }

        // Initial application data
        public int? District { get; set; }
        public string? SchoolYear { get; set; }
        public int PeriodsPerDay { get; set; } = 6; // Sensible default
    }

    // For updating users via admin endpoints (non-JWT)
    public class UserUpdateResource
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int? District { get; set; }
    }
}
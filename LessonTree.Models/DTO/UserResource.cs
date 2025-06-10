// **COMPLETE FILE** - User Identity and Profile Resources (JWT Transitional)
// RESPONSIBILITY: User identity, profile data, and account management
// DOES NOT: Handle authentication (see AuthenticationResource.cs) or configuration (see UserConfigurationResource.cs)
// CALLED BY: Controllers for user profile operations

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

    // Application data updates only (for JWT endpoints)
    public class UserApplicationUpdate
    {
        public int? District { get; set; }
    }
}
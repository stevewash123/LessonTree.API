// **COMPLETE FILE** - Authentication-related Resources
// RESPONSIBILITY: Login, logout, and authentication flow operations
// DOES NOT: Handle user profile data (see UserResource.cs) or configuration (see UserConfigurationResource.cs)
// CALLED BY: Controllers for authentication operations

namespace LessonTree.Models.DTO
{
    // Login request (auth only)
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // Future: Add other auth-related resources as needed
    // Examples that might be added later:
    // - LoginResponse (JWT token)
    // - RefreshTokenRequest
    // - PasswordResetRequest
    // - RegistrationRequest
}
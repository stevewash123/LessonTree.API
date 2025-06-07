// **COMPLETE FILE** - Base controller with JWT claim extraction
// RESPONSIBILITY: Extract user ID from JWT claims consistently across controllers
// DOES NOT: Handle auth validation (handled by [Authorize] attribute)
// CALLED BY: All controllers that need current user context

using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LessonTree.API.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        /// <summary>
        /// Extracts the current user ID from JWT 'sub' claim
        /// </summary>
        /// <returns>User ID from JWT, or throws if not found</returns>
        protected int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("User ID not found in JWT claims");
            }

            if (!int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid user ID in JWT claims");
            }

            return userId;
        }

        /// <summary>
        /// Safely extracts user ID, returns null if not found (for optional scenarios)
        /// </summary>
        protected int? GetCurrentUserIdOrNull()
        {
            try
            {
                return GetCurrentUserId();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets username from JWT claims
        /// </summary>
        protected string? GetCurrentUsername()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value
                ?? User.FindFirst("username")?.Value;
        }

        /// <summary>
        /// Checks if current user has specific role
        /// </summary>
        protected bool HasRole(string role)
        {
            return User.IsInRole(role);
        }
    }
}
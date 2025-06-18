// **COMPLETE FILE** - UserController for user profile management only
// RESPONSIBILITY: Current user profile endpoints with proper security
// DOES NOT: Allow access to other users' data (removed admin functions)
// CALLED BY: Angular frontend with JWT tokens for current user operations

using LessonTree.BLL.Service;
using LessonTree.Models.DTO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LessonTree.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserController : BaseController
    {
        private readonly IUserService _service;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserService service,
            ILogger<UserController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // GET /api/User/profile - Get current user's profile
        [HttpGet("profile")]
        public IActionResult GetCurrentUserProfile()
        {
            int userId = GetCurrentUserId();
            _logger.LogDebug("Fetching profile for current user ID: {UserId}", userId);

            var userResource = _service.GetUserResourceById(userId);
            if (userResource == null)
            {
                _logger.LogError("Current user with ID {UserId} not found", userId);
                return NotFound("User profile not found");
            }

            _logger.LogDebug("Returning profile for user: {UserName}", userResource.Username);
            return Ok(userResource);
        }

        // PUT /api/User/profile - Update current user's profile
        [HttpPut("profile")]
        public IActionResult UpdateCurrentUserProfile([FromBody] UserResource userResource)
        {
            int userId = GetCurrentUserId();
            _logger.LogDebug("Updating profile for current user ID: {UserId}", userId);

            // Ensure user can only update their own profile
            userResource.Id = userId;

            var updatedUserResource = _service.UpdateFromResource(userId, userResource);
            if (updatedUserResource == null)
            {
                _logger.LogError("Current user with ID {UserId} not found for update", userId);
                return NotFound("User profile not found");
            }

            _logger.LogInformation("Updated profile for user ID: {UserId}", userId);
            return Ok(updatedUserResource);
        }

        // DELETE /api/User/profile - Delete current user's account
        [HttpDelete("profile")]
        public IActionResult DeleteCurrentUserAccount()
        {
            int userId = GetCurrentUserId();
            _logger.LogDebug("Deleting account for current user ID: {UserId}", userId);

            bool deleted = _service.Delete(userId);
            if (!deleted)
            {
                _logger.LogError("Current user with ID {UserId} not found for deletion", userId);
                return NotFound("User account not found");
            }

            _logger.LogInformation("Deleted account for user ID: {UserId}", userId);
            return NoContent();
        }

        // GET /api/User/configuration - Get current user's configuration
        [HttpGet("configuration")]
        public IActionResult GetCurrentUserConfiguration()
        {
            int userId = GetCurrentUserId();
            _logger.LogDebug("Fetching configuration for current user ID: {UserId}", userId);

            try
            {
                var userConfig = _service.GetUserConfiguration(userId);
                if (userConfig == null)
                {
                    _logger.LogWarning("Configuration not found for current user ID {UserId}", userId);
                    return NotFound("User configuration not found");
                }

                _logger.LogDebug("Returning configuration for current user ID: {UserId}", userId);
                return Ok(userConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving configuration for current user ID: {UserId}", userId);
                return StatusCode(500, "Error retrieving user configuration");
            }
        }

        // PUT /api/User/configuration - Update current user's configuration
        [HttpPut("configuration")]
        public IActionResult UpdateCurrentUserConfiguration([FromBody] UserConfigurationUpdate configUpdate)
        {
            int userId = GetCurrentUserId();
            _logger.LogDebug("Updating configuration for current user ID: {UserId}", userId);

            if (configUpdate == null)
            {
                _logger.LogWarning("Configuration update request is null for current user ID: {UserId}", userId);
                return BadRequest("User configuration data is required");
            }

            try
            {
                var updatedConfig = _service.UpdateUserConfiguration(userId, configUpdate);
                if (updatedConfig == null)
                {
                    _logger.LogError("Current user with ID {UserId} not found for configuration update", userId);
                    return NotFound("User not found");
                }

                _logger.LogInformation("Updated configuration for current user ID: {UserId}", userId);
                return Ok(updatedConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating configuration for current user ID: {UserId}", userId);
                return StatusCode(500, "Error updating user configuration");
            }
        }

        // REMOVED METHODS (Security violations):
        // - GetUsers() - Admin function, should be in separate AdminController
        // - GetUser(int id) - Allows access to any user's profile
        // - UpdateUser(int id, UserResource) - Allows modification of any user
        // - DeleteUser(int id) - Allows deletion of any user
        // - GetUserConfiguration(int id) - Allows access to any user's config
        // - UpdateUserConfiguration(int id, ...) - Allows modification of any user's config
    }
}
// **COMPLETE FILE** - UserController with period assignment validation
// RESPONSIBILITY: User endpoints with enhanced configuration validation
// DOES NOT: Access UserConfigurationResource.UserId (doesn't exist)
// CALLED BY: Angular frontend with JWT tokens

using LessonTree.BLL.Service;
using LessonTree.Models.DTO;
using LessonTree.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LessonTree.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly IPeriodAssignmentValidationService _validationService;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserService service,
            IPeriodAssignmentValidationService validationService,
            ILogger<UserController> logger)
        {
            _service = service;
            _validationService = validationService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            _logger.LogDebug("Entering GetUsers");
            var userResources = _service.GetAllUserResources();
            _logger.LogDebug("Returning {Count} users", userResources.Count);
            return Ok(userResources);
        }

        [HttpGet("{id}")]
        public IActionResult GetUser(int id)
        {
            _logger.LogDebug("Entering GetUser with ID: {UserId}", id);
            var userResource = _service.GetUserResourceById(id);
            if (userResource == null)
            {
                _logger.LogError("User with ID {UserId} not found", id);
                return NotFound();
            }

            _logger.LogDebug("Returning user: {UserName}", userResource.Username);
            return Ok(userResource);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, [FromBody] UserResource userResource)
        {
            _logger.LogDebug("Entering UpdateUser with ID: {UserId}", id);
            if (id != userResource.Id)
            {
                _logger.LogWarning("ID mismatch: URL ID {UrlId} does not match body ID {BodyId}", id, userResource.Id);
                return BadRequest("ID mismatch");
            }

            var updatedUserResource = _service.UpdateFromResource(id, userResource);
            if (updatedUserResource == null)
            {
                _logger.LogError("User with ID {UserId} not found for update", id);
                return NotFound();
            }

            _logger.LogInformation("Updated user with ID: {UserId}", id);
            return Ok(updatedUserResource);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            _logger.LogDebug("Entering DeleteUser with ID: {UserId}", id);
            _service.Delete(id);
            _logger.LogInformation("Deleted user with ID: {UserId}", id);
            return NoContent();
        }

        [HttpGet("{id}/configuration")]
        public IActionResult GetUserConfiguration(int id)
        {
            _logger.LogDebug("Entering GetUserConfiguration for user ID: {UserId}", id);

            try
            {
                var userConfig = _service.GetUserConfiguration(id);
                if (userConfig == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found", id);
                    return NotFound("User not found");
                }

                _logger.LogDebug("Returning user configuration for user ID: {UserId}", id);
                return Ok(userConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user configuration for user ID: {UserId}", id);
                return StatusCode(500, "Error retrieving user configuration");
            }
        }

        [HttpPut("{id}/configuration")]
        public IActionResult UpdateUserConfiguration(int id, [FromBody] UserConfigurationUpdate configUpdate)
        {
            _logger.LogDebug("Entering UpdateUserConfiguration for user ID: {UserId}", id);

            if (configUpdate == null)
            {
                _logger.LogWarning("User configuration update request is null for user ID: {UserId}", id);
                return BadRequest("User configuration data is required");
            }

            // NEW: Validate period assignments before saving
            if (configUpdate.PeriodAssignments?.Any() == true)
            {
                var validationResult = _validationService.ValidatePeriodAssignments(
                    configUpdate.PeriodAssignments,
                    configUpdate.PeriodsPerDay);

                if (!validationResult.IsValid)
                {
                    _logger.LogWarning($"Period assignment validation failed for user {id}: {string.Join(", ", validationResult.Errors)}");

                    return BadRequest(new
                    {
                        message = "Period assignment validation failed",
                        errors = validationResult.Errors,
                        canGenerate = false
                    });
                }

                _logger.LogInformation($"Period assignment validation passed for user {id}");
            }

            try
            {
                var updatedConfig = _service.UpdateUserConfiguration(id, configUpdate);
                if (updatedConfig == null)
                {
                    _logger.LogError("User with ID {UserId} not found for configuration update", id);
                    return NotFound("User not found");
                }

                _logger.LogInformation("Updated user configuration for user ID: {UserId}", id);

                // Return success with validation status
                return Ok(new
                {
                    configuration = updatedConfig,
                    canGenerate = true,
                    validationMessage = "Configuration is valid and ready for schedule generation"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user configuration for user ID: {UserId}", id);
                return StatusCode(500, "Error updating user configuration");
            }
        }

        // NEW: Validation-only endpoint for real-time validation
        [HttpPost("{id}/configuration/validate")]
        public IActionResult ValidatePeriodAssignments(int id, [FromBody] UserConfigurationUpdate configUpdate)
        {
            _logger.LogDebug("Entering ValidatePeriodAssignments for user ID: {UserId}", id);

            if (configUpdate?.PeriodAssignments?.Any() != true)
            {
                return Ok(new { isValid = true, canGenerate = false, message = "No period assignments to validate" });
            }

            var validationResult = _validationService.ValidatePeriodAssignments(
                configUpdate.PeriodAssignments,
                configUpdate.PeriodsPerDay);

            return Ok(new
            {
                isValid = validationResult.IsValid,
                canGenerate = validationResult.IsValid,
                errors = validationResult.Errors,
                message = validationResult.IsValid
                    ? "Configuration is valid and ready for schedule generation"
                    : "Configuration has validation errors"
            });
        }
    }
}
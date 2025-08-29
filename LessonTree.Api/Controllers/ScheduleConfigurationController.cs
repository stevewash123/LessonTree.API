// **MODIFIED FILE** - Complete ScheduleConfigurationController.cs with Auto-Generation Endpoints
// INTEGRATION: Add these endpoints to existing ScheduleConfigurationController.cs

using LessonTree.Api.Controllers;
using LessonTree.API.Controllers;
using LessonTree.BLL.Services;
using LessonTree.Models.DTO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LessonTree.Api.Controllers
{
    /// <summary>
    /// Controller for schedule configuration CRUD operations with auto-generation
    /// Handles user schedule configuration templates and settings + automatic schedule generation
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleConfigurationController : BaseController
    {
        private readonly IScheduleConfigurationService _service;
        private readonly ILogger<ScheduleConfigurationController> _logger;
        private readonly IScheduleService _scheduleService;

        public ScheduleConfigurationController(
            IScheduleConfigurationService service,
            ILogger<ScheduleConfigurationController> logger,
            IScheduleService scheduleService)
        {
            _service = service;
            _logger = logger;
            _scheduleService = scheduleService;
        }

        // === EXISTING ENDPOINTS (unchanged) ===

        /// <summary>
        /// Get all schedule configurations for current user
        /// </summary>
        /// <returns>List of schedule configuration resources</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                int userId = GetCurrentUserId();
                var configurations = await _service.GetAllAsync(userId);
                return Ok(configurations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedule configurations");
                return StatusCode(500, new { status = "error", message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get schedule configuration summaries for current user
        /// </summary>
        /// <returns>List of schedule configuration summary resources</returns>
        [HttpGet("summaries")]
        public async Task<IActionResult> GetSummaries()
        {
            try
            {
                int userId = GetCurrentUserId();
                var summaries = await _service.GetSummariesAsync(userId);
                return Ok(summaries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedule configuration summaries");
                return StatusCode(500, new { status = "error", message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get schedule configuration by ID
        /// </summary>
        /// <param name="id">Schedule configuration ID</param>
        /// <returns>Schedule configuration resource</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                int userId = GetCurrentUserId();
                var configuration = await _service.GetByIdAsync(id, userId);

                if (configuration == null)
                {
                    return NotFound(new { status = "error", message = "Schedule configuration not found" });
                }

                return Ok(configuration);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedule configuration {ConfigId}", id);
                return StatusCode(500, new { status = "error", message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get current user's active schedule configuration
        /// </summary>
        /// <returns>Active schedule configuration resource</returns>
        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            try
            {
                int userId = GetCurrentUserId();
                var configuration = await _service.GetActiveAsync(userId);

                if (configuration == null)
                {
                    return NotFound(new { status = "error", message = "No active schedule configuration found" });
                }

                return Ok(configuration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active schedule configuration");
                return StatusCode(500, new { status = "error", message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get schedule configuration by school year
        /// </summary>
        /// <param name="schoolYear">School year to search for</param>
        /// <returns>Schedule configuration resource</returns>
        [HttpGet("schoolYear/{schoolYear}")]
        public async Task<IActionResult> GetBySchoolYear(string schoolYear)
        {
            try
            {
                int userId = GetCurrentUserId();
                var configuration = await _service.GetBySchoolYearAsync(userId, schoolYear);

                if (configuration == null)
                {
                    return NotFound(new { status = "error", message = $"No configuration found for school year {schoolYear}" });
                }

                return Ok(configuration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedule configuration for school year {SchoolYear}", schoolYear);
                return StatusCode(500, new { status = "error", message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get template schedule configurations for current user
        /// </summary>
        /// <returns>List of template schedule configuration resources</returns>
        [HttpGet("templates")]
        public async Task<IActionResult> GetTemplates()
        {
            try
            {
                int userId = GetCurrentUserId();
                var templates = await _service.GetTemplatesAsync(userId);
                return Ok(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedule configuration templates");
                return StatusCode(500, new { status = "error", message = "Internal server error" });
            }
        }

        /// <summary>
        /// Create new schedule configuration
        /// </summary>
        /// <param name="resource">Schedule configuration creation data</param>
        /// <returns>Created schedule configuration resource</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ScheduleConfigurationCreateResource resource)
        {
            try
            {
                int userId = GetCurrentUserId();

                // Create configuration
                var configuration = await _service.CreateAsync(resource, userId);

                // Automatically generate schedule
                var schedule = await _scheduleService.CreateScheduleFromConfigurationAsync(configuration.Id, userId);

                return Ok(new
                {
                    configuration = configuration,
                    schedule = schedule,
                    status = "success",
                    message = $"Configuration and schedule created successfully with {schedule.ScheduleEvents.Count} events"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { status = "error", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating configuration with schedule");
                return StatusCode(500, new { status = "error", message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update schedule configuration
        /// </summary>
        /// <param name="id">Schedule configuration ID</param>
        /// <param name="resource">Schedule configuration update data</param>
        /// <returns>Updated schedule configuration resource</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ScheduleConfigurationUpdateResource resource)
        {
            try
            {
                int userId = GetCurrentUserId();
                var configuration = await _service.UpdateAsync(id, resource, userId);
                return Ok(configuration);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { status = "error", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating schedule configuration {ConfigId}", id);
                return StatusCode(500, new { status = "error", message = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete schedule configuration
        /// </summary>
        /// <param name="id">Schedule configuration ID</param>
        /// <returns>Success response</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                int userId = GetCurrentUserId();
                await _service.DeleteAsync(id, userId);
                return Ok(new { status = "success", message = "Schedule configuration deleted successfully" });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { status = "error", message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { status = "error", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting schedule configuration {ConfigId}", id);
                return StatusCode(500, new { status = "error", message = "Internal server error" });
            }
        }

        /// <summary>
        /// Set schedule configuration as active
        /// </summary>
        /// <param name="id">Schedule configuration ID</param>
        /// <returns>Updated schedule configuration resource</returns>
        [HttpPost("{id}/activate")]
        public async Task<IActionResult> SetActive(int id)
        {
            try
            {
                int userId = GetCurrentUserId();
                var configuration = await _service.SetActiveAsync(id, userId);
                return Ok(configuration);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { status = "error", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating schedule configuration {ConfigId}", id);
                return StatusCode(500, new { status = "error", message = "Internal server error" });
            }
        }

        /// <summary>
        /// Copy schedule configuration as template
        /// </summary>
        /// <param name="id">Source schedule configuration ID</param>
        /// <param name="request">Copy configuration request</param>
        /// <returns>New template schedule configuration resource</returns>
        [HttpPost("{id}/copy")]
        public async Task<IActionResult> CopyAsTemplate(int id, [FromBody] CopyConfigurationRequest request)
        {
            try
            {
                int userId = GetCurrentUserId();
                var configuration = await _service.CopyAsTemplateAsync(id, request, userId);
                return Ok(configuration);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { status = "error", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error copying schedule configuration {ConfigId} as template", id);
                return StatusCode(500, new { status = "error", message = "Internal server error" });
            }
        }

        /// <summary>
        /// Validate schedule configuration
        /// </summary>
        /// <param name="id">Schedule configuration ID</param>
        /// <returns>Validation result resource</returns>
        [HttpGet("{id}/validate")]
        public async Task<IActionResult> Validate(int id)
        {
            try
            {
                int userId = GetCurrentUserId();
                var validation = await _service.ValidateAsync(id, userId);
                return Ok(validation);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { status = "error", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating schedule configuration {ConfigId}", id);
                return StatusCode(500, new { status = "error", message = "Internal server error" });
            }
        }

        // === NEW AUTO-GENERATION ENDPOINTS ===

        /// <summary>
        /// Update configuration and automatically regenerate schedule
        /// </summary>
        /// <param name="id">Configuration ID</param>
        /// <param name="resource">Configuration update data</param>
        /// <returns>Updated configuration with regenerated schedule</returns>
        [HttpPut("{id}/withSchedule")]
        public async Task<IActionResult> UpdateWithSchedule(int id, [FromBody] ScheduleConfigurationUpdateResource resource)
        {
            try
            {
                int userId = GetCurrentUserId();

                // Update configuration
                var configuration = await _service.UpdateAsync(id, resource, userId);

                // Automatically regenerate schedule
                var schedule = await _scheduleService.RegenerateScheduleFromConfigurationAsync(id, userId);

                return Ok(new
                {
                    configuration = configuration,
                    schedule = schedule,
                    status = "success",
                    message = $"Configuration updated and schedule regenerated with {schedule.ScheduleEvents.Count} events"
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { status = "error", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating configuration {ConfigId} with schedule", id);
                return StatusCode(500, new { status = "error", message = "Internal server error" });
            }
        }

        /// <summary>
        /// Generate schedule from existing configuration
        /// </summary>
        /// <param name="id">Configuration ID</param>
        /// <returns>Generated schedule</returns>
        [HttpPost("{id}/generateSchedule")]
        public async Task<IActionResult> GenerateSchedule(int id)
        {
            try
            {
                int userId = GetCurrentUserId();

                var schedule = await _scheduleService.CreateScheduleFromConfigurationAsync(id, userId);

                return Ok(new
                {
                    schedule = schedule,
                    status = "success",
                    message = $"Schedule generated successfully with {schedule.ScheduleEvents.Count} events"
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { status = "error", message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { status = "error", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating schedule from configuration {ConfigId}", id);
                return StatusCode(500, new { status = "error", message = "Internal server error" });
            }
        }

        /// <summary>
        /// Validate configuration for schedule generation
        /// </summary>
        /// <param name="id">Configuration ID</param>
        /// <returns>Detailed validation result</returns>
        [HttpGet("{id}/validateGeneration")]
        public async Task<IActionResult> ValidateGeneration(int id)
        {
            try
            {
                int userId = GetCurrentUserId();

                var validation = await _scheduleService.ValidateConfigurationAsync(id, userId);

                return Ok(validation);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { status = "error", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating configuration {ConfigId} for generation", id);
                return StatusCode(500, new { status = "error", message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get generation preview for configuration
        /// </summary>
        /// <param name="id">Configuration ID</param>
        /// <returns>Generation preview with estimated counts</returns>
        [HttpGet("{id}/generationPreview")]
        public async Task<IActionResult> GetGenerationPreview(int id)
        {
            try
            {
                int userId = GetCurrentUserId();

                var preview = await _scheduleService.GetGenerationPreviewAsync(id, userId);

                return Ok(preview);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { status = "error", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting generation preview for configuration {ConfigId}", id);
                return StatusCode(500, new { status = "error", message = "Internal server error" });
            }
        }
    }
}
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LessonTree.BLL.Service;
using LessonTree.Models.DTO;

namespace LessonTree.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class StandardController : ControllerBase
    {
        private readonly IStandardService _service;
        private readonly ILogger<StandardController> _logger;

        public StandardController(IStandardService service, ILogger<StandardController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetStandards()
        {
            _logger.LogDebug("Fetching all standards in controller");
            try
            {
                var standards = await _service.GetAllAsync();
                _logger.LogInformation("Successfully fetched {Count} standards", standards.Count);
                return Ok(standards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch all standards");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { status = "error", message = "An unexpected error occurred while retrieving standards." });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStandard(int id)
        {
            _logger.LogDebug("Fetching standard by ID: {StandardId} in controller", id);
            try
            {
                var standard = await _service.GetByIdAsync(id);
                if (standard == null)
                {
                    _logger.LogWarning("Standard with ID {StandardId} not found", id);
                    return NotFound(new { status = "error", message = $"Standard with ID {id} not found" });
                }
                _logger.LogInformation("Successfully fetched standard with ID: {StandardId}", id);
                return Ok(standard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch standard with ID: {StandardId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { status = "error", message = "An unexpected error occurred while retrieving the standard." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddStandard([FromBody] StandardCreateResource standardCreateResource)
        {
            _logger.LogDebug("Adding standard: {Title} in controller", standardCreateResource.Title);
            try
            {
                var createdId = await _service.AddAsync(standardCreateResource);
                var createdStandard = await _service.GetByIdAsync(createdId);
                _logger.LogInformation("Successfully added standard with ID: {StandardId}", createdId);
                return CreatedAtAction(nameof(GetStandard), new { id = createdId }, createdStandard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add standard: {Title}", standardCreateResource.Title);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { status = "error", message = "An unexpected error occurred while adding the standard." });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStandard(int id, [FromBody] StandardUpdateResource standardUpdateResource)
        {
            _logger.LogDebug("Updating standard with ID: {StandardId} in controller", id);
            if (id != standardUpdateResource.Id)
            {
                _logger.LogWarning("ID mismatch: Route ID {RouteId} does not match body ID {BodyId}", id, standardUpdateResource.Id);
                return BadRequest(new { status = "error", message = "ID in URL must match ID in body" });
            }
            try
            {
                var updatedStandard = await _service.UpdateAsync(standardUpdateResource);
                _logger.LogInformation("Successfully updated standard with ID: {StandardId}", id);
                return Ok(updatedStandard);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Standard with ID {StandardId} not found for update", id);
                return NotFound(new { status = "error", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update standard with ID: {StandardId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { status = "error", message = "An unexpected error occurred while updating the standard." });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStandard(int id)
        {
            _logger.LogDebug("Deleting standard with ID: {StandardId} in controller", id);
            try
            {
                await _service.DeleteAsync(id);
                _logger.LogInformation("Successfully deleted standard with ID: {StandardId}", id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Standard with ID {StandardId} not found for deletion", id);
                return NotFound(new { status = "error", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete standard with ID: {StandardId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { status = "error", message = "An unexpected error occurred while deleting the standard." });
            }
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetStandardsByCourseId(int courseId, [FromQuery] int? districtId = null)
        {
            _logger.LogDebug("Fetching standards by Course ID: {CourseId}, District ID: {DistrictId} in controller", courseId, districtId);
            try
            {
                var standards = await _service.GetByCourseIdAsync(courseId, districtId);
                _logger.LogInformation("Successfully fetched {Count} standards for Course ID: {CourseId}", standards.Count, courseId);
                return Ok(standards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch standards for Course ID: {CourseId}", courseId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { status = "error", message = "An unexpected error occurred while retrieving standards by course." });
            }
        }
    }
}
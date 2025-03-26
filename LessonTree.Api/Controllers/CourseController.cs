// Full File
using LessonTree.BLL.Service;
using LessonTree.Models.DTO;
using LessonTree.Models.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace LessonTree.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _service;
        private readonly ILogger<CourseController> _logger;

        public CourseController(ICourseService service, ILogger<CourseController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogError("Failed to extract UserId from JWT claims");
                throw new UnauthorizedAccessException("User ID not found in token");
            }
            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> GetCourses(ArchiveFilter filter = ArchiveFilter.Active)
        {
            int userId = GetCurrentUserId();
            _logger.LogDebug("Fetching courses for User ID: {UserId}, Filter: {Filter}", userId, filter);
            var courses = await _service.GetAllAsync(userId, filter);
            return Ok(courses);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourse(int id)
        {
            int userId = GetCurrentUserId();
            _logger.LogDebug("Fetching course with ID: {CourseId} for User ID: {UserId}", id, userId);
            var course = await _service.GetByIdAsync(id, userId);
            if (course == null)
            {
                _logger.LogWarning("Course with ID {Id} not found for User ID {UserId}", id, userId);
                return NotFound();
            }
            return Ok(course);
        }

        [HttpPost]
        public async Task<IActionResult> AddCourse([FromBody] CourseCreateResource courseCreateResource)
        {
            int userId = GetCurrentUserId();
            _logger.LogDebug("Adding new course: {Title} for User ID: {UserId}", courseCreateResource.Title, userId);
            await _service.AddAsync(courseCreateResource, userId);
            var createdCourse = await _service.GetByIdAsync((await _service.GetAllAsync(userId)).Last().Id, userId);
            _logger.LogInformation("Added course with ID: {CourseId}, Title: {Title}", createdCourse.Id, createdCourse.Title);
            return CreatedAtAction(nameof(GetCourse), new { id = createdCourse.Id }, createdCourse);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] CourseUpdateResource courseUpdateResource)
        {
            int userId = GetCurrentUserId();
            _logger.LogInformation("Updating course with ID {Id} for User ID {UserId}, DTO: {@CourseUpdateResource}", id, userId, courseUpdateResource);
            if (id != courseUpdateResource.Id)
            {
                _logger.LogWarning("ID mismatch: Route ID {RouteId} does not match DTO ID {DtoId}", id, courseUpdateResource.Id);
                return BadRequest(new ProblemDetails { Title = "ID mismatch", Detail = "Route ID must match DTO ID" });
            }
            await _service.UpdateAsync(courseUpdateResource, userId);
            _logger.LogInformation("Course with ID {Id} updated successfully", id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            int userId = GetCurrentUserId();
            _logger.LogDebug("Deleting course with ID: {CourseId} for User ID: {UserId}", id, userId);
            await _service.DeleteAsync(id, userId);
            _logger.LogInformation("Deleted course with ID: {CourseId}", id);
            return NoContent();
        }
    }
}
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
    public class CourseController : BaseController
    {
        private readonly ICourseService _service;
        private readonly ILogger<CourseController> _logger;

        public CourseController(ICourseService service, ILogger<CourseController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetCourses(
            [FromQuery] ArchiveFilter filter = ArchiveFilter.Active,
            [FromQuery] int? visibility = null) // Add visibility parameter
        {
            int userId = GetCurrentUserId();
            _logger.LogDebug("Fetching courses for User ID: {UserId}, Filter: {Filter}, Visibility: {Visibility}", userId, filter, visibility);
            var courses = await _service.GetAllAsync(userId, filter, visibility);
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

            // FIX: Return the updated course instead of NoContent
            var updatedCourse = await _service.GetByIdAsync(id, userId);
            _logger.LogInformation("Course with ID {Id} updated successfully", id);
            return Ok(updatedCourse);
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
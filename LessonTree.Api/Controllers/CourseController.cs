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
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _service;
        private readonly ILogger<CourseController> _logger;

        public CourseController(ICourseService service, ILogger<CourseController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetCourses()
        {
            var courses = await _service.GetAllAsync();
            return Ok(courses);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourse(int id)
        {
            var course = await _service.GetByIdAsync(id);
            if (course == null)
            {
                _logger.LogWarning("Course with id {Id} not found", id);
                return NotFound();
            }
            return Ok(course);
        }

        [HttpPost]
        public async Task<IActionResult> AddCourse([FromBody] CourseCreateResource courseCreateResource)
        {
            _logger.LogDebug("Adding new course: {Title}", courseCreateResource.Title);
            await _service.AddAsync(courseCreateResource);
            var createdCourse = await _service.GetByIdAsync((await _service.GetAllAsync()).Last().Id); // Adjusted for async
            _logger.LogInformation("Added course with ID: {CourseId}, Title: {Title}", createdCourse.Id, createdCourse.Title);
            return CreatedAtAction(nameof(GetCourse), new { id = createdCourse.Id }, createdCourse);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] CourseUpdateResource courseUpdateResource)
        {
            _logger.LogInformation("Updating course with id {Id}, DTO: {@CourseUpdateResource}", id, courseUpdateResource);
            if (id != courseUpdateResource.Id)
            {
                _logger.LogWarning("ID mismatch: Route id {RouteId} does not match DTO id {DtoId}", id, courseUpdateResource.Id);
                return BadRequest(new ProblemDetails { Title = "ID mismatch", Detail = "Route ID must match DTO ID" });
            }
            await _service.UpdateAsync(courseUpdateResource);
            _logger.LogInformation("Course with id {Id} updated successfully", id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            _logger.LogDebug("Deleting course with ID: {CourseId}", id);
            await _service.DeleteAsync(id);
            _logger.LogInformation("Deleted course with ID: {CourseId}", id);
            return NoContent();
        }
    }
}
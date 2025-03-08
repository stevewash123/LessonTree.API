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
        public IActionResult GetCourses()
        {
            var courses = _service.GetAll();
            return Ok(courses);
        }

        [HttpGet("{id}")]
        public IActionResult GetCourse(int id)
        {
            var course = _service.GetById(id);
            if (course == null)
            {
                _logger.LogWarning("Course with id {Id} not found", id);
                return NotFound();
            }
            return Ok(course);
        }

        [HttpPost]
        public IActionResult AddCourse([FromBody] CourseCreateResource courseCreateResource)
        {
            _logger.LogDebug("Adding new course: {Title}", courseCreateResource.Title);
            _service.Add(courseCreateResource);
            var createdCourse = _service.GetById(_service.GetAll().Last().Id); // Assuming GetAll returns in order of creation
            _logger.LogInformation("Added course with ID: {CourseId}, Title: {Title}", createdCourse.Id, createdCourse.Title);
            return CreatedAtAction(nameof(GetCourse), new { id = createdCourse.Id }, createdCourse);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateCourse(int id, [FromBody] CourseUpdateResource courseUpdateResource)
        {
            _logger.LogInformation("Updating course with id {Id}, DTO: {@CourseUpdateResource}", id, courseUpdateResource);
            if (id != courseUpdateResource.Id)
            {
                _logger.LogWarning("ID mismatch: Route id {RouteId} does not match DTO id {DtoId}", id, courseUpdateResource.Id);
                return BadRequest(new ProblemDetails { Title = "ID mismatch", Detail = "Route ID must match DTO ID" });
            }
            _service.Update(courseUpdateResource);
            _logger.LogInformation("Course with id {Id} updated successfully", id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteCourse(int id)
        {
            _logger.LogDebug("Deleting course with ID: {CourseId}", id);
            _service.Delete(id);
            _logger.LogInformation("Deleted course with ID: {CourseId}", id);
            return NoContent();
        }
    }
}
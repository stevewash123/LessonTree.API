using LessonTree.BLL.Service;
using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LessonTree.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TopicController : ControllerBase
    {
        private readonly ITopicService _service;
        private readonly ILogger<LessonController> _logger;

        public TopicController(ITopicService service, ILogger<LessonController> logger)
        {
            _service = service;
            _logger = logger;   
        }

        [HttpGet]
        public IActionResult GetTopics()
        {
            var topics = _service.GetAll();
            return Ok(topics);
        }

        [HttpGet("{id}")]
        public IActionResult GetTopic(int id)
        {
            var topic = _service.GetById(id);
            if (topic == null) return NotFound();
            return Ok(topic);
        }

        [HttpPost]
        public IActionResult AddTopic([FromBody] TopicCreateResource topicCreateResource)
        {
            _service.Add(topicCreateResource);
            var createdTopic = _service.GetById(_service.GetAll().Last().Id); // Assuming GetAll returns in order of creation
            return CreatedAtAction(nameof(GetTopic), new { id = createdTopic.Id }, createdTopic);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateTopic(int id, [FromBody] TopicUpdateResource topicUpdateResource)
        {
            if (id != topicUpdateResource.Id) return BadRequest();
            _service.Update(topicUpdateResource);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteTopic(int id)
        {
            _service.Delete(id);
            return NoContent();
        }
        [HttpPost("move")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult MoveTopic([FromBody] TopicMoveResource moveResource)
        {
            _logger.LogDebug("Entering MoveTopic with Topic ID: {TopicId}, New Course ID: {NewCourseId}",
                moveResource.TopicId, moveResource.NewCourseId);

            try
            {
                _service.MoveTopic(moveResource.TopicId, moveResource.NewCourseId);
                _logger.LogInformation("Moved Topic ID: {TopicId} to Course ID: {NewCourseId}",
                    moveResource.TopicId, moveResource.NewCourseId);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("Error moving topic: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error moving Topic ID: {TopicId} to Course ID: {NewCourseId}",
                    moveResource.TopicId, moveResource.NewCourseId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("copy")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult CopyTopic([FromBody] TopicMoveResource copyResource)
        {
            _logger.LogDebug("Entering CopyTopic with Topic ID: {TopicId}, New Course ID: {NewCourseId}",
                copyResource.TopicId, copyResource.NewCourseId);

            try
            {
                var newTopic = _service.CopyTopic(copyResource.TopicId, copyResource.NewCourseId);
                _logger.LogInformation("Copied Topic ID: {TopicId} to new Topic ID: {NewTopicId} under Course ID: {NewCourseId}",
                    copyResource.TopicId, newTopic.Id, copyResource.NewCourseId);
                return CreatedAtAction(nameof(GetTopic), new { id = newTopic.Id }, newTopic);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("Error copying topic: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error copying Topic ID: {TopicId} to Course ID: {NewCourseId}",
                    copyResource.TopicId, copyResource.NewCourseId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
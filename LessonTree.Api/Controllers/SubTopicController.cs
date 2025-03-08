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
    public class SubTopicController : ControllerBase
    {
        private readonly ISubTopicService _service;
        private readonly ILogger<SubTopicService> _logger;

        public SubTopicController(ISubTopicService service, ILogger<SubTopicService> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetSubTopics()
        {
            var subTopics = _service.GetAll();
            return Ok(subTopics);
        }

        [HttpGet("{id}")]
        public IActionResult GetSubTopic(int id)
        {
            var subTopic = _service.GetById(id);
            if (subTopic == null) return NotFound();
            return Ok(subTopic);
        }

        [HttpPost]
        public IActionResult AddSubTopic([FromBody] SubTopicCreateResource subTopicCreateResource)
        {
            _service.Add(subTopicCreateResource);
            var createdSubTopic = _service.GetById(_service.GetAll().Last().Id); // Assuming GetAll returns in order of creation
            return CreatedAtAction(nameof(GetSubTopic), new { id = createdSubTopic.Id }, createdSubTopic);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateSubTopic(int id, [FromBody] SubTopicUpdateResource subTopicUpdateResource)
        {
            if (id != subTopicUpdateResource.Id) return BadRequest();
            _service.Update(subTopicUpdateResource);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteSubTopic(int id)
        {
            _service.Delete(id);
            return NoContent();
        }

        [HttpPost("move")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult MoveSubTopic([FromBody] SubTopicMoveResource moveResource)
        {
            _logger.LogDebug("Entering MoveSubTopic with SubTopic ID: {SubTopicId}, New Topic ID: {NewTopicId}",
                moveResource.SubTopicId, moveResource.NewTopicId);

            try
            {
                _service.MoveSubTopic(moveResource.SubTopicId, moveResource.NewTopicId);
                _logger.LogInformation("Moved SubTopic ID: {SubTopicId} to Topic ID: {NewTopicId}",
                    moveResource.SubTopicId, moveResource.NewTopicId);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("Error moving subtopic: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error moving SubTopic ID: {SubTopicId} to Topic ID: {NewTopicId}",
                    moveResource.SubTopicId, moveResource.NewTopicId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("copy")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult CopySubTopic([FromBody] SubTopicMoveResource copyResource)
        {
            _logger.LogDebug("Entering CopySubTopic with SubTopic ID: {SubTopicId}, New Topic ID: {NewTopicId}",
                copyResource.SubTopicId, copyResource.NewTopicId);

            try
            {
                var newSubTopic = _service.CopySubTopic(copyResource.SubTopicId, copyResource.NewTopicId);
                _logger.LogInformation("Copied SubTopic ID: {SubTopicId} to new SubTopic ID: {NewSubTopicId} under Topic ID: {NewTopicId}",
                    copyResource.SubTopicId, newSubTopic.Id, copyResource.NewTopicId);
                return CreatedAtAction(nameof(GetSubTopic), new { id = newSubTopic.Id }, newSubTopic);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("Error copying subtopic: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error copying SubTopic ID: {SubTopicId} to Topic ID: {NewTopicId}",
                    copyResource.SubTopicId, copyResource.NewTopicId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSubTopic(int id)
        {
            try
            {
                var subTopic = await _service.GetByIdAsync(id);
                return Ok(subTopic);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSubTopics()
        {
            var subTopics = await _service.GetAllAsync();
            return Ok(subTopics);
        }

        [HttpPost]
        public async Task<IActionResult> AddSubTopic([FromBody] SubTopicCreateResource subTopicCreateResource)
        {
            var createdId = await _service.AddAsync(subTopicCreateResource);
            var createdSubTopic = await _service.GetByIdAsync(createdId);
            return CreatedAtAction(nameof(GetSubTopic), new { id = createdSubTopic.Id }, createdSubTopic);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSubTopic(int id, [FromBody] SubTopicUpdateResource subTopicUpdateResource)
        {
            if (id != subTopicUpdateResource.Id) return BadRequest();
            await _service.UpdateAsync(subTopicUpdateResource);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubTopic(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("move")]
        public async Task<IActionResult> MoveSubTopic([FromBody] SubTopicMoveResource moveResource)
        {
            _logger.LogDebug("Entering MoveSubTopic with SubTopic ID: {SubTopicId}, New Topic ID: {NewTopicId}",
                moveResource.SubTopicId, moveResource.NewTopicId);

            try
            {
                await _service.MoveSubTopic(moveResource.SubTopicId, moveResource.NewTopicId);
                _logger.LogInformation("Moved SubTopic ID: {SubTopicId} to Topic ID: {NewTopicId}",
                    moveResource.SubTopicId, moveResource.NewTopicId);
                return Ok(new { status = "success", message = "SubTopic moved successfully" });
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
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("copy")]
        public async Task<IActionResult> CopySubTopic([FromBody] SubTopicMoveResource copyResource)
        {
            _logger.LogDebug("Entering CopySubTopic with SubTopic ID: {SubTopicId}, New Topic ID: {NewTopicId}",
                copyResource.SubTopicId, copyResource.NewTopicId);

            try
            {
                var newSubTopic = await _service.CopySubTopicAsync(copyResource.SubTopicId, copyResource.NewTopicId);
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
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }
    }
}
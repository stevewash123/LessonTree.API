using LessonTree.BLL.Service;
using LessonTree.Models.DTO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class TopicController : ControllerBase
{
    private readonly ITopicService _service;
    private readonly ILogger<TopicController> _logger; // Fixed logger type

    public TopicController(ITopicService service, ILogger<TopicController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetTopics()
    {
        var topics = await _service.GetAllAsync();
        return Ok(topics);
    }

    [HttpGet("byCourse/{courseId}")]
    public async Task<IActionResult> GetTopicsByCourseId(int courseId)
    {
        var topics = await _service.GetTopicsByCourseAsync(courseId);
        return Ok(topics);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTopic(int id)
    {
        try
        {
            _logger.LogDebug("Fetching topic by ID: {TopicId}", id);
            var topic = await _service.GetByIdAsync(id);
            if (topic == null)
            {
                _logger.LogInformation("Topic with ID {TopicId} not found, returning 404", id);
                return NotFound();
            }

            _logger.LogDebug("Returning topic with ID {TopicId}", id);
            return Ok(topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching topic with ID {TopicId}: {Message}", id, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new { status = "error", message = "An unexpected error occurred while retrieving the topic." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddTopic([FromBody] TopicCreateResource topicCreateResource)
    {
        var createdTopicId = await _service.AddAsync(topicCreateResource);
        var createdTopic = await _service.GetByIdAsync(createdTopicId);
        return CreatedAtAction(nameof(GetTopic), new { id = createdTopic.Id }, createdTopic);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTopic(int id, [FromBody] TopicUpdateResource topicUpdateResource)
    {
        if (id != topicUpdateResource.Id) return BadRequest();
        await _service.UpdateAsync(topicUpdateResource);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTopic(int id)
    {
        try
        {
            _logger.LogDebug("Deleting topic with ID: {TopicId}", id);
            await _service.DeleteAsync(id);
            _logger.LogInformation("Topic deleted with ID: {TopicId}", id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Failed to delete topic with ID {TopicId}: {Message}", id, ex.Message);
            return NotFound(new { status = "error", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting topic with ID {TopicId}: {Message}", id, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new { status = "error", message = "An unexpected error occurred while deleting the topic." });
        }
    }

    [HttpPost("move")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> MoveTopic([FromBody] TopicMoveResource moveResource)
    {
        _logger.LogDebug("Entering MoveTopic with Topic ID: {TopicId}, New Course ID: {NewCourseId}",
            moveResource.TopicId, moveResource.NewCourseId);

        try
        {
            await _service.MoveTopicAsync(moveResource.TopicId, moveResource.NewCourseId);
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
    public async Task<IActionResult> CopyTopic([FromBody] TopicMoveResource copyResource)
    {
        _logger.LogDebug("Entering CopyTopic with Topic ID: {TopicId}, New Course ID: {NewCourseId}",
            copyResource.TopicId, copyResource.NewCourseId);

        try
        {
            var newTopic = await _service.CopyTopicAsync(copyResource.TopicId, copyResource.NewCourseId);
            _logger.LogInformation("Copied Topic ID: {TopicId} to new Topic ID: {NewTopicId} under Course ID: {NewCourseId}",
                copyResource.TopicId, newTopic.Id, copyResource.NewCourseId);
            var response =  CreatedAtAction(nameof(GetTopic), new { id = newTopic.Id }, newTopic);
            return response;
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
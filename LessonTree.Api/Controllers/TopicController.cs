// RESPONSIBILITY: Handles HTTP requests for Topic CRUD operations
// DOES NOT: Handle business logic or data access directly
// CALLED BY: Angular UI via HTTP requests

using LessonTree.BLL.Service;
using LessonTree.Models.DTO;
using LessonTree.Models.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class TopicController : ControllerBase
{
    private readonly ITopicService _service;
    private readonly ILogger<TopicController> _logger;

    public TopicController(ITopicService service, ILogger<TopicController> logger)
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
    public async Task<IActionResult> GetTopics(ArchiveFilter filter = ArchiveFilter.Active)
    {
        int userId = GetCurrentUserId();
        _logger.LogDebug("Fetching topics for User ID: {UserId}, Filter: {Filter}", userId, filter);
        var topics = await _service.GetAllAsync(userId, filter);
        return Ok(topics);
    }

    [HttpGet("byCourse/{courseId}")]
    public async Task<IActionResult> GetTopicsByCourseId(int courseId, ArchiveFilter filter = ArchiveFilter.Active)
    {
        int userId = GetCurrentUserId();
        _logger.LogDebug("Fetching topics for Course ID: {CourseId}, User ID: {UserId}, Filter: {Filter}", courseId, userId, filter);
        var topics = await _service.GetTopicsByCourseAsync(courseId, userId, filter);
        return Ok(topics);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTopic(int id)
    {
        try
        {
            int userId = GetCurrentUserId();
            _logger.LogDebug("Fetching topic by ID: {TopicId} for User ID: {UserId}", id, userId);
            var topic = await _service.GetByIdAsync(id, userId);
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
        int userId = GetCurrentUserId();
        _logger.LogDebug("Adding topic: {Title} for User ID: {UserId}", topicCreateResource.Title, userId);
        var createdTopicId = await _service.AddAsync(topicCreateResource, userId);
        var createdTopic = await _service.GetByIdAsync(createdTopicId, userId);
        _logger.LogInformation("Added topic with ID: {TopicId}", createdTopic.Id);
        return CreatedAtAction(nameof(GetTopic), new { id = createdTopic.Id }, createdTopic);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTopic(int id, [FromBody] TopicUpdateResource topicUpdateResource)
    {
        if (id != topicUpdateResource.Id) return BadRequest();

        int userId = GetCurrentUserId();
        _logger.LogDebug("Updating topic with ID: {TopicId} for User ID: {UserId}", id, userId);

        try
        {
            var updatedTopic = await _service.UpdateAsync(topicUpdateResource, userId);
            _logger.LogInformation("Updated topic with ID: {TopicId}", id);
            return Ok(updatedTopic);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Topic with ID {TopicId} not found for update", id);
            return NotFound(new { status = "error", message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "User ID {UserId} attempted to update topic ID {TopicId} owned by another user", userId, id);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating topic with ID {TopicId}: {Message}", id, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new { status = "error", message = "An unexpected error occurred while updating the topic." });
        }
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
    public async Task<IActionResult> MoveTopic([FromBody] TopicMoveResource moveResource)
    {
        int userId = GetCurrentUserId();
        _logger.LogDebug("Moving Topic ID: {TopicId} to Course ID: {NewCourseId} for User ID: {UserId}",
            moveResource.TopicId, moveResource.NewCourseId, userId);
        await _service.MoveTopicAsync(moveResource.TopicId, moveResource.NewCourseId, userId);
        _logger.LogInformation("Moved Topic ID: {TopicId} to Course ID: {NewCourseId}",
            moveResource.TopicId, moveResource.NewCourseId);
        return Ok();
    }

    [HttpPost("copy")]
    public async Task<IActionResult> CopyTopic([FromBody] TopicMoveResource copyResource)
    {
        int userId = GetCurrentUserId();
        _logger.LogDebug("Copying Topic ID: {TopicId} to Course ID: {NewCourseId} for User ID: {UserId}",
            copyResource.TopicId, copyResource.NewCourseId, userId);
        var newTopic = await _service.CopyTopicAsync(copyResource.TopicId, copyResource.NewCourseId, userId);
        _logger.LogInformation("Copied Topic ID: {TopicId} to new Topic ID: {NewTopicId} under Course ID: {NewCourseId}",
            copyResource.TopicId, newTopic.Id, copyResource.NewCourseId);
        return CreatedAtAction(nameof(GetTopic), new { id = newTopic.Id }, newTopic);
    }

    // Add SortOrder endpoint
    [HttpPut("{topicId}/sortOrder")]
    public async Task<IActionResult> UpdateTopicSortOrder(int topicId, [FromBody] int sortOrder)
    {
        int userId = GetCurrentUserId();
        _logger.LogDebug("Updating sort order for Topic ID: {TopicId} to {SortOrder} for User ID: {UserId}", topicId, sortOrder, userId);

        var topic = await _service.GetDomainTopicByIdAsync(topicId);
        if (topic == null)
        {
            _logger.LogError("Topic with ID {TopicId} not found", topicId);
            return NotFound();
        }
        if (topic.UserId != userId)
        {
            _logger.LogWarning("User ID {UserId} attempted to update sort order for topic ID {TopicId} owned by another user", userId, topicId);
            return Forbid();
        }

        try
        {
            await _service.UpdateSortOrderAsync(topicId, sortOrder);
            _logger.LogInformation("Updated sort order for Topic ID: {TopicId} to {SortOrder}", topicId, sortOrder);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update sort order for Topic ID: {TopicId}", topicId);
            return StatusCode(500, "Internal server error");
        }
    }

}
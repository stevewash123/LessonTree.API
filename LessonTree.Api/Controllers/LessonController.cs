// RESPONSIBILITY: Handles HTTP requests for Lesson CRUD operations
// DOES NOT: Handle business logic or data access directly
// CALLED BY: Angular UI via HTTP requestsusing LessonTree.BLL.Service;
using LessonTree.API.Controllers;
using LessonTree.BLL.Service;
using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;
using LessonTree.Models.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class LessonController : BaseController
{
    private readonly ILessonService _lessonService;
    private readonly IAttachmentService _attachmentService;
    private readonly ILogger<LessonController> _logger;

    public LessonController(
        ILessonService lessonService,
        IAttachmentService attachmentService,
        ILogger<LessonController> logger)
    {
        _lessonService = lessonService;
        _attachmentService = attachmentService;
        _logger = logger;
    }

    
    [HttpGet]
    public async Task<IActionResult> GetLessons(ArchiveFilter filter = ArchiveFilter.Active)
    {
        int userId = GetCurrentUserId();
        _logger.LogDebug("Fetching lessons for User ID: {UserId}, Filter: {Filter}", userId, filter);
        var lessons = await _lessonService.GetAllAsync(userId, filter);
        return Ok(lessons);
    }

    [HttpGet("bySubTopic/{subtopicId}")]
    public async Task<IActionResult> GetLessonsBySubtopic(int subtopicId, ArchiveFilter filter = ArchiveFilter.Active)
    {
        int userId = GetCurrentUserId();
        _logger.LogDebug("Fetching lessons for SubTopic ID: {SubTopicId}, User ID: {UserId}, Filter: {Filter}", subtopicId, userId, filter);
        var lessons = await _lessonService.GetLessonsBySubtopic(subtopicId, userId, filter);
        return Ok(lessons);
    }

    [HttpGet("byTopic/{topicId}")]
    public async Task<IActionResult> GetLessonsByTopic(int topicId, ArchiveFilter filter = ArchiveFilter.Active)
    {
        int userId = GetCurrentUserId();
        _logger.LogDebug("Fetching lessons for Topic ID: {TopicId}, User ID: {UserId}, Filter: {Filter}", topicId, userId, filter);
        var lessons = await _lessonService.GetLessonsByTopic(topicId, userId, filter);
        return Ok(lessons);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetLesson(int id)
    {
        int userId = GetCurrentUserId();
        _logger.LogDebug("Fetching lesson ID: {LessonId} for User ID: {UserId}", id, userId);

        var lessonDomain = await _lessonService.GetDomainLessonByIdAsync(id);
        if (lessonDomain == null)
        {
            _logger.LogError("Lesson with ID {LessonId} not found", id);
            return NotFound();
        }

        // Optional: Check ownership if needed
        if (lessonDomain.UserId != userId)
        {
            _logger.LogWarning("User ID {UserId} attempted to access lesson ID {LessonId} owned by another user", userId, id);
            return Forbid();
        }

        var lessonDto = await _lessonService.GetByIdAsync(id);
        return Ok(lessonDto);
    }

    [HttpPost]
    public async Task<IActionResult> AddLesson([FromBody] LessonCreateResource lessonCreateResource)
    {
        int userId = GetCurrentUserId();
        _logger.LogDebug("Adding lesson with Title: {Title} for User ID: {UserId}", lessonCreateResource.Title, userId);

        int createdId = await _lessonService.AddAsync(lessonCreateResource, userId);
        var createdLesson = await _lessonService.GetByIdAsync(createdId);
        _logger.LogInformation("Added lesson with ID: {LessonId}, Title: {Title}", createdLesson.Id, createdLesson.Title);
        return CreatedAtAction(nameof(GetLesson), new { id = createdLesson.Id }, createdLesson);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLesson(int id, [FromBody] LessonUpdateResource lessonUpdateResource)
    {
        int userId = GetCurrentUserId();
        _logger.LogDebug("Updating lesson ID: {LessonId} for User ID: {UserId}", id, userId);
        if (id != lessonUpdateResource.Id)
        {
            _logger.LogWarning("ID mismatch: URL ID {UrlId} does not match body ID {BodyId}", id, lessonUpdateResource.Id);
            return BadRequest();
        }

        var existingLesson = await _lessonService.GetDomainLessonByIdAsync(id);
        if (existingLesson == null)
        {
            _logger.LogError("Lesson with ID {LessonId} not found", id);
            return NotFound();
        }
        if (existingLesson.UserId != userId)
        {
            _logger.LogWarning("User ID {UserId} attempted to update lesson ID {LessonId} owned by another user", userId, id);
            return Forbid();
        }

        var updatedLesson = await _lessonService.UpdateAsync(lessonUpdateResource, userId);
        _logger.LogInformation("Updated lesson with ID: {LessonId}, Title: {Title}", id, lessonUpdateResource.Title);
        return Ok(updatedLesson);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLesson(int id)
    {
        int userId = GetCurrentUserId();
        _logger.LogDebug("Deleting lesson ID: {LessonId} for User ID: {UserId}", id, userId);

        var lesson = await _lessonService.GetDomainLessonByIdAsync(id);
        if (lesson == null)
        {
            _logger.LogError("Lesson with ID {LessonId} not found", id);
            return NotFound();
        }
        if (lesson.UserId != userId)
        {
            _logger.LogWarning("User ID {UserId} attempted to delete lesson ID {LessonId} owned by another user", userId, id);
            return Forbid();
        }

        await _lessonService.DeleteAsync(id);
        _logger.LogInformation("Deleted lesson with ID: {LessonId}", id);
        return NoContent();
    }

    [HttpPost("{id}/attachments")]
    //[Authorize(Roles = "PaidUser,Admin")]
    public async Task<IActionResult> AddAttachment(int id, IFormFile file)
    {
        int userId = GetCurrentUserId();
        _logger.LogDebug("Adding attachment to Lesson ID: {LessonId} for User ID: {UserId}", id, userId);

        var lesson = await _lessonService.GetDomainLessonByIdAsync(id);
        if (lesson == null)
        {
            _logger.LogError("Lesson with ID {LessonId} not found", id);
            return NotFound();
        }
        if (lesson.UserId != userId)
        {
            _logger.LogWarning("User ID {UserId} attempted to add attachment to lesson ID {LessonId} owned by another user", userId, id);
            return Forbid();
        }

        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("No file uploaded for Lesson ID: {LessonId}", id);
            return BadRequest("No file uploaded");
        }

        try
        {
            var attachment = new Attachment
            {
                FileName = file.FileName,
                
                ContentType = file.ContentType,
                Blob = ReadFileBytes(file)
            };
            int attachmentId = _attachmentService.CreateAttachment(attachment);
            await _lessonService.AddAttachmentAsync(id, attachmentId);
            _logger.LogInformation("Added attachment {FileName} to Lesson ID: {LessonId}", file.FileName, id);
            return CreatedAtAction(nameof(GetLesson), new { id }, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add attachment to Lesson ID: {LessonId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    private byte[] ReadFileBytes(IFormFile file)
    {
        using (var memoryStream = new MemoryStream())
        {
            file.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }

    [HttpDelete("{lessonId}/attachments/{attachmentId}")]
    [Authorize(Roles = "PaidUser,Admin")]
    public async Task<IActionResult> RemoveAttachment(int lessonId, int attachmentId)
    {
        int userId = GetCurrentUserId();
        _logger.LogDebug("Removing attachment ID: {AttachmentId} from Lesson ID: {LessonId} for User ID: {UserId}", attachmentId, lessonId, userId);

        var lesson = await _lessonService.GetDomainLessonByIdAsync(lessonId);
        if (lesson == null)
        {
            _logger.LogError("Lesson with ID {LessonId} not found", lessonId);
            return NotFound();
        }
        if (lesson.UserId != userId)
        {
            _logger.LogWarning("User ID {UserId} attempted to remove attachment from lesson ID {LessonId} owned by another user", userId, lessonId);
            return Forbid();
        }

        try
        {
            await _lessonService.RemoveAttachmentAsync(lessonId, attachmentId);
            _logger.LogInformation("Removed attachment ID: {AttachmentId} from Lesson ID: {LessonId}", attachmentId, lessonId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Error removing attachment: {Message}", ex.Message);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error removing attachment ID: {AttachmentId} from Lesson ID: {LessonId}", attachmentId, lessonId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{lessonId}/sortOrder")]
    public async Task<IActionResult> UpdateLessonSortOrder(int lessonId, [FromBody] int sortOrder)
    {
        int userId = GetCurrentUserId();
        _logger.LogDebug("Updating sort order for Lesson ID: {LessonId} to {SortOrder} for User ID: {UserId}", lessonId, sortOrder, userId);

        var lesson = await _lessonService.GetDomainLessonByIdAsync(lessonId);
        if (lesson == null)
        {
            _logger.LogError("Lesson with ID {LessonId} not found", lessonId);
            return NotFound();
        }
        if (lesson.UserId != userId)
        {
            _logger.LogWarning("User ID {UserId} attempted to update sort order for lesson ID {LessonId} owned by another user", userId, lessonId);
            return Forbid();
        }

        try
        {
            await _lessonService.UpdateSortOrderAsync(lessonId, sortOrder);
            _logger.LogInformation("Updated sort order for Lesson ID: {LessonId} to {SortOrder}", lessonId, sortOrder);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update sort order for Lesson ID: {LessonId}", lessonId);
            return StatusCode(500, "Internal server error");
        }
    }

    // Update MoveLesson to handle TopicId and SortOrder
    [HttpPost("move")]
    public async Task<IActionResult> MoveLesson([FromBody] LessonMoveResource moveResource)
    {
        int userId = GetCurrentUserId();
        _logger.LogDebug("Moving Lesson ID: {LessonId} to SubTopic ID: {NewSubTopicId}, Topic ID: {NewTopicId} for User ID: {UserId}",
            moveResource.LessonId, moveResource.NewSubTopicId, moveResource.NewTopicId, userId);

        var lesson = await _lessonService.GetDomainLessonByIdAsync(moveResource.LessonId);
        if (lesson == null)
        {
            _logger.LogError("Lesson with ID {LessonId} not found", moveResource.LessonId);
            return NotFound();
        }
        if (lesson.UserId != userId)
        {
            _logger.LogWarning("User ID {UserId} attempted to move lesson ID {LessonId} owned by another user", userId, moveResource.LessonId);
            return Forbid();
        }

        try
        {
            await _lessonService.MoveLessonAsync(moveResource.LessonId, moveResource.NewSubTopicId, moveResource.NewTopicId);
            _logger.LogInformation("Moved Lesson ID: {LessonId} to SubTopic ID: {NewSubTopicId}, Topic ID: {NewTopicId}",
                moveResource.LessonId, moveResource.NewSubTopicId, moveResource.NewTopicId);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Error moving lesson: {Message}", ex.Message);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error moving Lesson ID: {LessonId}", moveResource.LessonId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{lessonId}/standards")]
    public async Task<IActionResult> AddStandardToLesson(int lessonId, [FromBody] int standardId)
    {
        int userId = GetCurrentUserId();
        _logger.LogDebug("Adding standard {StandardId} to Lesson ID: {LessonId} for User ID: {UserId}", standardId, lessonId, userId);

        var lesson = await _lessonService.GetDomainLessonByIdAsync(lessonId);
        if (lesson == null)
        {
            _logger.LogError("Lesson with ID {LessonId} not found", lessonId);
            return NotFound();
        }
        if (lesson.UserId != userId)
        {
            _logger.LogWarning("User ID {UserId} attempted to add standard to lesson ID {LessonId} owned by another user", userId, lessonId);
            return Forbid();
        }

        try
        {
            await _lessonService.AddStandardToLessonAsync(lessonId, standardId);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Error adding standard: {Message}", ex.Message);
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{lessonId}/standards/{standardId}")]
    public async Task<IActionResult> RemoveStandardFromLesson(int lessonId, int standardId)
    {
        int userId = GetCurrentUserId();
        _logger.LogDebug("Removing standard {StandardId} from Lesson ID: {LessonId} for User ID: {UserId}", standardId, lessonId, userId);

        var lesson = await _lessonService.GetDomainLessonByIdAsync(lessonId);
        if (lesson == null)
        {
            _logger.LogError("Lesson with ID {LessonId} not found", lessonId);
            return NotFound();
        }
        if (lesson.UserId != userId)
        {
            _logger.LogWarning("User ID {UserId} attempted to remove standard from lesson ID {LessonId} owned by another user", userId, lessonId);
            return Forbid();
        }

        try
        {
            await _lessonService.RemoveStandardFromLessonAsync(lessonId, standardId);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Error removing standard: {Message}", ex.Message);
            return NotFound(ex.Message);
        }
    }

    [HttpPost("copy")]
    public async Task<IActionResult> CopyLesson([FromBody] LessonMoveResource copyResource)
    {
        int userId = GetCurrentUserId();
        _logger.LogDebug("Copying Lesson ID: {LessonId} to SubTopic ID: {NewSubTopicId} for User ID: {UserId}",
            copyResource.LessonId, copyResource.NewSubTopicId, userId);

        var lesson = await _lessonService.GetDomainLessonByIdAsync(copyResource.LessonId);
        if (lesson == null)
        {
            _logger.LogError("Lesson with ID {LessonId} not found", copyResource.LessonId);
            return NotFound();
        }

        try
        {
            var newLesson = await _lessonService.CopyLessonAsync(copyResource.LessonId, copyResource.NewSubTopicId, null, userId);
            _logger.LogInformation("Copied Lesson ID: {LessonId} to new Lesson ID: {NewLessonId}", copyResource.LessonId, newLesson.Id);
            return CreatedAtAction(nameof(GetLesson), new { id = newLesson.Id }, newLesson);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Error copying lesson: {Message}", ex.Message);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error copying Lesson ID: {LessonId}", copyResource.LessonId);
            return StatusCode(500, "Internal server error");
        }
    }


}
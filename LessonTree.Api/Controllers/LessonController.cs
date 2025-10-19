// RESPONSIBILITY: Handles HTTP requests for Lesson CRUD operations
// DOES NOT: Handle business logic or data access directly, access domain objects
// CALLED BY: Angular UI via HTTP requests

using LessonTree.BLL.Service;
using LessonTree.API.Controllers;
using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;
using LessonTree.Models.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        var lessonDto = await _lessonService.GetByIdAsync(id, userId); // Service handles ownership validation
        if (lessonDto == null)
        {
            _logger.LogWarning("Lesson with ID {LessonId} not found or not owned by User ID: {UserId}", id, userId);
            return NotFound();
        }

        return Ok(lessonDto);
    }

    [HttpPost]
    public async Task<IActionResult> AddLesson([FromBody] LessonCreateResource lessonCreateResource)
    {
        int userId = GetCurrentUserId();
        _logger.LogDebug("Adding lesson with Title: {Title} for User ID: {UserId}", lessonCreateResource.Title, userId);

        try
        {
            int createdId = await _lessonService.AddAsync(lessonCreateResource, userId);
            var createdLesson = await _lessonService.GetByIdAsync(createdId, userId);
            _logger.LogInformation("Added lesson with ID: {LessonId}, Title: {Title}", createdLesson.Id, createdLesson.Title);
            return CreatedAtAction(nameof(GetLesson), new { id = createdLesson.Id }, createdLesson);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Lesson creation failed: {Message}", ex.Message);
            return BadRequest(new { status = "error", message = ex.Message });
        }
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

        try
        {
            var updatedLesson = await _lessonService.UpdateAsync(lessonUpdateResource, userId); // Service handles ownership validation
            _logger.LogInformation("Updated lesson with ID: {LessonId}, Title: {Title} by User ID: {UserId}", id, lessonUpdateResource.Title, userId);
            return Ok(updatedLesson);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Lesson update failed: {Message}", ex.Message);
            return NotFound(new { status = "error", message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized lesson update attempt for ID: {LessonId} by User ID: {UserId}", id, userId);
            return Forbid();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLesson(int id)
    {
        int userId = GetCurrentUserId();
        _logger.LogDebug("Deleting lesson ID: {LessonId} for User ID: {UserId}", id, userId);

        try
        {
            await _lessonService.DeleteAsync(id, userId); // Service handles ownership validation
            _logger.LogInformation("Deleted lesson with ID: {LessonId} by User ID: {UserId}", id, userId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Lesson deletion failed: {Message}", ex.Message);
            return NotFound(new { status = "error", message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized lesson deletion attempt for ID: {LessonId} by User ID: {UserId}", id, userId);
            return Forbid();
        }
    }

    [HttpPost("{id}/attachments")]
    //[Authorize(Roles = "PaidUser,Admin")]
    public async Task<IActionResult> AddAttachment(int id, IFormFile file)
    {
        int userId = GetCurrentUserId();
        _logger.LogDebug("Adding attachment to Lesson ID: {LessonId} for User ID: {UserId}", id, userId);

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
            int attachmentId = await _attachmentService.CreateAttachmentAsync(attachment);
            await _lessonService.AddAttachmentAsync(id, attachmentId, userId); // Service handles ownership validation
            _logger.LogInformation("Added attachment {FileName} to Lesson ID: {LessonId} by User ID: {UserId}", file.FileName, id, userId);
            return CreatedAtAction(nameof(GetLesson), new { id }, null);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Attachment addition failed: {Message}", ex.Message);
            return NotFound(new { status = "error", message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized attachment addition attempt for Lesson ID: {LessonId} by User ID: {UserId}", id, userId);
            return Forbid();
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

        try
        {
            await _lessonService.RemoveAttachmentAsync(lessonId, attachmentId, userId); // Service handles ownership validation
            _logger.LogInformation("Removed attachment ID: {AttachmentId} from Lesson ID: {LessonId} by User ID: {UserId}", attachmentId, lessonId, userId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Attachment removal failed: {Message}", ex.Message);
            return NotFound(new { status = "error", message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized attachment removal attempt for Lesson ID: {LessonId} by User ID: {UserId}", lessonId, userId);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error removing attachment ID: {AttachmentId} from Lesson ID: {LessonId}", attachmentId, lessonId);
            return StatusCode(500, "Internal server error");
        }
    }


    [HttpPost("{lessonId}/standards")]
    public async Task<IActionResult> AddStandardToLesson(int lessonId, [FromBody] int standardId)
    {
        int userId = GetCurrentUserId();
        _logger.LogDebug("Adding standard {StandardId} to Lesson ID: {LessonId} for User ID: {UserId}", standardId, lessonId, userId);

        try
        {
            await _lessonService.AddStandardToLessonAsync(lessonId, standardId, userId); // Service handles ownership validation
            _logger.LogInformation("Added standard {StandardId} to Lesson ID: {LessonId} by User ID: {UserId}", standardId, lessonId, userId);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Standard addition failed: {Message}", ex.Message);
            return NotFound(new { status = "error", message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized standard addition attempt for Lesson ID: {LessonId} by User ID: {UserId}", lessonId, userId);
            return Forbid();
        }
    }

    [HttpDelete("{lessonId}/standards/{standardId}")]
    public async Task<IActionResult> RemoveStandardFromLesson(int lessonId, int standardId)
    {
        int userId = GetCurrentUserId();
        _logger.LogDebug("Removing standard {StandardId} from Lesson ID: {LessonId} for User ID: {UserId}", standardId, lessonId, userId);

        try
        {
            await _lessonService.RemoveStandardFromLessonAsync(lessonId, standardId, userId); // Service handles ownership validation
            _logger.LogInformation("Removed standard {StandardId} from Lesson ID: {LessonId} by User ID: {UserId}", standardId, lessonId, userId);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Standard removal failed: {Message}", ex.Message);
            return NotFound(new { status = "error", message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized standard removal attempt for Lesson ID: {LessonId} by User ID: {UserId}", lessonId, userId);
            return Forbid();
        }
    }

    [HttpPost("move")]
    public async Task<IActionResult> MoveLesson([FromBody] LessonMoveResource moveResource)
    {
        int userId = GetCurrentUserId();
        _logger.LogDebug("Moving Lesson ID: {LessonId} for User ID: {UserId}", moveResource.LessonId, userId);

        try
        {
            var movedLesson = await _lessonService.MoveLessonAsync(moveResource, userId);
            _logger.LogInformation("Moved Lesson ID: {LessonId} by User ID: {UserId}", moveResource.LessonId, userId);
            
            var result = new LessonPositioningResult
            {
                IsSuccess = true,
                LessonId = movedLesson.Id,
                NewSubTopicId = movedLesson.SubTopicId,
                NewTopicId = movedLesson.TopicId,
                TargetSortOrder = movedLesson.SortOrder,
                ModifiedEntities = new List<ModifiedEntityInfo>
                {
                    new ModifiedEntityInfo
                    {
                        EntityId = movedLesson.Id,
                        EntityType = "Lesson",
                        NewSortOrder = movedLesson.SortOrder,
                        ParentId = movedLesson.SubTopicId ?? movedLesson.TopicId,
                        ParentType = movedLesson.SubTopicId.HasValue ? "SubTopic" : "Topic"
                    }
                }
            };
            
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Lesson move failed: {Message}", ex.Message);
            return NotFound(new { status = "error", message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized Lesson move attempt for ID: {LessonId} by User ID: {UserId}", moveResource.LessonId, userId);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error moving Lesson ID: {LessonId}", moveResource.LessonId);
            return StatusCode(500, new { status = "error", message = ex.Message });
        }
    }

    [HttpPost("move-optimized")]
    public async Task<IActionResult> MoveLessonOptimized([FromBody] LessonMoveResource moveResource)
    {
        int userId = GetCurrentUserId();
        _logger.LogDebug("Optimized moving Lesson ID: {LessonId} for User ID: {UserId}", moveResource.LessonId, userId);

        try
        {
            var result = await _lessonService.MoveLessonWithOptimizationAsync(moveResource, userId);
            _logger.LogInformation("Optimized moved Lesson ID: {LessonId} by User ID: {UserId}, HasPartialUpdates: {HasPartial}",
                moveResource.LessonId, userId, result.HasPartialScheduleUpdates);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Optimized lesson move failed: {Message}", ex.Message);
            return NotFound(new { status = "error", message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized optimized lesson move attempt for ID: {LessonId} by User ID: {UserId}", moveResource.LessonId, userId);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in optimized lesson move for ID: {LessonId}", moveResource.LessonId);
            return StatusCode(500, new { status = "error", message = ex.Message });
        }
    }

    [HttpPost("copy")]
    public async Task<IActionResult> CopyLesson([FromBody] LessonMoveResource copyResource)
    {
        int userId = GetCurrentUserId();
        _logger.LogDebug("Copying Lesson ID: {LessonId} to SubTopic ID: {NewSubTopicId} for User ID: {UserId}",
            copyResource.LessonId, copyResource.NewSubTopicId, userId);

        try
        {
            var newLesson = await _lessonService.CopyLessonAsync(copyResource.LessonId, copyResource.NewSubTopicId, copyResource.NewTopicId, userId);
            _logger.LogInformation("Copied Lesson ID: {LessonId} to new Lesson ID: {NewLessonId} by User ID: {UserId}", copyResource.LessonId, newLesson.Id, userId);
            return CreatedAtAction(nameof(GetLesson), new { id = newLesson.Id }, newLesson);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Lesson copy failed: {Message}", ex.Message);
            return NotFound(new { status = "error", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error copying Lesson ID: {LessonId}", copyResource.LessonId);
            return StatusCode(500, "Internal server error");
        }
    }

    // ✅ NEW: Calendar Update Optimization - Optimized lesson creation with partial schedule generation
    [HttpPost("create-optimized")]
    public async Task<IActionResult> CreateLessonOptimized([FromBody] LessonCreateOptimizedResource createResource)
    {
        int userId = GetCurrentUserId();
        _logger.LogDebug("Creating optimized lesson with Title: {Title} for User ID: {UserId}, HasCalendarRange: {HasRange}",
            createResource.Title, userId, createResource.CalendarStartDate.HasValue);

        try
        {
            var result = await _lessonService.CreateLessonOptimizedAsync(createResource, userId);
            _logger.LogInformation("Created optimized lesson ID: {LessonId} by User ID: {UserId}, IsOptimized: {IsOptimized}",
                result.Lesson.Id, userId, result.IsOptimized);

            return CreatedAtAction(nameof(GetLesson), new { id = result.Lesson.Id }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Optimized lesson creation failed: {Message}", ex.Message);
            return BadRequest(new { status = "error", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in optimized lesson creation");
            return StatusCode(500, new { status = "error", message = ex.Message });
        }
    }

    // ✅ NEW: Calendar Update Optimization - Optimized lesson deletion with partial schedule generation
    [HttpDelete("delete-optimized")]
    public async Task<IActionResult> DeleteLessonOptimized([FromBody] LessonDeleteOptimizedRequest deleteRequest)
    {
        int userId = GetCurrentUserId();
        _logger.LogDebug("Deleting optimized lesson ID: {LessonId} for User ID: {UserId}, HasCalendarRange: {HasRange}",
            deleteRequest.LessonId, userId, deleteRequest.CalendarStartDate.HasValue);

        try
        {
            var result = await _lessonService.DeleteLessonOptimizedAsync(deleteRequest, userId);
            _logger.LogInformation("Deleted optimized lesson ID: {LessonId} by User ID: {UserId}, IsOptimized: {IsOptimized}",
                deleteRequest.LessonId, userId, result.IsOptimized);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Optimized lesson deletion failed: {Message}", ex.Message);
            return NotFound(new { status = "error", message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized optimized lesson deletion attempt for ID: {LessonId} by User ID: {UserId}", deleteRequest.LessonId, userId);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in optimized lesson deletion for ID: {LessonId}", deleteRequest.LessonId);
            return StatusCode(500, new { status = "error", message = ex.Message });
        }
    }


}
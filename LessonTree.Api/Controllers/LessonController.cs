using LessonTree.BLL.Service;
using LessonTree.Models.DTO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LessonTree.DAL.Domain;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class LessonController : ControllerBase
{
    private readonly ILessonService _lessonService;
    private readonly IAttachmentService _attachmentService;
    private readonly ILogger<LessonController> _logger;

    public LessonController(ILessonService lessonService, IAttachmentService attachmentService, ILogger<LessonController> logger)
    {
        _lessonService = lessonService;
        _attachmentService = attachmentService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetLessons()
    {
        var lessons = await _lessonService.GetAllAsync();
        return Ok(lessons);
    }

    [HttpGet("bySubTopic/{subtopicId}")]
    public async Task<IActionResult> GetLessonsBySubtopic(int subtopicId)
    {
        var lessons = await _lessonService.GetLessonsBySubtopic(subtopicId);
        return Ok(lessons);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetLesson(int id)
    {
        var lesson = await _lessonService.GetByIdAsync(id);
        if (lesson == null)
        {
            _logger.LogError("Lesson with ID {LessonId} not found", id);
            return NotFound();
        }
        return Ok(lesson);
    }

    [HttpPost]
    public async Task<IActionResult> AddLesson([FromBody] LessonCreateResource lessonCreateResource)
    {
        _logger.LogDebug("Entering AddLesson with Title: {Title}", lessonCreateResource.Title);
        await _lessonService.AddAsync(lessonCreateResource);
        var createdLesson = await _lessonService.GetByIdAsync((await _lessonService.GetAllAsync()).Last().Id); // Adjusted for async
        _logger.LogInformation("Added lesson with ID: {LessonId}, Title: {Title}", createdLesson.Id, createdLesson.Title);
        return CreatedAtAction(nameof(GetLesson), new { id = createdLesson.Id }, createdLesson);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLesson(int id, [FromBody] LessonUpdateResource lessonUpdateResource)
    {
        _logger.LogDebug("Entering UpdateLesson with ID: {LessonId}", id);
        if (id != lessonUpdateResource.Id)
        {
            _logger.LogWarning("ID mismatch: URL ID {UrlId} does not match body ID {BodyId}", id, lessonUpdateResource.Id);
            return BadRequest();
        }
        await _lessonService.UpdateAsync(lessonUpdateResource);
        _logger.LogInformation("Updated lesson with ID: {LessonId}, Title: {Title}", id, lessonUpdateResource.Title);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLesson(int id)
    {
        _logger.LogDebug("Entering DeleteLesson with ID: {LessonId}", id);
        await _lessonService.DeleteAsync(id);
        _logger.LogInformation("Deleted lesson with ID: {LessonId}", id);
        return NoContent();
    }

    [HttpPost("{id}/attachments")]
    [Authorize(Roles = "PaidUser,Admin")]
    public async Task<IActionResult> AddAttachment(int id, IFormFile file)
    {
        _logger.LogDebug("Entering AddAttachment for Lesson ID: {LessonId}", id);
        var lesson = await _lessonService.GetByIdAsync(id);
        if (lesson == null)
        {
            _logger.LogError("Lesson with ID {LessonId} not found", id);
            return NotFound();
        }

        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("No file uploaded for Lesson ID: {LessonId}", id);
            return BadRequest("No file uploaded");
        }

        try
        {
            // Step 1: Create a new Attachment entity from the uploaded file
            var attachment = new Attachment
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                Blob = ReadFileBytes(file)
            };

            // Step 2: Save the attachment and get its ID using AttachmentService
            int attachmentId = _attachmentService.CreateAttachment(attachment); // TODO: Make this async if possible

            // Step 3: Associate the attachment with the lesson using LessonService
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
        _logger.LogDebug("Entering RemoveAttachment for Lesson ID: {LessonId}, Attachment ID: {AttachmentId}", lessonId, attachmentId);
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

    [HttpPost("move")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> MoveLesson([FromBody] LessonMoveResource moveResource)
    {
        _logger.LogDebug("Entering MoveLesson with Lesson ID: {LessonId}, New SubTopic ID: {NewSubTopicId}",
            moveResource.LessonId, moveResource.NewSubTopicId);

        try
        {
            await _lessonService.MoveLessonAsync(moveResource.LessonId, moveResource.NewSubTopicId);
            _logger.LogInformation("Moved Lesson ID: {LessonId} to SubTopic ID: {NewSubTopicId}",
                moveResource.LessonId, moveResource.NewSubTopicId);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Error moving lesson: {Message}", ex.Message);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error moving Lesson ID: {LessonId} to SubTopic ID: {NewSubTopicId}",
                moveResource.LessonId, moveResource.NewSubTopicId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{lessonId}/standards")]
    public async Task<IActionResult> AddStandardToLesson(int lessonId, [FromBody] int standardId)
    {
        try
        {
            await _lessonService.AddStandardToLessonAsync(lessonId, standardId);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{lessonId}/standards/{standardId}")]
    public async Task<IActionResult> RemoveStandardFromLesson(int lessonId, int standardId)
    {
        try
        {
            await _lessonService.RemoveStandardFromLessonAsync(lessonId, standardId);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("copy")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> CopyLesson([FromBody] LessonMoveResource copyResource)
    {
        _logger.LogDebug("Entering CopyLesson with Lesson ID: {LessonId}, New SubTopic ID: {NewSubTopicId}",
            copyResource.LessonId, copyResource.NewSubTopicId);

        try
        {
            var newLesson = await _lessonService.CopyLessonAsync(copyResource.LessonId, copyResource.NewSubTopicId);
            _logger.LogInformation("Copied Lesson ID: {LessonId} to new Lesson ID: {NewLessonId} under SubTopic ID: {NewSubTopicId}",
                copyResource.LessonId, newLesson.Id, copyResource.NewSubTopicId);
            return CreatedAtAction(nameof(GetLesson), new { id = newLesson.Id }, newLesson);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Error copying lesson: {Message}", ex.Message);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error copying Lesson ID: {LessonId} to SubTopic ID: {NewSubTopicId}",
                copyResource.LessonId, copyResource.NewSubTopicId);
            return StatusCode(500, "Internal server error");
        }
    }
}
using LessonTree.BLL.Service;
using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LessonTree.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class LessonController : ControllerBase
    {
        private readonly ILessonService _lessonService; 
        private readonly IDocumentService _documentService; 
        private readonly ILogger<LessonController> _logger;

        public LessonController(ILessonService lessonService, IDocumentService documentService, ILogger<LessonController> logger)
        {
            _lessonService = lessonService;
            _documentService = documentService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetLessons()
        {
            var lessons = _lessonService.GetAll();
            return Ok(lessons);
        }

        [HttpGet("{id}")]
        public IActionResult GetLesson(int id)
        {
            var lesson = _lessonService.GetById(id);
            if (lesson == null)
            {
                _logger.LogError("Lesson with ID {LessonId} not found", id);
                return NotFound();
            }
            return Ok(lesson);
        }

        [HttpPost]
        public IActionResult AddLesson([FromBody] LessonCreateResource lessonCreateResource)
        {
            _logger.LogDebug("Entering AddLesson with Title: {Title}", lessonCreateResource.Title);
            _lessonService.Add(lessonCreateResource);
            var createdLesson = _lessonService.GetById(_lessonService.GetAll().Last().Id); // Assuming GetAll returns in order of creation
            _logger.LogInformation("Added lesson with ID: {LessonId}, Title: {Title}", createdLesson.Id, createdLesson.Title);
            return CreatedAtAction(nameof(GetLesson), new { id = createdLesson.Id }, createdLesson);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateLesson(int id, [FromBody] LessonUpdateResource lessonUpdateResource)
        {
            _logger.LogDebug("Entering UpdateLesson with ID: {LessonId}", id);
            if (id != lessonUpdateResource.Id)
            {
                _logger.LogWarning("ID mismatch: URL ID {UrlId} does not match body ID {BodyId}", id, lessonUpdateResource.Id);
                return BadRequest();
            }
            _lessonService.Update(lessonUpdateResource);
            _logger.LogInformation("Updated lesson with ID: {LessonId}, Title: {Title}", id, lessonUpdateResource.Title);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteLesson(int id)
        {
            _logger.LogDebug("Entering DeleteLesson with ID: {LessonId}", id);
            _lessonService.Delete(id);
            _logger.LogInformation("Deleted lesson with ID: {LessonId}", id);
            return NoContent();
        }

        [HttpPost("{id}/documents")]
        [Authorize(Roles = "PaidUser,Admin")]
        public IActionResult AddDocument(int id, IFormFile file)
        {
            _logger.LogDebug("Entering AddDocument for Lesson ID: {LessonId}", id);
            var lesson = _lessonService.GetById(id);
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
                // Step 1: Create a new Document entity from the uploaded file
                var document = new Document
                {
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    Blob = ReadFileBytes(file)
                };

                // Step 2: Save the document and get its ID using DocumentService
                int documentId = _documentService.CreateDocument(document);

                // Step 3: Associate the document with the lesson using LessonService
                _lessonService.AddDocument(id, documentId);
                _logger.LogInformation("Added document {FileName} to Lesson ID: {LessonId}", file.FileName, id);

                return CreatedAtAction(nameof(GetLesson), new { id }, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add document to Lesson ID: {LessonId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
        
        // Helper method to read file bytes (already present)
        private byte[] ReadFileBytes(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        [HttpDelete("{lessonId}/documents/{documentId}")]
        [Authorize(Roles = "PaidUser,Admin")] // Restrict to PaidUser and Admin
        public IActionResult RemoveDocument(int lessonId, int documentId)
        {
            _logger.LogDebug("Entering RemoveDocument for Lesson ID: {LessonId}, Document ID: {DocumentId}", lessonId, documentId);
            try
            {
                _lessonService.RemoveDocument(lessonId, documentId);
                _logger.LogInformation("Removed document ID: {DocumentId} from Lesson ID: {LessonId}", documentId, lessonId);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("Error removing document: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error removing document ID: {DocumentId} from Lesson ID: {LessonId}", documentId, lessonId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("move")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult MoveLesson([FromBody] LessonMoveResource moveResource)
        {
            _logger.LogDebug("Entering MoveLesson with Lesson ID: {LessonId}, New SubTopic ID: {NewSubTopicId}",
                moveResource.LessonId, moveResource.NewSubTopicId);

            try
            {
                _lessonService.MoveLesson(moveResource.LessonId, moveResource.NewSubTopicId);
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
        public IActionResult AddStandardToLesson(int lessonId, [FromBody] int standardId)
        {
            try
            {
                _lessonService.AddStandardToLesson(lessonId, standardId);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{lessonId}/standards/{standardId}")]
        public IActionResult RemoveStandardFromLesson(int lessonId, int standardId)
        {
            try
            {
                _lessonService.RemoveStandardFromLesson(lessonId, standardId);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("copy")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult CopyLesson([FromBody] LessonMoveResource copyResource)
        {
            _logger.LogDebug("Entering CopyLesson with Lesson ID: {LessonId}, New SubTopic ID: {NewSubTopicId}",
                copyResource.LessonId, copyResource.NewSubTopicId);

            try
            {
                var newLesson = _lessonService.CopyLesson(copyResource.LessonId, copyResource.NewSubTopicId);
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
}
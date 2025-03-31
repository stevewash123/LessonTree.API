// File: NoteController.cs
using Microsoft.AspNetCore.Mvc;
using LessonTree.DAL.Repositories;
using LessonTree.Models.DTO;
using AutoMapper;
using System.Threading.Tasks;
using LessonTree.DAL.Domain;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace LessonTree.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class NoteController : ControllerBase
    {
        private readonly INotesRepository _notesRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<NoteController> _logger;

        public NoteController(INotesRepository notesRepository, IUserRepository userRepository, IMapper mapper, ILogger<NoteController> logger)
        {
            _notesRepository = notesRepository;
            _userRepository = userRepository;
            _mapper = mapper;
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


        [HttpPost]
        public async Task<IActionResult> AddNote([FromBody] NoteCreateResource noteCreateResource)
        {
            _logger.LogInformation("Adding note for CourseId={CourseId}, TopicId={TopicId}, SubTopicId={SubTopicId}, LessonId={LessonId}, timestamp: {Timestamp}",
                noteCreateResource.CourseId, noteCreateResource.TopicId, noteCreateResource.SubTopicId, noteCreateResource.LessonId, DateTime.UtcNow.ToString("o"));

            // Validate that exactly one parent ID is provided
            int parentCount = (noteCreateResource.CourseId.HasValue ? 1 : 0) +
                             (noteCreateResource.TopicId.HasValue ? 1 : 0) +
                             (noteCreateResource.SubTopicId.HasValue ? 1 : 0) +
                             (noteCreateResource.LessonId.HasValue ? 1 : 0);
            if (parentCount != 1)
            {
                _logger.LogWarning("Invalid note creation: Exactly one parent ID must be provided, timestamp: {Timestamp}", DateTime.UtcNow.ToString("o"));
                return BadRequest("Exactly one parent ID (CourseId, TopicId, SubTopicId, or LessonId) must be provided.");
            }

            var note = _mapper.Map<Note>(noteCreateResource);
            int userId = GetCurrentUserId();
            note.CreatedBy = _userRepository.GetById(userId);
            await _notesRepository.AddAsync(note);

            _logger.LogInformation("Note added with ID {NoteId}, timestamp: {Timestamp}", note.Id, DateTime.UtcNow.ToString("o"));
            var noteResource = _mapper.Map<NoteResource>(note);
            return Ok(noteResource);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNote(int id, [FromBody] NoteUpdateResource noteUpdateResource)
        {
            _logger.LogInformation("Updating note with ID {NoteId}, timestamp: {Timestamp}", id, DateTime.UtcNow.ToString("o"));
            var note = await _notesRepository.GetByIdAsync(id);
            if (note == null)
            {
                _logger.LogWarning("Note with ID {NoteId} not found, timestamp: {Timestamp}", id, DateTime.UtcNow.ToString("o"));
                return NotFound();
            }

            _mapper.Map(noteUpdateResource, note);
            await _notesRepository.UpdateAsync(note);

            _logger.LogInformation("Note updated with ID {NoteId}, timestamp: {Timestamp}", id, DateTime.UtcNow.ToString("o"));
            var noteResource = _mapper.Map<NoteResource>(note);
            return Ok(noteResource);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            _logger.LogInformation("Deleting note with ID {NoteId}, timestamp: {Timestamp}", id, DateTime.UtcNow.ToString("o"));
            try
            {
                await _notesRepository.DeleteAsync(id);
                _logger.LogInformation("Note deleted with ID {NoteId}, timestamp: {Timestamp}", id, DateTime.UtcNow.ToString("o"));
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Note with ID {NoteId} not found, timestamp: {Timestamp}, error: {Error}", id, DateTime.UtcNow.ToString("o"), ex.Message);
                return NotFound();
            }
        }
    }
}
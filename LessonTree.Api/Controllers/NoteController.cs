using Microsoft.AspNetCore.Mvc;
using LessonTree.Models.DTO;
using LessonTree.BLL.Service;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using LessonTree.API.Controllers;

namespace LessonTree.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class NoteController : BaseController
    {
        private readonly INoteService _noteService;
        private readonly ILogger<NoteController> _logger;

        public NoteController(INoteService noteService, ILogger<NoteController> logger)
        {
            _noteService = noteService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AddNote([FromBody] NoteCreateResource noteCreateResource)
        {
            int userId = GetCurrentUserId();

            try
            {
                var noteResource = await _noteService.CreateNoteAsync(noteCreateResource, userId);
                return Ok(noteResource);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid note creation request for User ID: {UserId} - {Message}", userId, ex.Message);
                return BadRequest(new { status = "error", message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNote(int id, [FromBody] NoteUpdateResource noteUpdateResource)
        {
            int userId = GetCurrentUserId();

            var noteResource = await _noteService.UpdateNoteAsync(id, noteUpdateResource, userId);
            if (noteResource == null)
            {
                return NotFound($"Note {id} not found");
            }

            return Ok(noteResource);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            int userId = GetCurrentUserId();

            try
            {
                await _noteService.DeleteNoteAsync(id, userId);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid note deletion request for Note ID: {NoteId}, User ID: {UserId} - {Message}", id, userId, ex.Message);
                return NotFound(new { status = "error", message = ex.Message });
            }
        }
    }
}
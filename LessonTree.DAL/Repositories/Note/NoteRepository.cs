using LessonTree.DAL.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LessonTree.DAL.Repositories
{
    public class NotesRepository : INotesRepository
    {
        private readonly LessonTreeContext _context;
        private readonly ILogger<NotesRepository> _logger;

        public NotesRepository(LessonTreeContext context, ILogger<NotesRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Note?> GetByIdAsync(int id)
        {
            _logger.LogDebug("Retrieving note with ID: {NoteId}", id);

            var note = await _context.Notes.FirstOrDefaultAsync(n => n.Id == id);
            if (note == null)
            {
                _logger.LogWarning("Note with ID {NoteId} not found", id);
            }
            else
            {
                _logger.LogDebug("Note with ID {NoteId} retrieved successfully", id);
            }

            return note;
        }

        public async Task<int> AddAsync(Note note)
        {
            _logger.LogDebug("Adding note for User ID: {UserId}", note.UserId);

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Note added with ID: {NoteId} for User ID: {UserId}", note.Id, note.UserId);
            return note.Id;
        }

        public async Task UpdateAsync(Note note)
        {
            _logger.LogDebug("Updating note with ID: {NoteId}", note.Id);

            _context.Notes.Update(note);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Note with ID: {NoteId} updated successfully", note.Id);
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogDebug("Deleting note with ID: {NoteId}", id);

            var note = await _context.Notes.FindAsync(id);
            if (note != null)
            {
                _context.Notes.Remove(note);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Note with ID: {NoteId} deleted successfully", id);
            }
            else
            {
                _logger.LogWarning("Note with ID {NoteId} not found for deletion", id);
            }
        }
    }
}
// **COMPLETE FILE** - NoteRepository.cs - Standardized to match ScheduleRepository patterns
// RESPONSIBILITY: Note data access with consistent enterprise patterns
// DOES NOT: Handle note content validation (that's in services)
// CALLED BY: NoteService for all note operations

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
            _logger.LogInformation($"GetByIdAsync: Fetching note {id}");

            var note = await _context.Notes.FirstOrDefaultAsync(n => n.Id == id);

            if (note != null)
            {
                _logger.LogInformation($"GetByIdAsync: Found note {id} for user {note.UserId}");
            }
            else
            {
                _logger.LogInformation($"GetByIdAsync: Note {id} not found");
            }

            return note;
        }

        public async Task<int> AddAsync(Note note)
        {
            _logger.LogInformation($"AddAsync: Creating note for user {note.UserId}");

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"AddAsync: Created note {note.Id} for user {note.UserId}");
            return note.Id;
        }

        public async Task UpdateAsync(Note note)
        {
            _logger.LogInformation($"UpdateAsync: Updating note {note.Id}");

            var existingNote = await _context.Notes.FindAsync(note.Id);
            if (existingNote == null)
            {
                throw new ArgumentException($"Note {note.Id} not found");
            }

            // Update fields
            existingNote.Content = note.Content;
            existingNote.CourseId = note.CourseId;
            existingNote.TopicId = note.TopicId;
            existingNote.SubTopicId = note.SubTopicId;
            existingNote.LessonId = note.LessonId;
            existingNote.Visibility = note.Visibility;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"UpdateAsync: Updated note {note.Id}");
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation($"DeleteAsync: Deleting note {id}");

            var note = await _context.Notes.FindAsync(id);
            if (note == null)
            {
                throw new ArgumentException($"Note {id} not found");
            }

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"DeleteAsync: Deleted note {id}");
        }
    }
}
// File: NotesRepository.cs
using LessonTree.DAL.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LessonTree.DAL.Repositories
{
    public class NotesRepository : INotesRepository
    {
        private readonly LessonTreeContext _context;

        public NotesRepository(LessonTreeContext context)
        {
            _context = context;
        }

        public async Task<Note> GetByIdAsync(int id)
        {
            return await _context.Notes
                .FirstOrDefaultAsync(n => n.Id == id) ?? throw new Exception($"Note with ID {id} not found.");
        }

        public async Task AddAsync(Note note)
        {
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Note note)
        {
            _context.Notes.Update(note);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var note = await GetByIdAsync(id);
            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();
        }
    }
}
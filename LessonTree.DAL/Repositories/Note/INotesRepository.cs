using LessonTree.DAL.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LessonTree.DAL.Repositories
{
    public interface INotesRepository
    {
        Task<Note> GetByIdAsync(int id);
        Task AddAsync(Note note);
        Task UpdateAsync(Note note);
        Task DeleteAsync(int id);
    }
}

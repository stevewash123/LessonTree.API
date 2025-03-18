using System;
using System.Collections.Generic;
using System.Linq;
using LessonTree.DAL.Domain;

namespace LessonTree.DAL.Repositories
{
    public interface ILessonRepository
    {
        IQueryable<Lesson> GetAll(Func<IQueryable<Lesson>, IQueryable<Lesson>> include = null);
        Task<Lesson?> GetByIdAsync(int id, Func<IQueryable<Lesson>, IQueryable<Lesson>> include = null);
        Task<int> AddAsync(Lesson lesson); // Changed to Task<int>
        Task UpdateAsync(Lesson lesson);
        Task DeleteAsync(int id);
        Task AddAttachmentAsync(int lessonId, int attachmentId);
        Task RemoveAttachmentAsync(int lessonId, int attachmentId);
        IQueryable<Lesson> GetByTitle(string title);
    }
}
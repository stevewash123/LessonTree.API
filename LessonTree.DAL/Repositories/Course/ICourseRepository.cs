using LessonTree.DAL.Domain;
using Microsoft.EntityFrameworkCore; // Added for IQueryable

namespace LessonTree.DAL.Repositories
{
    public interface ICourseRepository
    {
        IQueryable<Course> GetAll(Func<IQueryable<Course>, IQueryable<Course>> include = null);
        Task<Course> GetByIdAsync(int id, Func<IQueryable<Course>, IQueryable<Course>> include = null);
        Task AddAsync(Course course);
        Task UpdateAsync(Course course);
        Task DeleteAsync(int id);
    }
}
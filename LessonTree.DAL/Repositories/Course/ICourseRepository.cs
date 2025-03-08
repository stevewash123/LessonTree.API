using LessonTree.DAL.Domain;
using Microsoft.EntityFrameworkCore; // Added for IQueryable

namespace LessonTree.DAL.Repositories
{
    public interface ICourseRepository
    {
        IQueryable<Course> GetAll(Func<IQueryable<Course>, IQueryable<Course>> include = null);
        Course GetById(int id, Func<IQueryable<Course>, IQueryable<Course>> include = null);
        void Add(Course course);
        void Update(Course course);
        void Delete(int id);
    }
}
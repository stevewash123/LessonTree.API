using LessonTree.DAL.Domain;
using Microsoft.EntityFrameworkCore;

namespace LessonTree.DAL.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        private readonly LessonTreeContext _context;

        public CourseRepository(LessonTreeContext context) => _context = context;

        public IQueryable<Course> GetAll(Func<IQueryable<Course>, IQueryable<Course>> include = null)
        {
            IQueryable<Course> query = _context.Courses;
            if (include != null)
            {
                query = include(query);
            }
            return query;
        }

        public int? GetUserSchoolId(int userId)
        {
            return _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.SchoolId)
                .FirstOrDefault();
        }

        public async Task<Course> GetByIdAsync(int id, Func<IQueryable<Course>, IQueryable<Course>> include = null)
        {
            IQueryable<Course> query = _context.Courses;
            if (include != null)
            {
                query = include(query);
            }
            // FIX: Return null instead of new Course() when not found
            return await query.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Course course)
        {
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Course course)
        {
            _context.Courses.Update(course);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var course = await GetByIdAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
        }
    }
}
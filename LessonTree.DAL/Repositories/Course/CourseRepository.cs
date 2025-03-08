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
            return query; // Return IQueryable to maintain flexibility
        }

        public Course GetById(int id, Func<IQueryable<Course>, IQueryable<Course>> include = null)
        {
            IQueryable<Course> query = _context.Courses;
            if (include != null)
            {
                query = include(query);
            }
            return query.FirstOrDefault(c => c.Id == id) ?? new Course(); // Default to empty Course if not found
        }

        public void Add(Course course)
        {
            _context.Courses.Add(course);
            _context.SaveChanges();
        }

        public void Update(Course course)
        {
            _context.Courses.Update(course);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var course = GetById(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                _context.SaveChanges();
            }
        }
    }
}
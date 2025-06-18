// **COMPLETE FILE** - CourseRepository.cs - Standardized to enterprise patterns
// RESPONSIBILITY: Course data access with user school context and visibility filtering
// DOES NOT: Handle course content validation or business rules (that's in services)
// CALLED BY: CourseService for all course operations

using LessonTree.DAL.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LessonTree.DAL.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        private readonly LessonTreeContext _context;
        private readonly ILogger<CourseRepository> _logger;

        public CourseRepository(LessonTreeContext context, ILogger<CourseRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IQueryable<Course> GetAll(Func<IQueryable<Course>, IQueryable<Course>> include = null)
        {
            _logger.LogInformation("GetAll: Retrieving all courses");

            IQueryable<Course> query = _context.Courses;
            if (include != null)
            {
                query = include(query);
            }
            return query;
        }

        public int? GetUserSchoolId(int userId)
        {
            _logger.LogInformation($"GetUserSchoolId: Fetching school ID for user {userId}");

            var schoolId = _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.SchoolId)
                .FirstOrDefault();

            if (schoolId.HasValue)
            {
                _logger.LogInformation($"GetUserSchoolId: Found school ID {schoolId} for user {userId}");
            }
            else
            {
                _logger.LogInformation($"GetUserSchoolId: No school ID found for user {userId}");
            }

            return schoolId;
        }

        public async Task<Course?> GetByIdAsync(int id, Func<IQueryable<Course>, IQueryable<Course>> include = null)
        {
            _logger.LogInformation($"GetByIdAsync: Fetching course {id}");

            IQueryable<Course> query = _context.Courses;
            if (include != null)
            {
                query = include(query);
            }

            var course = await query.FirstOrDefaultAsync(c => c.Id == id);

            if (course != null)
            {
                _logger.LogInformation($"GetByIdAsync: Found course {id} for user {course.UserId}");
            }
            else
            {
                _logger.LogInformation($"GetByIdAsync: Course {id} not found");
            }

            return course;
        }

        public async Task AddAsync(Course course)
        {
            _logger.LogInformation($"AddAsync: Creating course '{course.Title}' for user {course.UserId}");

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"AddAsync: Created course {course.Id} for user {course.UserId}");
        }

        public async Task UpdateAsync(Course course)
        {
            _logger.LogInformation($"UpdateAsync: Updating course {course.Id}");

            _context.Courses.Update(course);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"UpdateAsync: Updated course {course.Id}");
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation($"DeleteAsync: Deleting course {id}");

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                throw new ArgumentException($"Course {id} not found");
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"DeleteAsync: Deleted course {id}");
        }
    }
}
// **COMPLETE FILE** - StandardRepository.cs - Standardized to enterprise patterns
// RESPONSIBILITY: Standard data access with course/topic filtering and district-level operations
// DOES NOT: Handle standard content validation or educational compliance (that's in services)
// CALLED BY: StandardService for all standard operations

using LessonTree.DAL.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LessonTree.DAL.Repositories
{
    public class StandardRepository : IStandardRepository
    {
        private readonly LessonTreeContext _context;
        private readonly ILogger<StandardRepository> _logger;

        public StandardRepository(LessonTreeContext context, ILogger<StandardRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IQueryable<Standard> GetAll()
        {
            _logger.LogInformation("GetAll: Retrieving all standards");
            return _context.Standards.AsQueryable();
        }

        public async Task<Standard?> GetByIdAsync(int id)
        {
            _logger.LogInformation($"GetByIdAsync: Fetching standard {id}");

            var standard = await _context.Standards.FindAsync(id);

            if (standard != null)
            {
                _logger.LogInformation($"GetByIdAsync: Found standard {id} - '{standard.Title}'");
            }
            else
            {
                _logger.LogInformation($"GetByIdAsync: Standard {id} not found");
            }

            return standard;
        }

        public async Task<int> AddAsync(Standard standard)
        {
            _logger.LogInformation($"AddAsync: Creating standard '{standard.Title}' for course {standard.CourseId}");

            _context.Standards.Add(standard);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"AddAsync: Created standard {standard.Id} for course {standard.CourseId}");
            return standard.Id;
        }

        public async Task UpdateAsync(Standard standard)
        {
            _logger.LogInformation($"UpdateAsync: Updating standard {standard.Id}");

            _context.Standards.Update(standard);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"UpdateAsync: Updated standard {standard.Id}");
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation($"DeleteAsync: Deleting standard {id}");

            var standard = await _context.Standards.FindAsync(id);
            if (standard == null)
            {
                throw new ArgumentException($"Standard {id} not found");
            }

            _context.Standards.Remove(standard);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"DeleteAsync: Deleted standard {id}");
        }

        public IQueryable<Standard> GetByTopicId(int topicId)
        {
            _logger.LogInformation($"GetByTopicId: Retrieving standards for topic {topicId}");
            return _context.Standards.Where(s => s.TopicId == topicId);
        }

        public IQueryable<Standard> GetByCourseId(int courseId)
        {
            _logger.LogInformation($"GetByCourseId: Retrieving standards for course {courseId}");
            return _context.Standards.Where(s => s.CourseId == courseId);
        }
    }
}
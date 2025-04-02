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
            _logger.LogDebug("Retrieving all standards");
            return _context.Standards.AsQueryable();
        }

        public async Task<Standard?> GetByIdAsync(int id)
        {
            _logger.LogDebug("Retrieving standard by ID: {StandardId}", id);
            var standard = await _context.Standards.FindAsync(id);
            if (standard == null)
            {
                _logger.LogWarning("Standard with ID {StandardId} not found", id);
            }
            return standard;
        }

        public async Task<int> AddAsync(Standard standard)
        {
            _logger.LogDebug("Adding standard: {Title}", standard.Title);
            try
            {
                _context.Standards.Add(standard);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Added standard with ID: {StandardId}, Title: {Title}", standard.Id, standard.Title);
                return standard.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add standard: {Title}", standard.Title);
                throw;
            }
        }

        public async Task UpdateAsync(Standard standard)
        {
            _logger.LogDebug("Updating standard with ID: {StandardId}, Title: {Title}", standard.Id, standard.Title);
            try
            {
                _context.Standards.Update(standard);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated standard with ID: {StandardId}, Title: {Title}", standard.Id, standard.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update standard with ID: {StandardId}", standard.Id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogDebug("Deleting standard with ID: {StandardId}", id);
            try
            {
                var standard = await _context.Standards.FindAsync(id);
                if (standard != null)
                {
                    _context.Standards.Remove(standard);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Deleted standard with ID: {StandardId}", id);
                }
                else
                {
                    _logger.LogWarning("Standard with ID {StandardId} not found for deletion", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete standard with ID: {StandardId}", id);
                throw;
            }
        }

        public IQueryable<Standard> GetByTopicId(int topicId)
        {
            _logger.LogDebug("Retrieving standards by Topic ID: {TopicId}", topicId);
            return _context.Standards.Where(s => s.TopicId == topicId);
        }

        public IQueryable<Standard> GetByCourseId(int courseId)
        {
            _logger.LogDebug("Retrieving standards by Course ID: {CourseId}", courseId);
            return _context.Standards.Where(s => s.CourseId == courseId);
        }
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using LessonTree.DAL.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LessonTree.DAL.Repositories
{
    public class SubTopicRepository : ISubTopicRepository
    {
        private readonly LessonTreeContext _context;
        private readonly ILogger<SubTopicRepository> _logger;

        public SubTopicRepository(LessonTreeContext context, ILogger<SubTopicRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IQueryable<SubTopic> GetAll(Func<IQueryable<SubTopic>, IQueryable<SubTopic>> include = null)
        {
            _logger.LogDebug("Retrieving all subtopics");
            IQueryable<SubTopic> query = _context.SubTopics;
            if (include != null)
            {
                query = include(query);
            }
            return query;
        }

        public async Task<SubTopic> GetByIdAsync(int id, Func<IQueryable<SubTopic>, IQueryable<SubTopic>> include = null)
        {
            _logger.LogDebug("Retrieving subtopic by ID: {SubTopicId}", id);
            IQueryable<SubTopic> query = _context.SubTopics;
            if (include != null)
            {
                query = include(query);
            }
            var subTopic = await query.FirstOrDefaultAsync(st => st.Id == id);
            if (subTopic == null)
            {
                _logger.LogWarning("SubTopic with ID {SubTopicId} not found", id);
            }
            return subTopic; // Return null if not found, instead of new SubTopic()
        }

        public async Task<int> AddAsync(SubTopic subTopic)
        {
            _logger.LogDebug("Adding subtopic: {Title}", subTopic.Title);
            _context.SubTopics.Add(subTopic);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Added subtopic with ID: {SubTopicId}, Title: {Title}", subTopic.Id, subTopic.Title);
            return subTopic.Id;
        }

        public async Task UpdateAsync(SubTopic subTopic)
        {
            _logger.LogDebug("Updating subtopic: {Title}", subTopic.Title);
            _logger.LogDebug("Updating SubTopic {Id} with TopicId {TopicId}, IsDefault {IsDefault}", subTopic.Id, subTopic.TopicId, subTopic.IsDefault);
            _context.SubTopics.Update(subTopic);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated subtopic with ID: {SubTopicId}, Title: {Title}", subTopic.Id, subTopic.Title);
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogDebug("Deleting subtopic with ID: {SubTopicId}", id);
            var subTopic = await _context.SubTopics.FindAsync(id);
            if (subTopic != null)
            {
                _context.SubTopics.Remove(subTopic);
                var changes = await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted subtopic with ID: {SubTopicId}, Rows affected: {Changes}", id, changes);
            }
            else
            {
                _logger.LogWarning("SubTopic with ID {SubTopicId} not found for deletion", id);
            }
        }
    }
}
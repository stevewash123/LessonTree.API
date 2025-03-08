using System.Collections.Generic;
using System.Linq;
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

        public SubTopic GetById(int id, Func<IQueryable<SubTopic>, IQueryable<SubTopic>> include = null)
        {
            _logger.LogDebug("Retrieving subtopic by ID: {SubTopicId}", id);
            IQueryable<SubTopic> query = _context.SubTopics;
            if (include != null)
            {
                query = include(query);
            }
            var subTopic = query.FirstOrDefault(st => st.Id == id) ?? new SubTopic(); // Default to empty SubTopic if not found
            if (subTopic == null)
                _logger.LogWarning("SubTopic with ID {SubTopicId} not found", id);
            return subTopic;
        }

        public void Add(SubTopic subTopic)
        {
            _logger.LogDebug("Adding subtopic: {Title}", subTopic.Title);
            _context.SubTopics.Add(subTopic);
            _context.SaveChanges();
            _logger.LogInformation("Added subtopic with ID: {SubTopicId}, Title: {Title}", subTopic.Id, subTopic.Title);
        }

        public void Update(SubTopic subTopic)
        {
            _logger.LogDebug("Updating subtopic: {Title}", subTopic.Title);
            _context.SubTopics.Update(subTopic);
            _context.SaveChanges();
            _logger.LogInformation("Updated subtopic with ID: {SubTopicId}, Title: {Title}", subTopic.Id, subTopic.Title);
        }

        public void Delete(int id)
        {
            _logger.LogDebug("Deleting subtopic with ID: {SubTopicId}", id);
            var subTopic = _context.SubTopics.Find(id);
            if (subTopic != null)
            {
                _context.SubTopics.Remove(subTopic);
                _context.SaveChanges();
                _logger.LogInformation("Deleted subtopic with ID: {SubTopicId}", id);
            }
            else
            {
                _logger.LogWarning("SubTopic with ID {SubTopicId} not found for deletion", id);
            }
        }
    }
}
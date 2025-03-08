using System.Collections.Generic;
using System.Linq;
using LessonTree.DAL.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LessonTree.DAL.Repositories
{
    public class TopicRepository : ITopicRepository
    {
        private readonly LessonTreeContext _context;
        private readonly ILogger<TopicRepository> _logger;

        public TopicRepository(LessonTreeContext context, ILogger<TopicRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IQueryable<Topic> GetAll(Func<IQueryable<Topic>, IQueryable<Topic>> include = null)
        {
            _logger.LogDebug("Retrieving all topics");
            IQueryable<Topic> query = _context.Topics;
            if (include != null)
            {
                query = include(query);
            }
            return query;
        }

        public Topic GetById(int id, Func<IQueryable<Topic>, IQueryable<Topic>> include = null)
        {
            _logger.LogDebug("Retrieving topic by ID: {TopicId}", id);
            IQueryable<Topic> query = _context.Topics;
            if (include != null)
            {
                query = include(query);
            }
            var topic = query.FirstOrDefault(t => t.Id == id) ?? new Topic(); // Default to empty Topic if not found
            if (topic == null)
                _logger.LogWarning("Topic with ID {TopicId} not found", id);
            return topic;
        }

        public void Add(Topic topic)
        {
            _logger.LogDebug("Adding topic: {Title}", topic.Title);
            _context.Topics.Add(topic);
            _context.SaveChanges();
            _logger.LogInformation("Added topic with ID: {TopicId}, Title: {Title}", topic.Id, topic.Title);
        }

        public void Update(Topic topic)
        {
            _logger.LogDebug("Updating topic: {Title}", topic.Title);
            _context.Topics.Update(topic);
            _context.SaveChanges();
            _logger.LogInformation("Updated topic with ID: {TopicId}, Title: {Title}", topic.Id, topic.Title);
        }

        public void Delete(int id)
        {
            _logger.LogDebug("Deleting topic with ID: {TopicId}", id);
            var topic = _context.Topics.Find(id);
            if (topic != null)
            {
                _context.Topics.Remove(topic);
                _context.SaveChanges();
                _logger.LogInformation("Deleted topic with ID: {TopicId}", id);
            }
            else
            {
                _logger.LogWarning("Topic with ID {TopicId} not found for deletion", id);
            }
        }
    }
}
using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
using LessonTree.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

    public async Task<Topic> GetByIdAsync(int id, Func<IQueryable<Topic>, IQueryable<Topic>> include = null)
    {
        _logger.LogDebug("Retrieving topic by ID: {TopicId}", id);
        IQueryable<Topic> query = _context.Topics;
        if (include != null)
        {
            query = include(query);
        }
        var topic = await query.FirstOrDefaultAsync(t => t.Id == id); 
        if (topic == null)
            _logger.LogWarning("Topic with ID {TopicId} not found", id);
        return topic;
    }

    public async Task<int> AddAsync(Topic topic)
    {
        _logger.LogDebug("Adding topic: {Title}", topic.Title);
        _context.Topics.Add(topic);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Added topic with ID: {TopicId}, Title: {Title}", topic.Id, topic.Title);
        return topic.Id; // Return the ID after saving
    }

    public async Task UpdateAsync(Topic topic)
    {
        _logger.LogDebug("Updating topic: {Title}", topic.Title);
        _context.Topics.Update(topic);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated topic with ID: {TopicId}, Title: {Title}", topic.Id, topic.Title);
    }

    public async Task DeleteAsync(int id)
    {
        _logger.LogDebug("Deleting topic with ID: {TopicId}", id);
        var topic = await _context.Topics.FindAsync(id);
        if (topic != null)
        {
            _context.Topics.Remove(topic);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Deleted topic with ID: {TopicId}", id);
        }
        else
        {
            _logger.LogWarning("Topic with ID {TopicId} not found for deletion", id);
        }
    }
}
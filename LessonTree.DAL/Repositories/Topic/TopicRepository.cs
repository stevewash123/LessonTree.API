// **COMPLETE FILE** - TopicRepository.cs - Standardized to enterprise patterns
// RESPONSIBILITY: Topic data access with course relationships and hierarchy management
// DOES NOT: Handle topic content validation or business rules (that's in services)
// CALLED BY: TopicService for all topic operations

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
        _logger.LogInformation("GetAll: Retrieving all topics");

        IQueryable<Topic> query = _context.Topics;
        if (include != null)
        {
            query = include(query);
        }
        return query;
    }

    public async Task<Topic?> GetByIdAsync(int id, Func<IQueryable<Topic>, IQueryable<Topic>> include = null)
    {
        _logger.LogInformation($"GetByIdAsync: Fetching topic {id}");

        IQueryable<Topic> query = _context.Topics;
        if (include != null)
        {
            query = include(query);
        }

        var topic = await query.FirstOrDefaultAsync(t => t.Id == id);

        if (topic != null)
        {
            _logger.LogInformation($"GetByIdAsync: Found topic {id} for user {topic.UserId}");
        }
        else
        {
            _logger.LogInformation($"GetByIdAsync: Topic {id} not found");
        }

        return topic;
    }

    public async Task<int> AddAsync(Topic topic)
    {
        _logger.LogInformation($"AddAsync: Creating topic '{topic.Title}' for user {topic.UserId}");

        _context.Topics.Add(topic);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"AddAsync: Created topic {topic.Id} for user {topic.UserId}");
        return topic.Id;
    }

    public async Task UpdateAsync(Topic topic)
    {
        _logger.LogInformation($"UpdateAsync: Updating topic {topic.Id}");

        _context.Topics.Update(topic);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"UpdateAsync: Updated topic {topic.Id}");
    }

    public async Task DeleteAsync(int id)
    {
        _logger.LogInformation($"DeleteAsync: Deleting topic {id}");

        var topic = await _context.Topics.FindAsync(id);
        if (topic == null)
        {
            throw new ArgumentException($"Topic {id} not found");
        }

        _context.Topics.Remove(topic);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"DeleteAsync: Deleted topic {id}");
    }
}
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

    public async Task<Topic> MoveTopicToPositionAsync(int topicId, int targetCourseId, int afterSiblingId)
    {
        _logger.LogInformation($"MoveTopicToPositionAsync: Moving topic {topicId} after topic {afterSiblingId} in course {targetCourseId}");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Get the topic to move
            var topic = await GetByIdAsync(topicId);
            if (topic == null)
            {
                throw new ArgumentException($"Topic {topicId} not found");
            }

            // Get sibling topic for position calculation
            var siblingTopic = await GetByIdAsync(afterSiblingId);
            if (siblingTopic == null)
            {
                throw new ArgumentException($"Sibling topic {afterSiblingId} not found");
            }

            // Get all topics in target course
            var courseTopics = await _context.Topics
                .Where(t => t.CourseId == targetCourseId && !t.Archived)
                .OrderBy(t => t.SortOrder)
                .ToListAsync();

            // Calculate target position based on sibling (always after sibling)
            var targetSortOrder = CalculateTargetSortOrderFromSibling(courseTopics, afterSiblingId);

            _logger.LogInformation($"MoveTopicToPositionAsync: Calculated target sort order {targetSortOrder} for topic {topicId}");

            // Update topic course and position
            topic.CourseId = targetCourseId;
            topic.SortOrder = targetSortOrder;

            // Renumber all affected topics to prevent collisions
            await RenumberCourseTopicsAsync(courseTopics, topicId, targetSortOrder);

            // Save the moved topic
            _context.Topics.Update(topic);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            _logger.LogInformation($"MoveTopicToPositionAsync: Successfully moved topic {topicId} to position {targetSortOrder}");

            return topic;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private int CalculateTargetSortOrderFromSibling(List<Topic> courseTopics, int afterSiblingId)
    {
        var siblingTopic = courseTopics.FirstOrDefault(t => t.Id == afterSiblingId);
        if (siblingTopic != null)
        {
            // Position after the sibling
            return siblingTopic.SortOrder + 1;
        }

        // Fallback: append to end
        return courseTopics.Any() ? courseTopics.Max(t => t.SortOrder) + 1 : 0;
    }

    public async Task<int> GetMaxSortOrderInCourseAsync(int courseId)
    {
        var maxSortOrder = await _context.Topics
            .Where(t => t.CourseId == courseId && !t.Archived)
            .MaxAsync(t => (int?)t.SortOrder) ?? -1;

        return maxSortOrder;
    }

    private async Task RenumberCourseTopicsAsync(List<Topic> courseTopics, int movedTopicId, int targetSortOrder)
    {
        // Filter out the moved topic
        var otherTopics = courseTopics.Where(t => t.Id != movedTopicId).ToList();

        // Create new clean sequence
        var sortOrder = 0;
        foreach (var topic in otherTopics.OrderBy(t => t.SortOrder))
        {
            // Skip target position for moved topic
            if (sortOrder == targetSortOrder)
            {
                sortOrder++;
            }

            // Only update if sort order actually changes
            if (topic.SortOrder != sortOrder)
            {
                topic.SortOrder = sortOrder;
                _context.Topics.Update(topic);
                _logger.LogDebug($"RenumberCourseTopicsAsync: Updated topic {topic.Id} to sort order {sortOrder}");
            }

            sortOrder++;
        }
    }

    /// <summary>
    /// Get all topics in a course ordered by sort order
    /// </summary>
    public async Task<List<Topic>> GetTopicsByCourseIdAsync(int courseId, bool includeArchived = false)
    {
        _logger.LogInformation($"GetTopicsByCourseIdAsync: Fetching topics for course {courseId}");

        var query = _context.Topics.Where(t => t.CourseId == courseId);

        if (!includeArchived)
        {
            query = query.Where(t => !t.Archived);
        }

        var topics = await query
            .OrderBy(t => t.SortOrder)
            .ToListAsync();

        _logger.LogInformation($"GetTopicsByCourseIdAsync: Found {topics.Count} topics for course {courseId}");
        return topics;
    }

    /// <summary>
    /// Check if a topic belongs to a specific course
    /// </summary>
    public async Task<bool> IsTopicInCourseAsync(int topicId, int courseId)
    {
        return await _context.Topics
            .AnyAsync(t => t.Id == topicId && t.CourseId == courseId && !t.Archived);
    }

    /// <summary>
    /// Get the next available sort order for a course
    /// </summary>
    public async Task<int> GetNextSortOrderForCourseAsync(int courseId)
    {
        var maxSortOrder = await GetMaxSortOrderInCourseAsync(courseId);
        return maxSortOrder + 1;
    }

    /// <summary>
    /// Update multiple topics' sort orders in a single transaction
    /// </summary>
    public async Task UpdateTopicSortOrdersAsync(IEnumerable<Topic> topics)
    {
        foreach (var topic in topics)
        {
            _context.Topics.Update(topic);
        }

        await _context.SaveChangesAsync();
    }
}
using AutoMapper;
using LessonTree.BLL.Service;
using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
using LessonTree.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class TopicService : ITopicService
{
    private readonly ITopicRepository _topicRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ISubTopicRepository _subTopicRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<TopicService> _logger;

    public TopicService(
        ITopicRepository topicRepository,
        ICourseRepository courseRepository,
        ISubTopicRepository subTopicRepository,
        ILessonRepository lessonRepository,
        IMapper mapper,
        ILogger<TopicService> logger)
    {
        _topicRepository = topicRepository;
        _courseRepository = courseRepository;
        _subTopicRepository = subTopicRepository;
        _lessonRepository = lessonRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TopicResource> GetByIdAsync(int id)
    {
        _logger.LogDebug("Fetching topic by ID: {TopicId} in service", id);
        var topic = await _topicRepository.GetByIdAsync(id, q => q
            .Include(t => t.SubTopics)
            .ThenInclude(s => s.Lessons)
            .ThenInclude(l => l.LessonAttachments)
            .ThenInclude(ld => ld.Attachment));

        if (topic == null)
        {
            _logger.LogWarning("Topic with ID {TopicId} not found in service", id);
            return null;
        }

        _logger.LogDebug("Topic with ID {TopicId} found. Title: {Title}, HasSubTopics: {HasSubTopics}, SubTopicCount: {SubTopicCount}",
            topic.Id, topic.Title, topic.HasSubTopics, topic.SubTopics?.Count ?? 0);

        var topicResource = _mapper.Map<TopicResource>(topic);
        _logger.LogDebug("Mapped topic with ID {TopicId} to TopicResource. NodeId: {NodeId}, CourseId: {CourseId}",
            topicResource.Id, topicResource.NodeId, topicResource.CourseId);

        return topicResource;
    }
   
    public async Task<List<TopicResource>> GetAllAsync()
    {
        _logger.LogDebug("Fetching all topics");
        var topics = await _topicRepository.GetAll(q => q
            .Include(t => t.SubTopics).ThenInclude(s => s.Lessons).ThenInclude(l => l.LessonAttachments).ThenInclude(ld => ld.Attachment))
            .ToListAsync();
        return _mapper.Map<List<TopicResource>>(topics ?? new List<Topic>());
    }

    public async Task<int> AddAsync(TopicCreateResource topicCreateResource)
    {
        _logger.LogDebug("Adding topic: {Title}", topicCreateResource.Title);
        var topic = _mapper.Map<Topic>(topicCreateResource);
        var createdTopicId = await _topicRepository.AddAsync(topic);
        var defaultSubTopic = new SubTopic
        {
            Title = "Default SubTopic",
            TopicId = createdTopicId,
            IsDefault = true
        };
        await _subTopicRepository.AddAsync(defaultSubTopic);
        _logger.LogInformation("Topic added with ID: {TopicId}", createdTopicId);
        return createdTopicId;
    }

    public async Task UpdateAsync(TopicUpdateResource topicUpdateResource)
    {
        // Fetch the existing topic with its subtopics and lessons
        var existingTopic = await _topicRepository.GetByIdAsync(topicUpdateResource.Id, q => q
            .Include(t => t.SubTopics)
            .ThenInclude(s => s.Lessons));
        if (existingTopic == null)
        {
            _logger.LogWarning("Topic with ID {TopicId} not found for update", topicUpdateResource.Id);
            throw new ArgumentException("Topic not found");
        }

        // Map the updated values onto the existing topic entity
        _mapper.Map(topicUpdateResource, existingTopic);

        // Check if HasSubTopics is being set to FALSE
        if (existingTopic.HasSubTopics && !topicUpdateResource.HasSubTopics)
        {
            // Find the default subtopic
            var defaultSubTopic = existingTopic.SubTopics.FirstOrDefault(st => st.IsDefault);
            if (defaultSubTopic == null)
            {
                _logger.LogError("Default subtopic not found for topic ID {TopicId}", existingTopic.Id);
                throw new InvalidOperationException("Default subtopic not found");
            }

            // Move all lessons from non-default subtopics to the default subtopic
            var nonDefaultSubTopics = existingTopic.SubTopics.Where(st => !st.IsDefault).ToList();
            foreach (var subTopic in nonDefaultSubTopics)
            {
                foreach (var lesson in subTopic.Lessons)
                {
                    lesson.SubTopicId = defaultSubTopic.Id;
                    await _lessonRepository.UpdateAsync(lesson);
                }
            }

            // Delete empty non-default subtopics
            var emptySubTopics = nonDefaultSubTopics.Where(st => !st.Lessons.Any()).ToList();
            foreach (var emptySubTopic in emptySubTopics)
            {
                await _subTopicRepository.DeleteAsync(emptySubTopic.Id);
            }

            // Update the HasSubTopics property after handling subtopics
            existingTopic.HasSubTopics = false;
        }

        // Persist the updated topic
        await _topicRepository.UpdateAsync(existingTopic);
        _logger.LogInformation("Topic updated with ID: {TopicId}", existingTopic.Id);
    }
    private async Task MoveLessonsToDefaultSubTopicAsync(Topic topic)
    {
        var defaultSubTopic = topic.SubTopics.FirstOrDefault(st => st.IsDefault);
        if (defaultSubTopic == null)
        {
            throw new InvalidOperationException("Default subtopic not found");
        }

        var lessonsToMove = topic.SubTopics
            .Where(st => !st.IsDefault)
            .SelectMany(st => st.Lessons)
            .ToList();

        foreach (var lesson in lessonsToMove)
        {
            lesson.SubTopicId = defaultSubTopic.Id;
            await _lessonRepository.UpdateAsync(lesson);
        }
    }

    public async Task DeleteAsync(int id)
    {
        _logger.LogDebug("Deleting topic with ID: {TopicId} in service", id);
        var topic = await _topicRepository.GetByIdAsync(id);
        if (topic == null)
        {
            _logger.LogWarning("Cannot delete topic with ID {TopicId} because it was not found", id);
            throw new ArgumentException($"Topic with ID {id} not found");
        }

        _logger.LogDebug("Topic with ID {TopicId} found for deletion. Title: {Title}, HasSubTopics: {HasSubTopics}, SubTopicCount: {SubTopicCount}",
            topic.Id, topic.Title, topic.HasSubTopics, topic.SubTopics?.Count ?? 0);

        await _topicRepository.DeleteAsync(id);
        _logger.LogInformation("Topic deleted with ID: {TopicId} in service", id);
    }

    public async Task MoveTopicAsync(int topicId, int newCourseId)
    {
        _logger.LogDebug("Moving Topic ID: {TopicId} to Course ID: {NewCourseId}", topicId, newCourseId);

        var topic = await _topicRepository.GetByIdAsync(topicId, q => q.Include(t => t.SubTopics).ThenInclude(s => s.Lessons));
        if (topic == null)
        {
            _logger.LogError("Topic with ID {TopicId} not found", topicId);
            throw new ArgumentException("Topic not found");
        }

        var newCourse = await _courseRepository.GetByIdAsync(newCourseId);
        if (newCourse == null)
        {
            _logger.LogError("Course with ID {CourseId} not found", newCourseId);
            throw new ArgumentException("Course not found");
        }

        topic.CourseId = newCourseId;
        await _topicRepository.UpdateAsync(topic);
    }

    public async Task<TopicResource> CopyTopicAsync(int topicId, int newCourseId)
    {
        _logger.LogDebug("Copying Topic ID: {TopicId} to Course ID: {NewCourseId}", topicId, newCourseId);

        var originalTopic = await _topicRepository.GetByIdAsync(topicId, q => q
            .Include(t => t.SubTopics).ThenInclude(s => s.Lessons).ThenInclude(l => l.LessonAttachments).ThenInclude(ld => ld.Attachment)
            .Include(t => t.SubTopics).ThenInclude(s => s.Lessons).ThenInclude(l => l.LessonStandards));

        if (originalTopic == null)
        {
            _logger.LogError("Topic with ID {TopicId} not found", topicId);
            throw new ArgumentException("Topic not found");
        }

        var newTopic = new Topic
        {
            Title = originalTopic.Title,
            Description = originalTopic.Description,
            CourseId = newCourseId,
            HasSubTopics = originalTopic.HasSubTopics,
            SubTopics = originalTopic.SubTopics.Select(originalSubTopic => new SubTopic
            {
                Title = originalSubTopic.Title,
                Description = originalSubTopic.Description,
                Lessons = originalSubTopic.Lessons.Select(originalLesson => new Lesson
                {
                    Title = originalLesson.Title,
                    Objective = originalLesson.Objective,
                    LessonAttachments = originalLesson.LessonAttachments.Select(ld => new LessonAttachment
                    {
                        AttachmentId = ld.AttachmentId
                    }).ToList(),
                    LessonStandards = originalLesson.LessonStandards.Select(ls => new LessonStandard
                    {
                        StandardId = ls.StandardId
                    }).ToList()
                }).ToList()
            }).ToList()
        };

        await _topicRepository.AddAsync(newTopic);
        _logger.LogInformation("Copied Topic ID: {OriginalTopicId} to new Topic ID: {NewTopicId} under Course ID: {NewCourseId}",
            topicId, newTopic.Id, newCourseId);

        return _mapper.Map<TopicResource>(newTopic);
    }
}
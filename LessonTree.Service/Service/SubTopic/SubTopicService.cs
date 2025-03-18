using AutoMapper;
using LessonTree.BLL.Service;
using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
using LessonTree.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection.Emit;
using System.Security.AccessControl;

public class SubTopicService : ISubTopicService
{
    private readonly ISubTopicRepository _subTopicRepository;
    private readonly ITopicRepository _topicRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly ILogger<SubTopicService> _logger;
    private readonly IMapper _mapper;

    public SubTopicService(ISubTopicRepository subTopicRepository, ITopicRepository topicRepository,
                           ILessonRepository lessonRepository, ILogger<SubTopicService> logger, IMapper mapper)
    {
        _subTopicRepository = subTopicRepository;
        _topicRepository = topicRepository;
        _lessonRepository = lessonRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<SubTopicResource> GetByIdAsync(int id)
    {
        _logger.LogDebug("Fetching subtopic by ID: {SubTopicId}", id);
        var subTopic = await _subTopicRepository.GetByIdAsync(id, q => q
            .Include(s => s.Lessons).ThenInclude(l => l.LessonAttachments).ThenInclude(ld => ld.Attachment));
        if (subTopic == null)
        {
            _logger.LogWarning("SubTopic with ID {SubTopicId} not found in service", id);
            throw new KeyNotFoundException($"SubTopic with ID {id} not found.");
        }
        if (subTopic.Lessons == null) // Unexpected null due to eager loading
        {
            _logger.LogError("SubTopic ID {SubTopicId} has invalid lesson data.", id);
            throw new InvalidOperationException("SubTopic data is in an invalid state.");
        }
        return _mapper.Map<SubTopicResource>(subTopic);
    }

    public async Task<List<SubTopicResource>> GetAllAsync()
    {
        _logger.LogDebug("Fetching all subtopics");
        try
        {
            var subTopics = await _subTopicRepository.GetAll(q => q
                .Include(s => s.Lessons).ThenInclude(l => l.LessonAttachments).ThenInclude(ld => ld.Attachment))
                .ToListAsync();
            return _mapper.Map<List<SubTopicResource>>(subTopics ?? new List<SubTopic>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve SubTopics.");
            throw new InvalidOperationException("Failed to retrieve SubTopics due to a data access error.", ex);
        }
    }

    public async Task<int> AddAsync(SubTopicCreateResource subTopicCreateResource)
    {
        _logger.LogDebug("Adding subtopic: {Title}", subTopicCreateResource.Title);
        if (string.IsNullOrWhiteSpace(subTopicCreateResource.Title))
        {
            throw new ArgumentException("Title is required", nameof(subTopicCreateResource.Title));
        }
        var topic = await _topicRepository.GetByIdAsync(subTopicCreateResource.TopicId);
        if (topic == null)
        {
            _logger.LogWarning("Topic ID {TopicId} not found for new SubTopic.", subTopicCreateResource.TopicId);
            throw new ArgumentException("The specified Topic does not exist.", nameof(subTopicCreateResource.TopicId));
        }
        var subTopic = _mapper.Map<SubTopic>(subTopicCreateResource);
        var createdId = await _subTopicRepository.AddAsync(subTopic);
        _logger.LogInformation("SubTopic added with ID: {SubTopicId}", createdId);
        return createdId;
    }

    public async Task UpdateAsync(SubTopicUpdateResource subTopicUpdateResource)
    {
        _logger.LogDebug("Attempting to update subtopic: {Title}", subTopicUpdateResource.Title);

        // Fetch the existing SubTopic
        var existingSubTopic = await _subTopicRepository.GetByIdAsync(subTopicUpdateResource.Id);
        if (existingSubTopic == null)
        {
            throw new KeyNotFoundException($"SubTopic with ID {subTopicUpdateResource.Id} not found.");
        }

        // Check if the SubTopic is default
        if (existingSubTopic.IsDefault)
        {
            _logger.LogWarning("Cannot update a default SubTopic. Unable to update default SubTopic with ID {SubTopicId}", existingSubTopic.Id);
            throw new InvalidOperationException("Cannot update a default SubTopic.");
        }

        // Map the DTO onto the existing entity, leaving TopicId unchanged
        _mapper.Map(subTopicUpdateResource, existingSubTopic);
        _logger.LogDebug("Updating subtopic: {Title}", subTopicUpdateResource.Title);
        
        // Persist the changes
        await _subTopicRepository.UpdateAsync(existingSubTopic);
        _logger.LogInformation("SubTopic updated with ID: {SubTopicId}", subTopicUpdateResource.Id);
    }

    public async Task DeleteAsync(int id)
    {
        _logger.LogDebug("Attempting to delete subtopic with ID: {SubTopicId}", id);

        // Fetch the SubTopic to check its properties
        var subTopic = await _subTopicRepository.GetByIdAsync(id);
        if (subTopic == null)
        {
            _logger.LogWarning("SubTopic with ID {SubTopicId} not found for deletion", id);
            throw new KeyNotFoundException($"SubTopic with ID {id} not found.");
        }

        // Check if the SubTopic is default
        if (subTopic.IsDefault)
        {
            _logger.LogWarning("Attempt to delete default SubTopic with ID {SubTopicId}", id);
            throw new InvalidOperationException("Cannot delete a default SubTopic.");
        }
        _logger.LogDebug("Deleting subtopic with ID: {SubTopicId}", id);
        await _subTopicRepository.DeleteAsync(id);
        _logger.LogInformation("SubTopic deleted with ID: {SubTopicId}", id);
    }

    public async Task MoveSubTopic(int subTopicId, int newTopicId)
    {
        _logger.LogDebug("Moving SubTopic ID: {SubTopicId} to Topic ID: {NewTopicId}", subTopicId, newTopicId);
        var subTopic = await _subTopicRepository.GetByIdAsync(subTopicId, q => q.Include(s => s.Lessons));
        if (subTopic == null)
        {
            _logger.LogError("SubTopic with ID {SubTopicId} not found", subTopicId);
            throw new ArgumentException("SubTopic not found");
        }
        if (subTopic.TopicId == newTopicId)
        {
            _logger.LogWarning("SubTopic ID {SubTopicId} is already in Topic ID {TopicId}", subTopicId, newTopicId);
            throw new InvalidOperationException("SubTopic is already in the specified Topic.");
        }
        var newTopic = await _topicRepository.GetByIdAsync(newTopicId, q => q.Include(t => t.SubTopics));
        if (newTopic == null)
        {
            _logger.LogError("Topic with ID {TopicId} not found", newTopicId);
            throw new ArgumentException("Topic not found");
        }

        if (!newTopic.HasSubTopics)
        {
            // Find the default SubTopic of the target Topic
            var defaultSubTopic = newTopic.SubTopics.FirstOrDefault(st => st.IsDefault);
            if (defaultSubTopic == null)
            {
                _logger.LogError("Default SubTopic not found for Topic ID {TopicId}", newTopicId);
                throw new InvalidOperationException("Default SubTopic not found in target Topic");
            }

            _logger.LogDebug("Moving lessons from SubTopic {SubTopicId} to default SubTopic {DefaultSubTopicId}", subTopicId, defaultSubTopic.Id);

            // Create a copy of the Lessons list to avoid modification during iteration
            var lessonsToMove = subTopic.Lessons.ToList();
            foreach (var lesson in lessonsToMove)
            {
                try
                {
                    lesson.SubTopicId = defaultSubTopic.Id;
                    await _lessonRepository.UpdateAsync(lesson);
                    _logger.LogDebug("Updated lesson {LessonId} to SubTopic {SubTopicId}", lesson.Id, defaultSubTopic.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update lesson {LessonId} for SubTopic {SubTopicId}", lesson.Id, defaultSubTopic.Id);
                    throw new InvalidOperationException("Failed to move lessons to the default SubTopic.", ex);
                }
            }

            // Delete the original SubTopic if it’s now empty
            if (!subTopic.Lessons.Any())
            {
                await _subTopicRepository.DeleteAsync(subTopic.Id);
                _logger.LogInformation("Deleted empty SubTopic ID: {SubTopicId}", subTopic.Id);
            }
        }
        else
        {
            subTopic.TopicId = newTopicId;
            await _subTopicRepository.UpdateAsync(subTopic);
            _logger.LogInformation("Moved SubTopic ID: {SubTopicId} to Topic ID: {NewTopicId}", subTopic.Id, newTopicId);
        }
    }

    public async Task<SubTopicResource> CopySubTopicAsync(int subTopicId, int newTopicId)
    {
        _logger.LogDebug("Copying SubTopic ID: {SubTopicId} to Topic ID: {NewTopicId}", subTopicId, newTopicId);
        var originalSubTopic = await _subTopicRepository.GetByIdAsync(subTopicId, q => q
            .Include(s => s.Lessons).ThenInclude(l => l.LessonAttachments).ThenInclude(ld => ld.Attachment)
            .Include(s => s.Lessons).ThenInclude(l => l.LessonStandards));
        if (originalSubTopic == null)
        {
            _logger.LogError("SubTopic with ID {SubTopicId} not found", subTopicId);
            throw new ArgumentException("SubTopic not found");
        }
        var newTopic = await _topicRepository.GetByIdAsync(newTopicId, q => q.Include(t => t.SubTopics));
        if (newTopic == null)
        {
            _logger.LogError("Topic with ID {TopicId} not found", newTopicId);
            throw new ArgumentException("Topic not found");
        }
        if (!newTopic.HasSubTopics)
        {
            var defaultSubTopic = newTopic.SubTopics.FirstOrDefault(st => st.IsDefault);
            if (defaultSubTopic == null)
            {
                _logger.LogError("Default SubTopic not found for Topic ID {TopicId}", newTopicId);
                throw new InvalidOperationException("Default SubTopic not found in target Topic");
            }
            try
            {
                foreach (var originalLesson in originalSubTopic.Lessons)
                {
                    Lesson newLesson = copyLesson(defaultSubTopic.Id, originalLesson);
                    await _lessonRepository.AddAsync(newLesson);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to copy lessons for SubTopic ID {SubTopicId}", subTopicId);
                throw new InvalidOperationException("Failed to copy lessons to the default SubTopic.", ex);
            }
            return _mapper.Map<SubTopicResource>(defaultSubTopic);
        }
        else
        {
            // Create a new SubTopic under the target Topic
            var newSubTopic = new SubTopic
            {
                Title = originalSubTopic.Title,
                Description = originalSubTopic.Description,
                TopicId = newTopicId,
                Lessons = originalSubTopic.Lessons.Select(originalLesson => copyLesson(newTopicId, originalLesson)).ToList()
            };

            await _subTopicRepository.AddAsync(newSubTopic);
            _logger.LogInformation("Copied SubTopic ID: {OriginalSubTopicId} to new SubTopic ID: {NewSubTopicId} under Topic ID: {NewTopicId}",
                subTopicId, newSubTopic.Id, newTopicId);

            return _mapper.Map<SubTopicResource>(newSubTopic);
        }
    }

    private static Lesson copyLesson(int defaultSubTopicId, Lesson originalLesson)
    {
        return new Lesson
        {
            Title = originalLesson.Title,
            Objective = originalLesson.Objective,
            Level = originalLesson.Level,
            Materials = originalLesson.Materials,
            ClassTime = originalLesson.ClassTime,
            Methods = originalLesson.Methods,
            SpecialNeeds = originalLesson.SpecialNeeds,
            Assessment = originalLesson.Assessment,
            SubTopicId = defaultSubTopicId,
            LessonAttachments = originalLesson.LessonAttachments.Select(ld => new LessonAttachment
            {
                AttachmentId = ld.AttachmentId
            }).ToList(),
            LessonStandards = originalLesson.LessonStandards.Select(ls => new LessonStandard
            {
                StandardId = ls.StandardId
            }).ToList()
        };
    }
}
using LessonTree.DAL;
using LessonTree.DAL.Repositories;
using LessonTree.Models.DTO;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using LessonTree.DAL.Domain;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace LessonTree.BLL.Service
{
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

        public SubTopicResource GetById(int id)
        {
            _logger.LogDebug("Fetching subtopic by ID: {SubTopicId}", id);
            var subTopic = _subTopicRepository.GetById(id, q => q
                .Include(s => s.Lessons).ThenInclude(l => l.LessonDocuments).ThenInclude(ld => ld.Document));
            if (subTopic == null)
                _logger.LogWarning("SubTopic with ID {SubTopicId} not found in service", id);
            return _mapper.Map<SubTopicResource>(subTopic ?? new SubTopic());
        }

        public List<SubTopicResource> GetAll()
        {
            _logger.LogDebug("Fetching all subtopics");
            var subTopics = _subTopicRepository.GetAll(q => q
                .Include(s => s.Lessons).ThenInclude(l => l.LessonDocuments).ThenInclude(ld => ld.Document))
                .ToList();
            return _mapper.Map<List<SubTopicResource>>(subTopics ?? new List<SubTopic>());
        }

        public void Add(SubTopicCreateResource subTopicCreateResource)
        {
            _logger.LogDebug("Adding subtopic: {Title}", subTopicCreateResource.Title);
            var subTopic = _mapper.Map<SubTopic>(subTopicCreateResource);
            _subTopicRepository.Add(subTopic);
            _logger.LogInformation("SubTopic added with ID: {SubTopicId}", subTopic.Id);
        }

        public void Update(SubTopicUpdateResource subTopicUpdateResource)
        {
            _logger.LogDebug("Updating subtopic: {Title}", subTopicUpdateResource.Title);
            var subTopic = _mapper.Map<SubTopic>(subTopicUpdateResource);
            _subTopicRepository.Update(subTopic);
            _logger.LogInformation("SubTopic updated with ID: {SubTopicId}", subTopic.Id);
        }

        public void Delete(int id)
        {
            _logger.LogDebug("Deleting subtopic with ID: {SubTopicId}", id);
            _subTopicRepository.Delete(id);
            _logger.LogInformation("SubTopic deleted with ID: {SubTopicId}", id);
        }

        public void MoveSubTopic(int subTopicId, int newTopicId)
        {
            _logger.LogDebug("Moving SubTopic ID: {SubTopicId} to Topic ID: {NewTopicId}", subTopicId, newTopicId);

            var subTopic = _subTopicRepository.GetById(subTopicId);
            if (subTopic == null)
            {
                _logger.LogError("SubTopic with ID {SubTopicId} not found", subTopicId);
                throw new ArgumentException("SubTopic not found");
            }

            var newTopic = _topicRepository.GetById(newTopicId);
            if (newTopic == null)
            {
                _logger.LogError("Topic with ID {TopicId} not found", newTopicId);
                throw new ArgumentException("Topic not found");
            }

            subTopic.TopicId = newTopicId;
            _subTopicRepository.Update(subTopic);
        }

        public SubTopicResource CopySubTopic(int subTopicId, int newTopicId)
        {
            _logger.LogDebug("Copying SubTopic ID: {SubTopicId} to Topic ID: {NewTopicId}", subTopicId, newTopicId);

            var originalSubTopic = _subTopicRepository.GetById(subTopicId, q => q
                .Include(s => s.Lessons).ThenInclude(l => l.LessonDocuments).ThenInclude(ld => ld.Document)
                .Include(s => s.Lessons).ThenInclude(l => l.LessonStandards));

            if (originalSubTopic == null)
            {
                _logger.LogError("SubTopic with ID {SubTopicId} not found", subTopicId);
                throw new ArgumentException("SubTopic not found");
            }

            var newSubTopic = new SubTopic
            {
                Title = originalSubTopic.Title,
                Description = originalSubTopic.Description,
                TopicId = newTopicId,
                Lessons = originalSubTopic.Lessons.Select(originalLesson => new Lesson
                {
                    Title = originalLesson.Title,
                    Content = originalLesson.Content,
                    LessonDocuments = originalLesson.LessonDocuments.Select(ld => new LessonDocument
                    {
                        DocumentId = ld.DocumentId
                    }).ToList(),
                    LessonStandards = originalLesson.LessonStandards.Select(ls => new LessonStandard
                    {
                        StandardId = ls.StandardId
                    }).ToList()
                }).ToList()
            };

            _subTopicRepository.Add(newSubTopic);
            _logger.LogInformation("Copied SubTopic ID: {OriginalSubTopicId} to new SubTopic ID: {NewSubTopicId} under Topic ID: {NewTopicId}",
                subTopicId, newSubTopic.Id, newTopicId);

            return _mapper.Map<SubTopicResource>(newSubTopic); // Return the newly created SubTopic
        }
    }
}
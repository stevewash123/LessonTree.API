using LessonTree.DAL.Repositories;
using LessonTree.Models.DTO;
using Microsoft.Extensions.Logging;
using LessonTree.DAL.Domain;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace LessonTree.BLL.Service
{
    public class TopicService : ITopicService
    {
        private readonly ITopicRepository _topicRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ILogger<TopicService> _logger;
        private readonly IMapper _mapper;

        public TopicService(ITopicRepository topicRepository, ICourseRepository courseRepository, ILogger<TopicService> logger, IMapper mapper)
        {
            _topicRepository = topicRepository;
            _courseRepository = courseRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public TopicResource GetById(int id)
        {
            _logger.LogDebug("Fetching topic by ID: {TopicId}", id);
            var topic = _topicRepository.GetById(id, q => q
                .Include(t => t.SubTopics).ThenInclude(s => s.Lessons).ThenInclude(l => l.LessonDocuments).ThenInclude(ld => ld.Document));
            if (topic == null)
                _logger.LogWarning("Topic with ID {TopicId} not found in service", id);
            return _mapper.Map<TopicResource>(topic ?? new Topic());
        }

        public List<TopicResource> GetAll()
        {
            _logger.LogDebug("Fetching all topics");
            var topics = _topicRepository.GetAll(q => q
                .Include(t => t.SubTopics).ThenInclude(s => s.Lessons).ThenInclude(l => l.LessonDocuments).ThenInclude(ld => ld.Document))
                .ToList();
            return _mapper.Map<List<TopicResource>>(topics ?? new List<Topic>());
        }

        public void Add(TopicCreateResource topicCreateResource)
        {
            _logger.LogDebug("Adding topic: {Title}", topicCreateResource.Title);
            var topic = _mapper.Map<Topic>(topicCreateResource);
            _topicRepository.Add(topic);
            _logger.LogInformation("Topic added with ID: {TopicId}", topic.Id);
        }

        public void Update(TopicUpdateResource topicUpdateResource)
        {
            _logger.LogDebug("Updating topic: {Title}", topicUpdateResource.Title);
            var topic = _mapper.Map<Topic>(topicUpdateResource);
            _topicRepository.Update(topic);
            _logger.LogInformation("Topic updated with ID: {TopicId}", topic.Id);
        }

        public void Delete(int id)
        {
            _logger.LogDebug("Deleting topic with ID: {TopicId}", id);
            _topicRepository.Delete(id);
            _logger.LogInformation("Topic deleted with ID: {TopicId}", id);
        }

        public void MoveTopic(int topicId, int newCourseId)
        {
            _logger.LogDebug("Moving Topic ID: {TopicId} to Course ID: {NewCourseId}", topicId, newCourseId);

            var topic = _topicRepository.GetById(topicId, q => q.Include(t => t.SubTopics).ThenInclude(s => s.Lessons));
            if (topic == null)
            {
                _logger.LogError("Topic with ID {TopicId} not found", topicId);
                throw new ArgumentException("Topic not found");
            }

            var newCourse = _courseRepository.GetById(newCourseId);
            if (newCourse == null)
            {
                _logger.LogError("Course with ID {CourseId} not found", newCourseId);
                throw new ArgumentException("Course not found");
            }

            topic.CourseId = newCourseId;
            _topicRepository.Update(topic);
        }

        public TopicResource CopyTopic(int topicId, int newCourseId)
        {
            _logger.LogDebug("Copying Topic ID: {TopicId} to Course ID: {NewCourseId}", topicId, newCourseId);

            var originalTopic = _topicRepository.GetById(topicId, q => q
                .Include(t => t.SubTopics).ThenInclude(s => s.Lessons).ThenInclude(l => l.LessonDocuments).ThenInclude(ld => ld.Document)
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
                SubTopics = originalTopic.SubTopics.Select(originalSubTopic => new SubTopic
                {
                    Title = originalSubTopic.Title,
                    Description = originalSubTopic.Description,
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
                }).ToList()
            };

            _topicRepository.Add(newTopic);
            _logger.LogInformation("Copied Topic ID: {OriginalTopicId} to new Topic ID: {NewTopicId} under Course ID: {NewCourseId}",
                topicId, newTopic.Id, newCourseId);

            return _mapper.Map<TopicResource>(newTopic); // Return the newly created Topic
        }
    }
}
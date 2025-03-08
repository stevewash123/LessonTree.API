using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
using LessonTree.Models.DTO;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using LessonTree.DAL;

namespace LessonTree.BLL.Service
{
    public class LessonService : ILessonService
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly ISubTopicRepository _subTopicRepository;
        private readonly IStandardRepository _standardRepository;
        private readonly ILogger<LessonService> _logger;
        private readonly IMapper _mapper;

        public LessonService(ILessonRepository lessonRepository, ISubTopicRepository subTopicRepository, IStandardRepository standardRepository, ILogger<LessonService> logger, IMapper mapper)
        {
            _lessonRepository = lessonRepository;
            _subTopicRepository = subTopicRepository;
            _standardRepository = standardRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public LessonDetailResource GetById(int id)
        {
            _logger.LogDebug($"Fetching lesson with ID {id}");
            var lesson = _lessonRepository.GetAll(q => q
                .Include(l => l.SubTopic)
                .Include(l => l.LessonDocuments).ThenInclude(ld => ld.Document)
                .Include(l => l.LessonStandards).ThenInclude(ls => ls.Standard))
                .FirstOrDefault(l => l.Id == id);
            if (lesson == null)
            {
                _logger.LogWarning($"Lesson with ID {id} not found");
                return null;
            }
            return _mapper.Map<LessonDetailResource>(lesson);
        }

        public List<LessonResource> GetAll()
        {
            _logger.LogDebug("Fetching all lessons");
            var lessons = _lessonRepository.GetAll()
                .Select(l => new LessonResource
                {
                    Id = l.Id,
                    NodeId = $"lesson_{l.Id}",
                    Title = l.Title,
                    Content = l.Content
                })
                .ToList();
            return lessons;
        }

        public void Add(LessonCreateResource lessonCreateResource)
        {
            _logger.LogDebug("Adding lesson: {Title}", lessonCreateResource.Title);
            var lesson = _mapper.Map<Lesson>(lessonCreateResource);
            _lessonRepository.Add(lesson);
        }

        public void Update(LessonUpdateResource lessonUpdateResource)
        {
            _logger.LogDebug("Updating lesson: {Title}", lessonUpdateResource.Title);
            var lesson = _mapper.Map<Lesson>(lessonUpdateResource);
            _lessonRepository.Update(lesson);
        }

        public void Delete(int id)
        {
            _logger.LogDebug("Deleting lesson with ID: {LessonId}", id);
            _lessonRepository.Delete(id);
        }

        public void AddDocument(int lessonId, int documentId)
        {
            _logger.LogDebug("Adding document ID: {DocumentId} to Lesson ID: {LessonId}", documentId, lessonId);
            try
            {
                _lessonRepository.AddDocument(lessonId, documentId);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Failed to add document to Lesson ID: {LessonId}", lessonId);
                throw;
            }
        }

        public void RemoveDocument(int lessonId, int documentId)
        {
            _logger.LogDebug("Removing document ID: {DocumentId} from Lesson ID: {LessonId}", documentId, lessonId);
            _lessonRepository.RemoveDocument(lessonId, documentId);
        }

        public List<LessonResource> GetByTitle(string title)
        {
            _logger.LogDebug("Fetching lessons by title: {Title}", title);
            var lessons = _lessonRepository.GetByTitle(title).ToList();
            return _mapper.Map<List<LessonResource>>(lessons ?? new List<Lesson>());
        }

        public void MoveLesson(int lessonId, int newSubTopicId)
        {
            _logger.LogDebug("Moving Lesson ID: {LessonId} to SubTopic ID: {NewSubTopicId}", lessonId, newSubTopicId);

            var lesson = _lessonRepository.GetById(lessonId, q => q.Include(l => l.SubTopic));
            if (lesson == null)
            {
                _logger.LogError("Lesson with ID {LessonId} not found", lessonId);
                throw new ArgumentException("Lesson not found");
            }

            var newSubTopic = _subTopicRepository.GetById(newSubTopicId);
            if (newSubTopic == null)
            {
                _logger.LogError("SubTopic with ID {SubTopicId} not found", newSubTopicId);
                throw new ArgumentException("SubTopic not found");
            }

            lesson.SubTopicId = newSubTopicId;
            _lessonRepository.Update(lesson);
        }

        public void AddStandardToLesson(int lessonId, int standardId)
        {
            var lesson = _lessonRepository.GetById(lessonId);
            if (lesson == null)
            {
                throw new ArgumentException("Lesson not found");
            }

            var standard = _standardRepository.GetById(standardId);
            if (standard == null)
            {
                throw new ArgumentException("Standard not found");
            }

            if (!lesson.LessonStandards.Any(ls => ls.StandardId == standardId))
            {
                lesson.LessonStandards.Add(new LessonStandard { LessonId = lessonId, StandardId = standardId });
                _lessonRepository.Update(lesson);
            }
        }

        public void RemoveStandardFromLesson(int lessonId, int standardId)
        {
            var lesson = _lessonRepository.GetById(lessonId);
            if (lesson == null)
            {
                throw new ArgumentException("Lesson not found");
            }

            var lessonStandard = lesson.LessonStandards.FirstOrDefault(ls => ls.StandardId == standardId);
            if (lessonStandard != null)
            {
                lesson.LessonStandards.Remove(lessonStandard);
                _lessonRepository.Update(lesson);
            }
        }

        public LessonResource CopyLesson(int lessonId, int newSubTopicId)
        {
            _logger.LogDebug("Copying Lesson ID: {LessonId} to SubTopic ID: {NewSubTopicId}", lessonId, newSubTopicId);

            var originalLesson = _lessonRepository.GetById(lessonId, q => q
                .Include(l => l.LessonDocuments).ThenInclude(ld => ld.Document)
                .Include(l => l.LessonStandards));

            if (originalLesson == null)
            {
                _logger.LogError("Lesson with ID {LessonId} not found", lessonId);
                throw new ArgumentException("Lesson not found");
            }

            var newLesson = new Lesson
            {
                Title = originalLesson.Title,
                Content = originalLesson.Content,
                SubTopicId = newSubTopicId,
                LessonDocuments = originalLesson.LessonDocuments.Select(ld => new LessonDocument
                {
                    DocumentId = ld.DocumentId
                }).ToList(),
                LessonStandards = originalLesson.LessonStandards.Select(ls => new LessonStandard
                {
                    StandardId = ls.StandardId
                }).ToList()
            };

            _lessonRepository.Add(newLesson);
            _logger.LogInformation("Copied Lesson ID: {OriginalLessonId} to new Lesson ID: {NewLessonId} under SubTopic ID: {NewSubTopicId}",
                lessonId, newLesson.Id, newSubTopicId);

            return _mapper.Map<LessonResource>(newLesson); 
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LessonTree.DAL
{
    public class LessonRepository : ILessonRepository
    {
        private readonly LessonTreeContext _context;
        private readonly ILogger<LessonRepository> _logger;

        public LessonRepository(LessonTreeContext context, ILogger<LessonRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IQueryable<Lesson> GetAll(Func<IQueryable<Lesson>, IQueryable<Lesson>> include = null)
        {
            _logger.LogDebug("Retrieving all lessons");
            var query = _context.Lessons.AsQueryable();
            if (include != null)
            {
                query = include(query);
            }
            return query;
        }

        public Lesson GetById(int id, Func<IQueryable<Lesson>, IQueryable<Lesson>> include = null)
        {
            _logger.LogDebug("Retrieving lesson by ID: {LessonId}", id);
            IQueryable<Lesson> query = _context.Lessons;
            if (include != null)
            {
                query = include(query);
            }
            else
            {
                query = query
                    .Include(l => l.LessonDocuments).ThenInclude(ld => ld.Document)
                    .Include(l => l.SubTopic)
                    .Include(l => l.LessonStandards).ThenInclude(ls => ls.Standard);
            }
            var lesson = query.FirstOrDefault(l => l.Id == id);
            if (lesson == null)
                _logger.LogWarning("Lesson with ID {LessonId} not found in database", id);
            return lesson ?? new Lesson();
        }

        public void Add(Lesson lesson)
        {
            _logger.LogDebug("Adding lesson to database: {Title}", lesson.Title);
            _context.Lessons.Add(lesson);
            _context.SaveChanges();
        }

        public void Update(Lesson lesson)
        {
            _logger.LogDebug("Updating lesson in database: {Title}", lesson.Title);
            _context.Lessons.Update(lesson);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            _logger.LogDebug("Deleting lesson with ID: {LessonId}", id);
            var lesson = _context.Lessons.Find(id);
            if (lesson != null)
            {
                _context.Lessons.Remove(lesson);
                _context.SaveChanges();
            }
            else
            {
                _logger.LogWarning("Lesson with ID {LessonId} not found for deletion", id);
            }
        }

        public void AddDocument(int lessonId, int documentId)
        {
            _logger.LogDebug("Adding document ID: {DocumentId} to Lesson ID: {LessonId}", documentId, lessonId);
            var lesson = GetById(lessonId);
            if (lesson == null)
            {
                _logger.LogError("Lesson with ID {LessonId} not found for document addition", lessonId);
                throw new ArgumentException("Lesson not found");
            }
            var document = _context.Documents.Find(documentId);
            if (document == null)
            {
                _logger.LogError("Document with ID {DocumentId} not found", documentId);
                throw new ArgumentException("Document not found");
            }
            var lessonDocument = new LessonDocument { LessonId = lessonId, DocumentId = documentId };
            _context.LessonDocuments.Add(lessonDocument);
            _context.SaveChanges();
            _logger.LogInformation("Document ID: {DocumentId} added to Lesson ID: {LessonId}", documentId, lessonId);
        }

        public void RemoveDocument(int lessonId, int documentId)
        {
            _logger.LogDebug("Removing document ID: {DocumentId} from Lesson ID: {LessonId}", documentId, lessonId);
            var lessonDocument = _context.LessonDocuments
                .FirstOrDefault(ld => ld.LessonId == lessonId && ld.DocumentId == documentId);
            if (lessonDocument == null)
            {
                _logger.LogError("Document with ID {DocumentId} not found in Lesson ID: {LessonId}", documentId, lessonId);
                throw new ArgumentException("Document not found in lesson");
            }
            _context.LessonDocuments.Remove(lessonDocument);
            _context.SaveChanges();
            _logger.LogInformation("Document ID: {DocumentId} removed from Lesson ID: {LessonId}", documentId, lessonId);
        }

        public List<Lesson> GetByTitle(string title)
        {
            _logger.LogDebug("Retrieving lessons by title: {Title}", title);
            var lessons = _context.Lessons
                .Include(l => l.LessonDocuments).ThenInclude(ld => ld.Document)
                .Include(l => l.SubTopic)
                .Where(l => l.Title.Contains(title))
                .ToList();
            _logger.LogDebug("Found {Count} lessons with title containing: {Title}", lessons.Count, title);
            return lessons ?? new List<Lesson>();
        }
    }
}
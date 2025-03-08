using LessonTree.DAL.Domain;
using Microsoft.Extensions.Logging;

namespace LessonTree.DAL.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly LessonTreeContext _context;
        private readonly ILogger<DocumentRepository> _logger;

        public DocumentRepository(LessonTreeContext context, ILogger<DocumentRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void Add(Document document)
        {
            _logger.LogDebug("Adding document to database: {FileName}", document.FileName);
            _context.Documents.Add(document);
            _context.SaveChanges();
        }
    }
}
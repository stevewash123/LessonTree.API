using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
using Microsoft.Extensions.Logging;

namespace LessonTree.BLL.Service
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly ILogger<DocumentService> _logger;

        public DocumentService(IDocumentRepository documentRepository, ILogger<DocumentService> logger)
        {
            _documentRepository = documentRepository;
            _logger = logger;
        }

        public int CreateDocument(Document document)
        {
            _logger.LogDebug("Creating document: {FileName}", document.FileName);
            _documentRepository.Add(document);
            return document.Id;
        }
    }
}
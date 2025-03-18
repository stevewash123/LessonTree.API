using LessonTree.DAL.Domain;
using Microsoft.Extensions.Logging;

namespace LessonTree.DAL.Repositories
{
    public class AttachmentRepository : IAttachmentRepository
    {
        private readonly LessonTreeContext _context;
        private readonly ILogger<AttachmentRepository> _logger;

        public AttachmentRepository(LessonTreeContext context, ILogger<AttachmentRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void Add(Attachment document)
        {
            _logger.LogDebug("Adding document to database: {FileName}", document.FileName);
            _context.Attachments.Add(document);
            _context.SaveChanges();
        }
    }
}
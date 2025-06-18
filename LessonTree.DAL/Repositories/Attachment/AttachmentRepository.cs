using LessonTree.DAL.Domain;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

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

        public async Task<int> AddAsync(Attachment attachment)
        {
            _logger.LogDebug("Adding attachment to database: {FileName}", attachment.FileName);
            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Attachment added with ID: {AttachmentId}, FileName: {FileName}", attachment.Id, attachment.FileName);
            return attachment.Id;
        }
    }
}
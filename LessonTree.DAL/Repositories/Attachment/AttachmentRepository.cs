// **COMPLETE FILE** - AttachmentRepository.cs - Standardized to enterprise patterns
// RESPONSIBILITY: Attachment data access for file blob management and metadata storage
// DOES NOT: Handle file validation, processing, or Google Drive integration (that's in services)
// CALLED BY: AttachmentService for all attachment operations

using LessonTree.DAL.Domain;
using Microsoft.EntityFrameworkCore;
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

        public async Task<Attachment?> GetByIdAsync(int id)
        {
            _logger.LogInformation($"GetByIdAsync: Fetching attachment {id}");

            var attachment = await _context.Attachments.FirstOrDefaultAsync(a => a.Id == id);

            if (attachment != null)
            {
                _logger.LogInformation($"GetByIdAsync: Found attachment {id} - {attachment.FileName}");
            }
            else
            {
                _logger.LogInformation($"GetByIdAsync: Attachment {id} not found");
            }

            return attachment;
        }

        public async Task<int> AddAsync(Attachment attachment)
        {
            _logger.LogInformation($"AddAsync: Creating attachment '{attachment.FileName}' (Type: {attachment.Type})");

            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"AddAsync: Created attachment {attachment.Id} - {attachment.FileName}");
            return attachment.Id;
        }

        public async Task UpdateAsync(Attachment attachment)
        {
            _logger.LogInformation($"UpdateAsync: Updating attachment {attachment.Id}");

            _context.Attachments.Update(attachment);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"UpdateAsync: Updated attachment {attachment.Id}");
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation($"DeleteAsync: Deleting attachment {id}");

            var attachment = await _context.Attachments.FindAsync(id);
            if (attachment == null)
            {
                throw new ArgumentException($"Attachment {id} not found");
            }

            _context.Attachments.Remove(attachment);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"DeleteAsync: Deleted attachment {id}");
        }
    }
}
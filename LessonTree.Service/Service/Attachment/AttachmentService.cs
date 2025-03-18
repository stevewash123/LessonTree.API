using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
using Microsoft.Extensions.Logging;

namespace LessonTree.BLL.Service
{
    public class AttachmentService : IAttachmentService
    {
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly ILogger<AttachmentService> _logger;

        public AttachmentService(IAttachmentRepository attachmentRepository, ILogger<AttachmentService> logger)
        {
            _attachmentRepository = attachmentRepository;
            _logger = logger;
        }

        public int CreateAttachment(Attachment attachment)
        {
            _logger.LogDebug("Creating attachment: {FileName}", attachment.FileName);
            _attachmentRepository.Add(attachment);
            return attachment.Id;
        }
    }
}
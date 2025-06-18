using LessonTree.BLL.Service;
using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
using Microsoft.Extensions.Logging;

public class AttachmentService : IAttachmentService
{
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly ILogger<AttachmentService> _logger;

    public AttachmentService(IAttachmentRepository attachmentRepository, ILogger<AttachmentService> logger)
    {
        _attachmentRepository = attachmentRepository;
        _logger = logger;
    }

    public async Task<int> CreateAttachmentAsync(Attachment attachment)
    {
        _logger.LogDebug("Creating attachment: {FileName}", attachment.FileName);
        int attachmentId = await _attachmentRepository.AddAsync(attachment);
        _logger.LogInformation("Attachment created with ID: {AttachmentId}", attachmentId);
        return attachmentId;
    }
}
using LessonTree.Models.DTO;
using LessonTree.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LessonTree.BLL.Service
{
    public interface ILessonService
    {
        // Query operations - return DTOs only, all require userId for ownership validation
        Task<LessonDetailResource?> GetByIdAsync(int id, int userId);
        Task<List<LessonResource>> GetAllAsync(int userId, ArchiveFilter filter = ArchiveFilter.Active);
        Task<List<LessonResource>> GetLessonsBySubtopic(int subTopicId, int userId, ArchiveFilter filter = ArchiveFilter.Active);
        Task<List<LessonResource>> GetLessonsByTopic(int topicId, int userId, ArchiveFilter filter = ArchiveFilter.Active);

        // CRUD operations - all include userId for ownership validation
        Task<int> AddAsync(LessonCreateResource lessonCreateResource, int userId);
        Task<LessonDetailResource> UpdateAsync(LessonUpdateResource lessonUpdateResource, int userId);
        Task DeleteAsync(int id, int userId);

        // Attachment operations - all include userId for ownership validation
        Task AddAttachmentAsync(int lessonId, int attachmentId, int userId);
        Task RemoveAttachmentAsync(int lessonId, int attachmentId, int userId);

        // Move/Copy operations - all include userId for ownership validation
        Task MoveLessonAsync(int lessonId, int? newSubTopicId, int? newTopicId, int userId);
        Task<LessonResource> CopyLessonAsync(int lessonId, int? newSubTopicId, int? newTopicId, int userId);

        // Standards operations - all include userId for ownership validation
        Task AddStandardToLessonAsync(int lessonId, int standardId, int userId);
        Task RemoveStandardFromLessonAsync(int lessonId, int standardId, int userId);

        // Sort order operations - all include userId for ownership validation
        Task UpdateSortOrderAsync(int lessonId, int sortOrder, int userId);
    }
}
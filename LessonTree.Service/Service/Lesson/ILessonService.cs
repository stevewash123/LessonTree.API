using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;
using LessonTree.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LessonTree.BLL.Service
{
    public interface ILessonService
    {
        Task<LessonDetailResource?> GetByIdAsync(int id);
        Task<Lesson?> GetDomainLessonByIdAsync(int id);
        Task<List<LessonResource>> GetAllAsync(int? userId = null, ArchiveFilter filter = ArchiveFilter.Active);
        Task<List<LessonResource>> GetLessonsBySubtopic(int subTopicId, int? userId = null, ArchiveFilter filter = ArchiveFilter.Active); 
        Task<List<LessonResource>> GetLessonsByTopic(int topicId, int? userId = null, ArchiveFilter filter = ArchiveFilter.Active);
        Task<int> AddAsync(LessonCreateResource lessonCreateResource, int userId);
        Task UpdateAsync(LessonUpdateResource lessonUpdateResource);
        Task DeleteAsync(int id);
        Task AddAttachmentAsync(int lessonId, int attachmentId);
        Task RemoveAttachmentAsync(int lessonId, int attachmentId);
        Task MoveLessonAsync(int lessonId, int? newSubTopicId, int? newTopicId);
        Task AddStandardToLessonAsync(int lessonId, int standardId);
        Task RemoveStandardFromLessonAsync(int lessonId, int standardId);
        Task<LessonResource> CopyLessonAsync(int lessonId, int? newSubTopicId, int? newTopicId, int userId); // Updated
    }
}
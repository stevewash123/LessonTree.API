using System.Collections.Generic;
using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;

namespace LessonTree.BLL.Service
{
    public interface ILessonService
    {
        Task<LessonDetailResource?> GetByIdAsync(int id);
        Task<List<LessonResource>> GetAllAsync();
        Task<int> AddAsync(LessonCreateResource lessonCreateResource); // Updated
        Task UpdateAsync(LessonUpdateResource lessonUpdateResource);
        Task DeleteAsync(int id);
        Task AddAttachmentAsync(int lessonId, int attachmentId);
        Task RemoveAttachmentAsync(int lessonId, int attachmentId);
        Task<List<LessonResource>> GetByTitleAsync(string title);
        Task MoveLessonAsync(int lessonId, int newSubTopicId);
        Task AddStandardToLessonAsync(int lessonId, int standardId);
        Task RemoveStandardFromLessonAsync(int lessonId, int standardId);
        Task<LessonResource> CopyLessonAsync(int lessonId, int newSubTopicId);
    }
}
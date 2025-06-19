using LessonTree.Models.DTO;
using LessonTree.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LessonTree.BLL.Service
{
    public interface ITopicService
    {
        // Query operations - return DTOs only
        Task<TopicResource> GetByIdAsync(int id, int userId);
        Task<List<TopicResource>> GetAllAsync(int userId, ArchiveFilter filter = ArchiveFilter.Active);
        Task<List<TopicResource>> GetTopicsByCourseAsync(int courseId, int userId, ArchiveFilter filter = ArchiveFilter.Active);

        // CRUD operations - all include userId for ownership validation
        Task<int> AddAsync(TopicCreateResource topicCreateResource, int userId);
        Task<TopicResource> UpdateAsync(TopicUpdateResource topicUpdateResource, int userId);
        Task DeleteAsync(int id, int userId); // ADDED userId parameter

        // Operations - all include userId for ownership validation
        Task MoveTopicAsync(int topicId, int newCourseId, int userId);
        Task<TopicResource> CopyTopicAsync(int topicId, int newCourseId, int userId);
        Task UpdateSortOrderAsync(int topicId, int sortOrder); 

        // REMOVED: Task<Topic?> GetDomainTopicByIdAsync(int id) - No domain object exposure
    }
}
using LessonTree.Models.DTO;
using LessonTree.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LessonTree.BLL.Service
{
    public interface ISubTopicService
    {
        // Query operations - return DTOs only
        Task<SubTopicResource> GetByIdAsync(int id, int userId);
        Task<List<SubTopicResource>> GetAllAsync(int userId, ArchiveFilter filter = ArchiveFilter.Active);
        Task<List<SubTopicResource>> GetSubtopicsByTopicIdAsync(int topicId, int userId, ArchiveFilter filter = ArchiveFilter.Active);

        // CRUD operations - all include userId for ownership validation
        Task<int> AddAsync(SubTopicCreateResource subTopicCreateResource, int userId);
        Task<SubTopicResource> UpdateAsync(SubTopicUpdateResource subTopicUpdateResource, int userId);
        Task DeleteAsync(int id, int userId); // ADDED userId parameter

        // Operations - all include userId for ownership validation  
        Task MoveSubTopic(int subTopicId, int newTopicId, int userId); // ADDED userId parameter
        Task<SubTopicResource> CopySubTopicAsync(int subTopicId, int newTopicId, int userId);
        Task UpdateSortOrderAsync(int subTopicId, int sortOrder, int userId); // ADDED userId parameter

        // REMOVED: Task<SubTopic?> GetDomainSubTopicByIdAsync(int id) - No domain object exposure
    }
}
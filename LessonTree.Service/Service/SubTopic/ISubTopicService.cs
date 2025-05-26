using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;
using LessonTree.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LessonTree.BLL.Service
{
    public interface ISubTopicService
    {
        Task<SubTopicResource> GetByIdAsync(int id, int userId);
        Task<SubTopic?> GetDomainSubTopicByIdAsync(int id);
        Task<List<SubTopicResource>> GetAllAsync(int userId, ArchiveFilter filter = ArchiveFilter.Active);
        Task<List<SubTopicResource>> GetSubtopicsByTopicIdAsync(int topicId, int userId, ArchiveFilter filter = ArchiveFilter.Active);
        Task<int> AddAsync(SubTopicCreateResource subTopicCreateResource, int userId);
        Task<SubTopicResource> UpdateAsync(SubTopicUpdateResource subTopicUpdateResource, int userId);
        Task DeleteAsync(int id);
        Task MoveSubTopic(int subTopicId, int newTopicId);
        Task<SubTopicResource> CopySubTopicAsync(int subTopicId, int newTopicId, int userId); // Updated
        Task UpdateSortOrderAsync(int subTopicId, int sortOrder);
    }
}
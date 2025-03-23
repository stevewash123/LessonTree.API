using System.Collections.Generic;
using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;

namespace LessonTree.BLL.Service
{
    public interface ISubTopicService
    {
        Task<SubTopicResource> GetByIdAsync(int id);
        Task<List<SubTopicResource>> GetAllAsync();
        Task<List<SubTopicResource>> GetSubtopicsByTopicIdAsync(int topicId);
        Task<int> AddAsync(SubTopicCreateResource subTopicCreateResource);
        Task UpdateAsync(SubTopicUpdateResource subTopicUpdateResource);
        Task DeleteAsync(int id);
        Task MoveSubTopic(int subTopicId, int newTopicId);
        Task<SubTopicResource> CopySubTopicAsync(int subTopicId, int newTopicId);
    }
}
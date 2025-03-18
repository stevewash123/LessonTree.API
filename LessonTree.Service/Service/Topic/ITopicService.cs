using System.Collections.Generic;
using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;

namespace LessonTree.BLL.Service
{
    public interface ITopicService
    {
        Task<TopicResource> GetByIdAsync(int id);
        Task<List<TopicResource>> GetAllAsync();
        Task<int> AddAsync(TopicCreateResource topicCreateResource); // Changed return type to Task<int>
        Task UpdateAsync(TopicUpdateResource topicUpdateResource);
        Task DeleteAsync(int id);
        Task MoveTopicAsync(int topicId, int newCourseId);
        Task<TopicResource> CopyTopicAsync(int topicId, int newCourseId);
    }
}
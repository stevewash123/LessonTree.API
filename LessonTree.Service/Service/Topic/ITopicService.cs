using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;
using LessonTree.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LessonTree.BLL.Service
{
    public interface ITopicService
    {
        Task<TopicResource> GetByIdAsync(int id, int userId);
        Task<Topic?> GetDomainTopicByIdAsync(int id);
        Task<List<TopicResource>> GetAllAsync(int userId, ArchiveFilter filter = ArchiveFilter.Active);
        Task<List<TopicResource>> GetTopicsByCourseAsync(int courseId, int userId, ArchiveFilter filter = ArchiveFilter.Active);
        Task<int> AddAsync(TopicCreateResource topicCreateResource, int userId);
        Task UpdateAsync(TopicUpdateResource topicUpdateResource);
        Task DeleteAsync(int id);
        Task MoveTopicAsync(int topicId, int newCourseId, int userId);
        Task<TopicResource> CopyTopicAsync(int topicId, int newCourseId, int userId);
    }
}
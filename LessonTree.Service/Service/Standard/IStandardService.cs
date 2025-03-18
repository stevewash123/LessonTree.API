using System.Collections.Generic;
using System.Threading.Tasks;
using LessonTree.Models.DTO;

namespace LessonTree.BLL.Service
{
    public interface IStandardService
    {
        Task<List<StandardResource>> GetAllAsync();
        Task<StandardResource?> GetByIdAsync(int id);
        Task<int> AddAsync(StandardCreateResource standardCreateResource); 
        Task UpdateAsync(StandardUpdateResource standardUpdateResource);
        Task DeleteAsync(int id);
        Task<List<StandardResource>> GetByTopicIdAsync(int topicId);
    }
}
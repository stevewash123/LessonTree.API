using System.Linq;
using System.Threading.Tasks;
using LessonTree.DAL.Domain;

namespace LessonTree.DAL.Repositories
{
    public interface IStandardRepository
    {
        IQueryable<Standard> GetAll();
        Task<Standard?> GetByIdAsync(int id);
        Task<int> AddAsync(Standard standard); // Updated to return ID
        Task UpdateAsync(Standard standard);
        Task DeleteAsync(int id);
        IQueryable<Standard> GetByTopicId(int topicId); 
        IQueryable<Standard> GetByCourseId(int courseId);
    }
}
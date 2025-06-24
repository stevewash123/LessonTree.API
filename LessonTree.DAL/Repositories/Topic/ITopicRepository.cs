using LessonTree.DAL.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LessonTree.DAL.Repositories
{
    public interface ITopicRepository
    {
        IQueryable<Topic> GetAll(Func<IQueryable<Topic>, IQueryable<Topic>> include = null);
        Task<Topic> GetByIdAsync(int id, Func<IQueryable<Topic>, IQueryable<Topic>> include = null);
        Task<int> AddAsync(Topic topic); // Changed return type to Task<int>
        Task UpdateAsync(Topic topic);
        Task DeleteAsync(int id);
        Task<Topic> MoveTopicToPositionAsync(int topicId, int targetCourseId, int relativeTopicId, string position);
        Task<int> GetMaxSortOrderInCourseAsync(int courseId);

    }
}

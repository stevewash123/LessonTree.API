using LessonTree.DAL.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LessonTree.DAL.Repositories
{
    public interface ISubTopicRepository
    {
        Task<SubTopic> GetByIdAsync(int id, Func<IQueryable<SubTopic>, IQueryable<SubTopic>> include = null);
        IQueryable<SubTopic> GetAll(Func<IQueryable<SubTopic>, IQueryable<SubTopic>> include = null);
        Task<int> AddAsync(SubTopic subTopic);
        Task UpdateAsync(SubTopic subTopic);
        Task DeleteAsync(int id);
    }
}

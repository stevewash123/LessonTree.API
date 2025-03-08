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
        Topic GetById(int id, Func<IQueryable<Topic>, IQueryable<Topic>> include = null);
        void Add(Topic topic);
        void Update(Topic topic);
        void Delete(int id);
    }
}

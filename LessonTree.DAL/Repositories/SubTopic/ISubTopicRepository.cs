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
        IQueryable<SubTopic> GetAll(Func<IQueryable<SubTopic>, IQueryable<SubTopic>> include = null);
        SubTopic GetById(int id, Func<IQueryable<SubTopic>, IQueryable<SubTopic>> include = null);
        void Add(SubTopic subTopic);
        void Update(SubTopic subTopic);
        void Delete(int id);
    }
}

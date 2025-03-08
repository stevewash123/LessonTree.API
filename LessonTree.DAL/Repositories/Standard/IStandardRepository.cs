using LessonTree.DAL.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LessonTree.DAL.Repositories
{
    public interface IStandardRepository
    {
        IQueryable<Standard> GetAll();
        Standard GetById(int id);
        void Add(Standard standard);
        void Update(Standard standard);
        void Delete(int id);
        IQueryable<Standard> GetByTopicId(int topicId);
    }
}

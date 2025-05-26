using LessonTree.DAL.Domain;
using System.Collections.Generic;

namespace LessonTree.DAL.Repositories
{
    public interface IUserRepository
    {
        User GetById(int id);
        User GetByUserName(string userName);
        List<User> GetAll();
        void Add(User user);
        void Update(User user);
        void Delete(int id);
    }
}
using LessonTree.DAL.Domain;
using System.Collections.Generic;

namespace LessonTree.BLL.Service
{
    public interface IUserService
    {
        User GetById(int id);
        User GetByUserName(string userName); // Updated from GetByUsername
        List<User> GetAll();
        void Add(User user);
        void Update(User user);
        void Delete(int id);
    }
}
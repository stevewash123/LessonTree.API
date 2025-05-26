using LessonTree.DAL.Domain;
using System.Collections.Generic;

namespace LessonTree.BLL.Service
{
    public interface IUserService
    {
        User GetById(int id);
        User GetByUserName(string userName);
        List<User> GetAll();
        void Add(User user);
        User Update(User user);
        void Delete(int id);
    }
}
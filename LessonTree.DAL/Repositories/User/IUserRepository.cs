// **COMPLETE FILE** - JWT-aligned IUserRepository interface
// RESPONSIBILITY: User data access contract focused on application data
// DOES NOT: Include identity data operations - JWT owns that
// CALLED BY: UserService implementations

using LessonTree.DAL.Domain;

namespace LessonTree.DAL.Repositories
{
    public interface IUserRepository
    {
        User? GetById(int id);
        User? GetByUserName(string userName);
        List<User> GetAll();
        void Add(User user);
        void Update(User user);
        void Delete(int id);
        UserConfiguration? GetUserConfiguration(int userId);
        void UpdateUserConfiguration(int userId, UserConfiguration configuration);
    }
}
// **COMPLETE FILE** - JWT-aligned IUserRepository interface
// RESPONSIBILITY: User data access contract focused on application data
// DOES NOT: Include identity data operations - JWT owns that
// CALLED BY: UserService implementations

using LessonTree.DAL.Domain;

namespace LessonTree.DAL.Repositories
{
    public interface IUserRepository
    {
        // Core user operations
        User? GetById(int id);
        User? GetByUserName(string userName);
        List<User> GetAll();
        void Add(User user);
        void Update(User user);  // Only updates application data (District, Configuration)
        void Delete(int id);

        // Configuration-specific operations
        UserConfiguration? GetUserConfiguration(int userId);
        void UpdateUserConfiguration(int userId, UserConfiguration configuration);
    }
}
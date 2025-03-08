using Microsoft.AspNetCore.Identity;

namespace LessonTree.DAL.Domain
{
    public class User : IdentityUser<int> // Use int as key type
    {
        // Username and PasswordHash inherited from IdentityUser as UserName and PasswordHash
    }
}
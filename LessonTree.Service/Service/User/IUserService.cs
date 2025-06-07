// **COMPLETE FILE** - Fixed IUserService interface to match implementation
// RESPONSIBILITY: User service contract aligned with clean JWT DTOs
// DOES NOT: Reference properties that don't exist in clean DTOs
// CALLED BY: Dependency injection and UserController

using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;
using System.Collections.Generic;

namespace LessonTree.BLL.Service
{
    public interface IUserService
    {
        // Basic user operations
        UserResource? GetUserResourceById(int id);
        UserResource? GetUserResourceByUserName(string userName);
        List<UserResource> GetAllUserResources();
        UserResource CreateUser(UserCreateResource userCreateResource);
        UserResource? UpdateFromResource(int id, UserResource userResource);
        bool Delete(int id);

        // User configuration operations (clean JWT approach)
        UserConfigurationResource? GetUserConfiguration(int userId);
        UserConfigurationResource? UpdateUserConfiguration(int userId, UserConfigurationUpdate configUpdate);  // FIXED: Use UserConfigurationUpdate
    }
}
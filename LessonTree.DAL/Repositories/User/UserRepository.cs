// **COMPLETE FILE** - UserRepository.cs - Fixed for UserConfiguration refactor
// RESPONSIBILITY: User data access focused on basic user profile and simple configuration
// DOES NOT: Handle schedule configuration (that's ScheduleConfiguration domain)
// CALLED BY: UserService for user profile operations

using System.Collections.Generic;
using System.Linq;
using LessonTree.DAL.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LessonTree.DAL.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly LessonTreeContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(LessonTreeContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public User? GetById(int id)
        {
            _logger.LogDebug("Retrieving user by ID: {UserId}", id);
            var user = _context.Users
                .Include(u => u.Configuration) // Only basic UserConfiguration, no period assignments
                .FirstOrDefault(u => u.Id == id);

            if (user == null)
                _logger.LogWarning("User with ID {UserId} not found", id);

            return user;
        }

        public User? GetByUserName(string userName)
        {
            _logger.LogDebug("Retrieving user by UserName: {UserName}", userName);
            var user = _context.Users
                .Include(u => u.Configuration) // Only basic UserConfiguration
                .SingleOrDefault(u => u.UserName == userName);

            if (user == null)
                _logger.LogWarning("User with UserName {UserName} not found", userName);

            return user;
        }

        public List<User> GetAll()
        {
            _logger.LogDebug("Retrieving all users");
            return _context.Users
                .Include(u => u.Configuration) // Only basic UserConfiguration
                .ToList();
        }

        public void Add(User user)
        {
            _logger.LogDebug("Adding user: {UserName}", user.UserName);
            _context.Users.Add(user);
            _context.SaveChanges();
            _logger.LogInformation("Added user with ID: {UserId}, UserName: {UserName}", user.Id, user.UserName);
        }

        public void Update(User user)
        {
            _logger.LogDebug("Updating user application data: {UserId}", user.Id);

            var existingUser = _context.Users
                .Include(u => u.Configuration)
                .FirstOrDefault(u => u.Id == user.Id);

            if (existingUser == null)
            {
                _logger.LogError("User with ID {UserId} not found for update", user.Id);
                return;
            }

            // Update ONLY basic user profile data
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.Email = user.Email;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.DistrictId = user.DistrictId;
            existingUser.SchoolId = user.SchoolId;

            // Handle UserConfiguration updates if provided
            if (user.Configuration != null)
            {
                UpdateUserConfigurationInternal(existingUser, user.Configuration);
            }

            _context.SaveChanges();
            _logger.LogInformation("Updated application data for user ID: {UserId}", user.Id);
        }

        public void Delete(int id)
        {
            _logger.LogDebug("Deleting user with ID: {UserId}", id);
            var user = _context.Users
                .Include(u => u.Configuration)
                .FirstOrDefault(u => u.Id == id);

            if (user != null)
            {
                _context.Users.Remove(user); // Cascade delete will handle UserConfiguration
                _context.SaveChanges();
                _logger.LogInformation("Deleted user with ID: {UserId}", id);
            }
            else
            {
                _logger.LogWarning("User with ID {UserId} not found for deletion", id);
            }
        }

        public UserConfiguration? GetUserConfiguration(int userId)
        {
            _logger.LogDebug("Retrieving configuration for user ID: {UserId}", userId);
            return _context.UserConfigurations
                .FirstOrDefault(uc => uc.UserId == userId);
        }

        public void UpdateUserConfiguration(int userId, UserConfiguration configuration)
        {
            _logger.LogDebug("Updating configuration for user ID: {UserId}", userId);

            var existingUser = _context.Users
                .Include(u => u.Configuration)
                .FirstOrDefault(u => u.Id == userId);

            if (existingUser == null)
            {
                _logger.LogError("User with ID {UserId} not found for configuration update", userId);
                return;
            }

            UpdateUserConfigurationInternal(existingUser, configuration);
            _context.SaveChanges();
            _logger.LogInformation("Updated configuration for user ID: {UserId}", userId);
        }

        // Private helper method - simplified for basic user configuration only
        private void UpdateUserConfigurationInternal(User existingUser, UserConfiguration newConfiguration)
        {
            if (existingUser.Configuration == null)
            {
                // Create new basic configuration
                existingUser.Configuration = new UserConfiguration
                {
                    UserId = existingUser.Id,
                    LastUpdated = DateTime.UtcNow,
                    SettingsJson = newConfiguration.SettingsJson
                };
            }
            else
            {
                // Update existing basic configuration properties
                existingUser.Configuration.LastUpdated = DateTime.UtcNow;
                existingUser.Configuration.SettingsJson = newConfiguration.SettingsJson;
            }

            _logger.LogDebug("Updated UserConfiguration for user {UserId}", existingUser.Id);
        }
    }
}
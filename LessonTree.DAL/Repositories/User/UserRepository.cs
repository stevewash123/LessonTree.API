// **COMPLETE FILE** - JWT-aligned UserRepository
// RESPONSIBILITY: User data access focused on application data (district, configuration)
// DOES NOT: Handle identity data updates (firstName, lastName, email) - JWT owns that
// CALLED BY: UserService for application data operations

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
                .Include(u => u.Configuration)
                    .ThenInclude(c => c!.PeriodAssignments) // Load period assignments
                .FirstOrDefault(u => u.Id == id);

            if (user == null)
                _logger.LogWarning("User with ID {UserId} not found", id);

            return user;
        }

        public User? GetByUserName(string userName)
        {
            _logger.LogDebug("Retrieving user by UserName: {UserName}", userName);
            var user = _context.Users
                .Include(u => u.Configuration)
                    .ThenInclude(c => c!.PeriodAssignments)
                .SingleOrDefault(u => u.UserName == userName);

            if (user == null)
                _logger.LogWarning("User with UserName {UserName} not found", userName);

            return user;
        }

        public List<User> GetAll()
        {
            _logger.LogDebug("Retrieving all users");
            return _context.Users
                .Include(u => u.Configuration)
                    .ThenInclude(c => c!.PeriodAssignments)
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
                    .ThenInclude(c => c!.PeriodAssignments)
                .FirstOrDefault(u => u.Id == user.Id);

            if (existingUser == null)
            {
                _logger.LogError("User with ID {UserId} not found for update", user.Id);
                return;
            }

            // Update ONLY application data - NOT identity data (JWT owns that)
            existingUser.District = user.District;

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
                    .ThenInclude(c => c!.PeriodAssignments)
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
                .Include(uc => uc.PeriodAssignments)
                .FirstOrDefault(uc => uc.UserId == userId);
        }

        public void UpdateUserConfiguration(int userId, UserConfiguration configuration)
        {
            _logger.LogDebug("Updating configuration for user ID: {UserId}", userId);

            var existingUser = _context.Users
                .Include(u => u.Configuration)
                    .ThenInclude(c => c!.PeriodAssignments)
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

        // Private helper method
        private void UpdateUserConfigurationInternal(User existingUser, UserConfiguration newConfiguration)
        {
            if (existingUser.Configuration == null)
            {
                // Create new configuration
                existingUser.Configuration = new UserConfiguration
                {
                    UserId = existingUser.Id,
                    SchoolYear = newConfiguration.SchoolYear,
                    PeriodsPerDay = newConfiguration.PeriodsPerDay,
                    LastUpdated = DateTime.UtcNow,
                    PeriodAssignments = new List<PeriodAssignment>()
                };
            }
            else
            {
                // Update existing configuration properties
                existingUser.Configuration.SchoolYear = newConfiguration.SchoolYear;
                existingUser.Configuration.PeriodsPerDay = newConfiguration.PeriodsPerDay;
                existingUser.Configuration.LastUpdated = DateTime.UtcNow;

                // Clear and rebuild period assignments
                existingUser.Configuration.PeriodAssignments.Clear();
            }

            // Add new period assignments
            if (newConfiguration.PeriodAssignments != null)
            {
                foreach (var assignment in newConfiguration.PeriodAssignments)
                {
                    existingUser.Configuration.PeriodAssignments.Add(new PeriodAssignment
                    {
                        Period = assignment.Period,
                        CourseId = assignment.CourseId,
                        SectionName = assignment.SectionName,
                        Room = assignment.Room,
                        Notes = assignment.Notes,
                        BackgroundColor = assignment.BackgroundColor,
                        FontColor = assignment.FontColor,
                        UserConfigurationId = existingUser.Configuration.Id
                    });
                }
            }

            _logger.LogDebug("Updated UserConfiguration for user {UserId}", existingUser.Id);
        }
    }
}
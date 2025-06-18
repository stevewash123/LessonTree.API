// **CLEANED** - UserService with period assignments removed
// RESPONSIBILITY: User business logic for basic account management only
// DOES NOT: Handle period assignments (moved to ScheduleConfigurationService)
// CALLED BY: UserController for user account operations

using LessonTree.DAL.Repositories;
using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace LessonTree.BLL.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository repository, IMapper mapper, ILogger<UserService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public UserResource? GetUserResourceById(int id)
        {
            _logger.LogDebug("Fetching user resource by ID: {UserId}", id);
            var user = _repository.GetById(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", id);
                return null;
            }

            return MapUserToResource(user);
        }

        public UserResource? GetUserResourceByUserName(string userName)
        {
            _logger.LogDebug("Fetching user resource by UserName: {UserName}", userName);
            var user = _repository.GetByUserName(userName);
            if (user == null)
            {
                _logger.LogWarning("User with UserName {UserName} not found", userName);
                return null;
            }

            return MapUserToResource(user);
        }

        public List<UserResource> GetAllUserResources()
        {
            _logger.LogDebug("Fetching all user resources");
            var users = _repository.GetAll();
            return users.Select(MapUserToResource).ToList();
        }

        public UserResource CreateUser(UserCreateResource userCreateResource)
        {
            _logger.LogDebug("Creating user: {UserName}", userCreateResource.Username);

            var user = _mapper.Map<User>(userCreateResource);
            _repository.Add(user);

            _logger.LogInformation("User created with ID: {UserId}", user.Id);
            return MapUserToResource(user);
        }

        public UserResource? UpdateFromResource(int id, UserResource userResource)
        {
            _logger.LogDebug("Updating user from resource: ID {UserId}", id);

            var existingUser = _repository.GetById(id);
            if (existingUser == null)
            {
                _logger.LogError("User with ID {UserId} not found for update", id);
                return null;
            }

            // Update basic user properties only
            existingUser.FirstName = userResource.FirstName;
            existingUser.LastName = userResource.LastName;
            existingUser.Email = userResource.Email;
            existingUser.PhoneNumber = userResource.Phone;

            // Handle simplified UserConfiguration updates if provided
            if (userResource.Configuration != null)
            {
                UpdateUserConfigurationFromResource(existingUser, userResource.Configuration);
            }

            _repository.Update(existingUser);

            // Fetch updated entity and return as resource
            var updatedUser = _repository.GetById(id);
            _logger.LogInformation("User updated with ID: {UserId}", id);
            return MapUserToResource(updatedUser);
        }

        public bool Delete(int id)
        {
            _logger.LogDebug("Deleting user with ID: {UserId}", id);

            var user = _repository.GetById(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found for deletion", id);
                return false;
            }

            _repository.Delete(id);
            _logger.LogInformation("User deleted with ID: {UserId}", id);
            return true;
        }

        public UserConfigurationResource? GetUserConfiguration(int userId)
        {
            _logger.LogDebug("Fetching user configuration for user ID: {UserId}", userId);

            var user = _repository.GetById(userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", userId);
                return null;
            }

            if (user.Configuration == null)
            {
                _logger.LogDebug("No configuration found for user ID: {UserId}, returning defaults", userId);
                return CreateDefaultUserConfiguration();
            }

            return _mapper.Map<UserConfigurationResource>(user.Configuration);
        }

        public UserConfigurationResource? UpdateUserConfiguration(int userId, UserConfigurationUpdate configUpdate)
        {
            try
            {
                _logger.LogDebug("Updating basic configuration for user ID: {UserId}", userId);

                var existingUser = _repository.GetById(userId);
                if (existingUser == null)
                {
                    _logger.LogError("User with ID {UserId} not found for configuration update", userId);
                    return null;
                }

                // Create simplified domain object (no period assignments)
                var configurationDomain = new UserConfiguration
                {
                    LastUpdated = DateTime.UtcNow
                    // REMOVED: All schedule-related properties moved to ScheduleConfiguration
                    // - SchoolYear (moved to ScheduleConfiguration)
                    // - PeriodsPerDay (moved to ScheduleConfiguration)
                    // - PeriodAssignments (moved to ScheduleConfiguration)
                };

                // Update via repository (repository sets UserId)
                _repository.UpdateUserConfiguration(userId, configurationDomain);

                _logger.LogInformation("Updated basic configuration for user ID: {UserId}", userId);
                return GetUserConfiguration(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user configuration for user ID: {UserId}", userId);
                throw;
            }
        }

        // === PRIVATE HELPER METHODS ===

        private UserResource MapUserToResource(User user)
        {
            var userResource = _mapper.Map<UserResource>(user);

            // Map simplified UserConfiguration if it exists
            if (user.Configuration != null)
            {
                userResource.Configuration = _mapper.Map<UserConfigurationResource>(user.Configuration);
            }

            return userResource;
        }

        private void UpdateUserConfigurationFromResource(User existingUser, UserConfigurationResource configResource)
        {
            if (existingUser.Configuration == null)
            {
                existingUser.Configuration = new UserConfiguration
                {
                    UserId = existingUser.Id,
                    LastUpdated = DateTime.UtcNow
                };
            }

            // Update basic configuration properties only
            existingUser.Configuration.LastUpdated = DateTime.UtcNow;

            // REMOVED: All schedule-related property updates
            // - SchoolYear (moved to ScheduleConfiguration)
            // - PeriodsPerDay (moved to ScheduleConfiguration)
            // - PeriodAssignments (moved to ScheduleConfiguration)
        }

        private UserConfigurationResource CreateDefaultUserConfiguration()
        {
            return new UserConfigurationResource
            {
                LastUpdated = DateTime.UtcNow
                // REMOVED: All schedule-related defaults moved to ScheduleConfiguration
                // - SchoolYear (moved to ScheduleConfiguration)
                // - PeriodsPerDay (moved to ScheduleConfiguration)
                // - PeriodAssignments (moved to ScheduleConfiguration)
            };
        }

        // REMOVED: All period assignment validation methods - moved to ScheduleConfigurationService
        // - ValidatePeriodAssignment()
        // - IsValidSpecialPeriodType()
        // - ParseSpecialPeriodType()
        // - GetCurrentSchoolYear()
    }
}
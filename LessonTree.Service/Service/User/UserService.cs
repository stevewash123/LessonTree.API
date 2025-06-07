// **COMPLETE FILE** - UserService with clean JWT approach and fixed period validation
// RESPONSIBILITY: User business logic without UserId in DTOs
// DOES NOT: Reference UserConfigurationResource.UserId (doesn't exist)
// CALLED BY: UserController for all user operations

using LessonTree.DAL.Repositories;
using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;
using LessonTree.Models.Enums;
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

            // Update basic properties
            existingUser.FirstName = userResource.FirstName;
            existingUser.LastName = userResource.LastName;

            // Handle UserConfiguration updates if provided
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
                _logger.LogDebug("Updating configuration for user ID: {UserId}", userId);

                var existingUser = _repository.GetById(userId);
                if (existingUser == null)
                {
                    _logger.LogError("User with ID {UserId} not found for configuration update", userId);
                    return null;
                }

                // Validate period assignments before processing
                if (configUpdate.PeriodAssignments != null)
                {
                    foreach (var assignment in configUpdate.PeriodAssignments)
                    {
                        if (assignment.CourseId.HasValue)
                        {
                            var courseId = assignment.CourseId.Value;

                            if (courseId < 0)
                            {
                                // Validate it's a known fixed period type
                                if (!FixedPeriodTypeExtensions.IsValidFixedPeriodId(courseId))
                                {
                                    throw new ArgumentException($"Invalid fixed period type: {courseId}. " +
                                        $"Valid options are: {string.Join(", ", Enum.GetValues<FixedPeriodType>().Cast<int>())}");
                                }
                                // Fixed periods are valid - continue without database validation
                                _logger.LogDebug("Validated fixed period type: {CourseId} for period {Period}", courseId, assignment.Period);
                            }
                            else if (courseId > 0)
                            {
                                // For now, skip course validation since we don't have course repository injected
                                // TODO: Add ICourseRepository to constructor when needed for strict validation
                                _logger.LogDebug("Allowing positive CourseId: {CourseId} for period {Period} (validation skipped)", courseId, assignment.Period);
                            }
                            else
                            {
                                throw new ArgumentException("CourseId cannot be zero");
                            }
                        }
                    }
                }

                // Create domain object from clean DTO (no UserId needed)
                var configurationDomain = new UserConfiguration
                {
                    SchoolYear = configUpdate.SchoolYear,
                    PeriodsPerDay = configUpdate.PeriodsPerDay,
                    LastUpdated = DateTime.UtcNow,
                    PeriodAssignments = configUpdate.PeriodAssignments?.Select(pa => new PeriodAssignment
                    {
                        Period = pa.Period,
                        CourseId = pa.CourseId,
                        SectionName = pa.SectionName ?? string.Empty,
                        Room = pa.Room ?? string.Empty,
                        Notes = pa.Notes ?? string.Empty,
                        BackgroundColor = pa.BackgroundColor ?? "#2196F3",
                        FontColor = pa.FontColor ?? "#FFFFFF"
                    }).ToList() ?? new List<PeriodAssignment>()
                };

                // Update via repository (repository sets UserId)
                _repository.UpdateUserConfiguration(userId, configurationDomain);

                _logger.LogInformation("Updated configuration for user ID: {UserId}", userId);
                return GetUserConfiguration(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user configuration for user ID: {UserId}", userId);
                throw;
            }
        }

        // Private helper methods
        private UserResource MapUserToResource(User user)
        {
            var userResource = _mapper.Map<UserResource>(user);

            // Map UserConfiguration if it exists
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

            existingUser.Configuration.SchoolYear = configResource.SchoolYear;
            existingUser.Configuration.PeriodsPerDay = configResource.PeriodsPerDay;
            existingUser.Configuration.LastUpdated = DateTime.UtcNow;

            // Handle period assignments if provided
            if (configResource.PeriodAssignments != null)
            {
                existingUser.Configuration.PeriodAssignments.Clear();
                foreach (var assignmentResource in configResource.PeriodAssignments)
                {
                    existingUser.Configuration.PeriodAssignments.Add(new PeriodAssignment
                    {
                        Period = assignmentResource.Period,
                        CourseId = assignmentResource.CourseId,
                        SectionName = assignmentResource.SectionName,
                        Room = assignmentResource.Room,
                        Notes = assignmentResource.Notes,
                        BackgroundColor = assignmentResource.BackgroundColor,
                        FontColor = assignmentResource.FontColor,
                        UserConfigurationId = existingUser.Configuration.Id
                    });
                }
            }
        }

        private UserConfigurationResource CreateDefaultUserConfiguration()
        {
            var currentSchoolYear = GetCurrentSchoolYear();

            return new UserConfigurationResource
            {
                LastUpdated = DateTime.UtcNow,
                SchoolYear = currentSchoolYear,
                PeriodsPerDay = 6, // Common default
                PeriodAssignments = new List<PeriodAssignmentResource>() // Empty - teacher will configure
            };
        }

        private string GetCurrentSchoolYear()
        {
            var currentYear = DateTime.Now.Year;
            var currentMonth = DateTime.Now.Month;

            // School year typically starts in August (month 8) and ends in June (month 6)
            if (currentMonth >= 8) // August or later = current year is start of school year
            {
                return $"{currentYear}-{currentYear + 1}";
            }
            else // Before August = current year is end of school year
            {
                return $"{currentYear - 1}-{currentYear}";
            }
        }
    }
}
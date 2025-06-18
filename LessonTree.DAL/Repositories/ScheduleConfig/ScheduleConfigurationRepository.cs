// **NEW REPOSITORY** - ScheduleConfigurationRepository.cs
// RESPONSIBILITY: Schedule configuration data access and management
// DOES NOT: Handle schedule events (that's ScheduleRepository)
// CALLED BY: ScheduleConfigurationController for configuration operations

using LessonTree.DAL.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LessonTree.DAL.Repositories
{
    public class ScheduleConfigurationRepository : IScheduleConfigurationRepository
    {
        private readonly LessonTreeContext _context;
        private readonly ILogger<ScheduleConfigurationRepository> _logger;

        public ScheduleConfigurationRepository(LessonTreeContext context, ILogger<ScheduleConfigurationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<ScheduleConfiguration>> GetByUserIdAsync(int userId)
        {
            _logger.LogInformation($"GetByUserIdAsync: Fetching all configurations for user {userId}");

            var configurations = await _context.ScheduleConfigurations
                .Include(sc => sc.PeriodAssignments.OrderBy(pa => pa.Period))
                .Where(sc => sc.UserId == userId)
                .OrderByDescending(sc => sc.IsActive)
                .ThenByDescending(sc => sc.LastUpdated)
                .ToListAsync();

            _logger.LogInformation($"GetByUserIdAsync: Found {configurations.Count} configurations for user {userId}");
            return configurations;
        }

        public async Task<ScheduleConfiguration?> GetByIdAsync(int id)
        {
            _logger.LogInformation($"GetByIdAsync: Fetching configuration {id}");

            var configuration = await _context.ScheduleConfigurations
                .Include(sc => sc.PeriodAssignments.OrderBy(pa => pa.Period))
                .Include(sc => sc.User)
                .FirstOrDefaultAsync(sc => sc.Id == id);

            if (configuration != null)
            {
                _logger.LogInformation($"GetByIdAsync: Found configuration {id} for user {configuration.UserId}");
            }
            else
            {
                _logger.LogWarning($"GetByIdAsync: Configuration {id} not found");
            }

            return configuration;
        }

        public async Task<ScheduleConfiguration?> GetActiveByUserIdAsync(int userId)
        {
            _logger.LogInformation($"GetActiveByUserIdAsync: Fetching active configuration for user {userId}");

            var configuration = await _context.ScheduleConfigurations
                .Include(sc => sc.PeriodAssignments.OrderBy(pa => pa.Period))
                .FirstOrDefaultAsync(sc => sc.UserId == userId && sc.IsActive);

            if (configuration != null)
            {
                _logger.LogInformation($"GetActiveByUserIdAsync: Found active configuration {configuration.Id} for user {userId}");
            }
            else
            {
                _logger.LogInformation($"GetActiveByUserIdAsync: No active configuration found for user {userId}");
            }

            return configuration;
        }

        public async Task<ScheduleConfiguration?> GetByUserIdAndSchoolYearAsync(int userId, string schoolYear)
        {
            _logger.LogInformation($"GetByUserIdAndSchoolYearAsync: Fetching configuration for user {userId}, school year {schoolYear}");

            var configuration = await _context.ScheduleConfigurations
                .Include(sc => sc.PeriodAssignments.OrderBy(pa => pa.Period))
                .FirstOrDefaultAsync(sc => sc.UserId == userId && sc.SchoolYear == schoolYear);

            return configuration;
        }

        public async Task<ScheduleConfiguration> CreateAsync(ScheduleConfiguration configuration)
        {
            _logger.LogInformation($"CreateAsync: Creating configuration '{configuration.Title}' for user {configuration.UserId}");

            // Set metadata
            configuration.CreatedDate = DateTime.UtcNow;
            configuration.LastUpdated = DateTime.UtcNow;

            // If this is the first configuration for the user, make it active
            var existingConfigs = await _context.ScheduleConfigurations
                .Where(sc => sc.UserId == configuration.UserId)
                .CountAsync();

            if (existingConfigs == 0)
            {
                configuration.IsActive = true;
            }

            // If setting as active, deactivate others
            if (configuration.IsActive)
            {
                await DeactivateOtherConfigurationsAsync(configuration.UserId);
            }

            _context.ScheduleConfigurations.Add(configuration);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"CreateAsync: Created configuration {configuration.Id} for user {configuration.UserId}");

            // Return with includes
            return await GetByIdAsync(configuration.Id) ?? configuration;
        }

        public async Task<ScheduleConfiguration> UpdateAsync(ScheduleConfiguration configuration)
        {
            _logger.LogInformation($"UpdateAsync: Updating configuration {configuration.Id}");

            var existingConfig = await _context.ScheduleConfigurations
                .Include(sc => sc.PeriodAssignments)
                .FirstOrDefaultAsync(sc => sc.Id == configuration.Id);

            if (existingConfig == null)
            {
                throw new ArgumentException($"ScheduleConfiguration {configuration.Id} not found");
            }

            // Update basic properties
            existingConfig.Title = configuration.Title;
            existingConfig.SchoolYear = configuration.SchoolYear;
            existingConfig.StartDate = configuration.StartDate;
            existingConfig.EndDate = configuration.EndDate;
            existingConfig.PeriodsPerDay = configuration.PeriodsPerDay;
            existingConfig.TeachingDays = configuration.TeachingDays;
            existingConfig.IsTemplate = configuration.IsTemplate;
            existingConfig.LastUpdated = DateTime.UtcNow;

            // Handle active status change
            if (configuration.IsActive && !existingConfig.IsActive)
            {
                await DeactivateOtherConfigurationsAsync(existingConfig.UserId);
                existingConfig.IsActive = true;
            }
            else if (!configuration.IsActive && existingConfig.IsActive)
            {
                existingConfig.IsActive = false;
            }

            // Update period assignments
            _context.PeriodAssignments.RemoveRange(existingConfig.PeriodAssignments);
            foreach (var assignment in configuration.PeriodAssignments)
            {
                assignment.ScheduleConfigurationId = existingConfig.Id;
                assignment.Id = 0; // Reset for new entity
            }
            _context.PeriodAssignments.AddRange(configuration.PeriodAssignments);

            await _context.SaveChangesAsync();

            _logger.LogInformation($"UpdateAsync: Updated configuration {configuration.Id}");

            // Return with includes
            return await GetByIdAsync(configuration.Id) ?? existingConfig;
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation($"DeleteAsync: Deleting configuration {id}");

            var configuration = await _context.ScheduleConfigurations
                .Include(sc => sc.PeriodAssignments)
                .Include(sc => sc.Schedules)
                .FirstOrDefaultAsync(sc => sc.Id == id);

            if (configuration == null)
            {
                throw new ArgumentException($"ScheduleConfiguration {id} not found");
            }

            // Check if configuration is in use by schedules
            if (configuration.Schedules.Any())
            {
                throw new InvalidOperationException($"Cannot delete configuration {id} - it is used by {configuration.Schedules.Count} schedule(s)");
            }

            _context.ScheduleConfigurations.Remove(configuration);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"DeleteAsync: Deleted configuration {id}");
        }

        public async Task<ScheduleConfiguration> SetActiveConfigurationAsync(int userId, int configurationId)
        {
            _logger.LogInformation($"SetActiveConfigurationAsync: Setting configuration {configurationId} as active for user {userId}");

            var configuration = await _context.ScheduleConfigurations
                .FirstOrDefaultAsync(sc => sc.Id == configurationId && sc.UserId == userId);

            if (configuration == null)
            {
                throw new ArgumentException($"ScheduleConfiguration {configurationId} not found for user {userId}");
            }

            // Deactivate other configurations
            await DeactivateOtherConfigurationsAsync(userId);

            // Activate this configuration
            configuration.IsActive = true;
            configuration.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"SetActiveConfigurationAsync: Set configuration {configurationId} as active for user {userId}");

            return await GetByIdAsync(configurationId) ?? configuration;
        }

        public async Task<ScheduleConfiguration> CopyAsTemplateAsync(int sourceConfigurationId, string newTitle)
        {
            _logger.LogInformation($"CopyAsTemplateAsync: Copying configuration {sourceConfigurationId} as template '{newTitle}'");

            var sourceConfig = await GetByIdAsync(sourceConfigurationId);
            if (sourceConfig == null)
            {
                throw new ArgumentException($"Source configuration {sourceConfigurationId} not found");
            }

            var newConfig = new ScheduleConfiguration
            {
                UserId = sourceConfig.UserId,
                Title = newTitle,
                SchoolYear = sourceConfig.SchoolYear,
                StartDate = sourceConfig.StartDate,
                EndDate = sourceConfig.EndDate,
                PeriodsPerDay = sourceConfig.PeriodsPerDay,
                TeachingDays = sourceConfig.TeachingDays,
                IsActive = false,
                IsTemplate = true,
                PeriodAssignments = sourceConfig.PeriodAssignments.Select(pa => new PeriodAssignment
                {
                    Period = pa.Period,
                    CourseId = pa.CourseId,
                    SpecialPeriodType = pa.SpecialPeriodType,
                    TeachingDays = pa.TeachingDays,
                    Room = pa.Room,
                    Notes = pa.Notes,
                    BackgroundColor = pa.BackgroundColor,
                    FontColor = pa.FontColor
                }).ToList()
            };

            return await CreateAsync(newConfig);
        }

        public async Task<List<ScheduleConfiguration>> GetTemplatesAsync(int userId)
        {
            _logger.LogInformation($"GetTemplatesAsync: Fetching templates for user {userId}");

            var templates = await _context.ScheduleConfigurations
                .Include(sc => sc.PeriodAssignments.OrderBy(pa => pa.Period))
                .Where(sc => sc.UserId == userId && sc.IsTemplate)
                .OrderBy(sc => sc.Title)
                .ToListAsync();

            _logger.LogInformation($"GetTemplatesAsync: Found {templates.Count} templates for user {userId}");
            return templates;
        }

        // === PRIVATE HELPER METHODS ===

        private async Task DeactivateOtherConfigurationsAsync(int userId)
        {
            var activeConfigs = await _context.ScheduleConfigurations
                .Where(sc => sc.UserId == userId && sc.IsActive)
                .ToListAsync();

            foreach (var config in activeConfigs)
            {
                config.IsActive = false;
            }
        }
    }
}
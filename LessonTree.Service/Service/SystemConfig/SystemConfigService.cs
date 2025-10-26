using LessonTree.DAL;
using LessonTree.DAL.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LessonTree.Service.Service.SystemConfig
{
    public class SystemConfigService : ISystemConfigService
    {
        private readonly LessonTreeContext _context;
        private readonly ILogger<SystemConfigService> _logger;

        private const string LAST_SEED_KEY = "LAST_SEED_DATE";

        public SystemConfigService(LessonTreeContext context, ILogger<SystemConfigService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string?> GetConfigValueAsync(string key)
        {
            try
            {
                var config = await _context.SystemConfigs
                    .FirstOrDefaultAsync(c => c.Key == key);

                return config?.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get config value for key: {Key}", key);
                return null;
            }
        }

        public async Task SetConfigValueAsync(string key, string value, string? description = null)
        {
            try
            {
                var config = await _context.SystemConfigs
                    .FirstOrDefaultAsync(c => c.Key == key);

                if (config == null)
                {
                    config = new DAL.Domain.SystemConfig
                    {
                        Key = key,
                        Value = value,
                        Description = description,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow
                    };
                    _context.SystemConfigs.Add(config);
                }
                else
                {
                    config.Value = value;
                    config.UpdatedDate = DateTime.UtcNow;
                    if (description != null)
                    {
                        config.Description = description;
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated system config: {Key} = {Value}", key, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set config value for key: {Key}", key);
                throw;
            }
        }

        public async Task<DateTime?> GetLastSeedDateAsync()
        {
            try
            {
                var seedDateString = await GetConfigValueAsync(LAST_SEED_KEY);

                if (string.IsNullOrEmpty(seedDateString))
                {
                    _logger.LogInformation("No last seed date found in system config");
                    return null;
                }

                if (DateTime.TryParse(seedDateString, out var seedDate))
                {
                    _logger.LogInformation("Last seed date: {SeedDate}", seedDate);
                    return seedDate;
                }

                _logger.LogWarning("Invalid last seed date format in config: {SeedDateString}", seedDateString);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get last seed date");
                return null;
            }
        }

        public async Task SetLastSeedDateAsync(DateTime seedDate)
        {
            try
            {
                var seedDateString = seedDate.ToString("O"); // ISO 8601 format
                await SetConfigValueAsync(
                    LAST_SEED_KEY,
                    seedDateString,
                    "Last time the demo data was seeded"
                );

                _logger.LogInformation("Set last seed date to: {SeedDate}", seedDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set last seed date");
                throw;
            }
        }

        public async Task<bool> ShouldReseedAsync()
        {
            try
            {
                var lastSeedDate = await GetLastSeedDateAsync();

                // If no seed date exists, we should seed
                if (lastSeedDate == null)
                {
                    _logger.LogInformation("No previous seed date found - should reseed");
                    return true;
                }

                // Check if more than 24 hours have passed
                var hoursSinceLastSeed = (DateTime.UtcNow - lastSeedDate.Value).TotalHours;
                var shouldReseed = hoursSinceLastSeed >= 24;

                _logger.LogInformation(
                    "Last seed: {LastSeed}, Hours ago: {Hours:F1}, Should reseed: {ShouldReseed}",
                    lastSeedDate.Value,
                    hoursSinceLastSeed,
                    shouldReseed
                );

                return shouldReseed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to determine if should reseed");
                // Default to not reseeding on error to avoid infinite loops
                return false;
            }
        }
    }
}
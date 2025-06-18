using AutoMapper;
using LessonTree.BLL.Services;
using LessonTree.DAL.Repositories;
using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;
using Microsoft.Extensions.Logging;

namespace LessonTree.BLL.Services
{
    /// <summary>
    /// Service for schedule configuration operations
    /// Handles CRUD operations for user schedule configurations/templates
    /// </summary>
    public class ScheduleConfigurationService : IScheduleConfigurationService
    {
        private readonly IScheduleConfigurationRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<ScheduleConfigurationService> _logger;

        public ScheduleConfigurationService(
            IScheduleConfigurationRepository repository,
            IMapper mapper,
            ILogger<ScheduleConfigurationService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<ScheduleConfigurationResource>> GetAllAsync(int userId)
        {
            _logger.LogDebug("Getting all schedule configurations for user {UserId}", userId);

            var configurations = await _repository.GetByUserIdAsync(userId);
            return _mapper.Map<List<ScheduleConfigurationResource>>(configurations);
        }

        public async Task<ScheduleConfigurationResource?> GetByIdAsync(int id, int userId)
        {
            _logger.LogDebug("Getting schedule configuration {ConfigId} for user {UserId}", id, userId);

            var configuration = await _repository.GetByIdAsync(id);
            if (configuration == null)
            {
                _logger.LogWarning("Schedule configuration {ConfigId} not found", id);
                return null;
            }

            if (configuration.UserId != userId)
            {
                _logger.LogWarning("Schedule configuration {ConfigId} not owned by user {UserId}", id, userId);
                throw new UnauthorizedAccessException("Schedule configuration not owned by user");
            }

            return _mapper.Map<ScheduleConfigurationResource>(configuration);
        }

        public async Task<ScheduleConfigurationResource?> GetActiveAsync(int userId)
        {
            _logger.LogDebug("Getting active schedule configuration for user {UserId}", userId);

            var configuration = await _repository.GetActiveByUserIdAsync(userId);
            return configuration != null ? _mapper.Map<ScheduleConfigurationResource>(configuration) : null;
        }

        public async Task<ScheduleConfigurationResource?> GetBySchoolYearAsync(int userId, string schoolYear)
        {
            _logger.LogDebug("Getting schedule configuration for user {UserId} and school year {SchoolYear}", userId, schoolYear);

            var configuration = await _repository.GetByUserIdAndSchoolYearAsync(userId, schoolYear);
            return configuration != null ? _mapper.Map<ScheduleConfigurationResource>(configuration) : null;
        }

        public async Task<List<ScheduleConfigurationResource>> GetTemplatesAsync(int userId)
        {
            _logger.LogDebug("Getting template schedule configurations for user {UserId}", userId);

            var templates = await _repository.GetTemplatesAsync(userId);
            return _mapper.Map<List<ScheduleConfigurationResource>>(templates);
        }

        public async Task DeleteAsync(int id, int userId)
        {
            _logger.LogDebug("Deleting schedule configuration {ConfigId} for user {UserId}", id, userId);

            // Validate ownership
            var configuration = await _repository.GetByIdAsync(id);
            if (configuration == null)
            {
                throw new ArgumentException("Schedule configuration not found");
            }

            if (configuration.UserId != userId)
            {
                throw new UnauthorizedAccessException("Schedule configuration not owned by user");
            }

            await _repository.DeleteAsync(id);

            _logger.LogInformation("Deleted schedule configuration {ConfigId} for user {UserId}", id, userId);
        }

        public async Task<ScheduleConfigurationResource> SetActiveAsync(int id, int userId)
        {
            _logger.LogDebug("Setting schedule configuration {ConfigId} as active for user {UserId}", id, userId);

            var updated = await _repository.SetActiveConfigurationAsync(userId, id);

            _logger.LogInformation("Set schedule configuration {ConfigId} as active for user {UserId}", id, userId);
            return _mapper.Map<ScheduleConfigurationResource>(updated);
        }

        public async Task<ScheduleConfigurationResource> CopyAsTemplateAsync(int id, CopyConfigurationRequest request, int userId)
        {
            _logger.LogDebug("Copying schedule configuration {ConfigId} as template '{NewTitle}' for user {UserId}",
                id, request.NewTitle, userId);

            // Validate ownership of source
            var source = await _repository.GetByIdAsync(id);
            if (source == null)
            {
                throw new ArgumentException("Source schedule configuration not found");
            }

            if (source.UserId != userId)
            {
                throw new UnauthorizedAccessException("Source schedule configuration not owned by user");
            }

            var copied = await _repository.CopyAsTemplateAsync(id, request.NewTitle);

            _logger.LogInformation("Copied schedule configuration {SourceId} as template {NewId} '{NewTitle}' for user {UserId}",
                id, copied.Id, request.NewTitle, userId);

            return _mapper.Map<ScheduleConfigurationResource>(copied);
        }

        public async Task<ScheduleConfigurationValidationResource> ValidateAsync(int id, int userId)
        {
            _logger.LogDebug("Validating schedule configuration {ConfigId} for user {UserId}", id, userId);

            var configuration = await GetByIdAsync(id, userId);
            if (configuration == null)
            {
                throw new ArgumentException("Schedule configuration not found");
            }

            var validation = new ScheduleConfigurationValidationResource
            {
                IsValid = true,
                CanGenerateSchedule = true,
                Errors = new List<string>(),
                Warnings = new List<string>()
            };

            // Basic validation rules
            if (configuration.StartDate >= configuration.EndDate)
            {
                validation.IsValid = false;
                validation.CanGenerateSchedule = false;
                validation.Errors.Add("Start date must be before end date");
            }

            if (configuration.PeriodsPerDay < 1 || configuration.PeriodsPerDay > 10)
            {
                validation.IsValid = false;
                validation.CanGenerateSchedule = false;
                validation.Errors.Add("Periods per day must be between 1 and 10");
            }

            if (configuration.TeachingDays.Length == 0)
            {
                validation.IsValid = false;
                validation.CanGenerateSchedule = false;
                validation.Errors.Add("At least one teaching day must be specified");
            }

            // Check for gaps in period assignments
            var assignedPeriods = configuration.PeriodAssignments.Select(pa => pa.Period).ToHashSet();
            var missingPeriods = new List<int>();
            for (int i = 1; i <= configuration.PeriodsPerDay; i++)
            {
                if (!assignedPeriods.Contains(i))
                {
                    missingPeriods.Add(i);
                }
            }

            if (missingPeriods.Any())
            {
                validation.Warnings.Add($"No assignments for period(s): {string.Join(", ", missingPeriods)}");
            }

            _logger.LogDebug("Validation complete for configuration {ConfigId}: Valid={IsValid}, CanGenerate={CanGenerate}",
                id, validation.IsValid, validation.CanGenerateSchedule);

            return validation;
        }

        public async Task<List<ScheduleConfigurationSummaryResource>> GetSummariesAsync(int userId)
        {
            _logger.LogDebug("Getting schedule configuration summaries for user {UserId}", userId);

            var configurations = await _repository.GetByUserIdAsync(userId);

            // Map to summary resources manually since AutoMapper mapping would be complex
            return configurations.Select(config => new ScheduleConfigurationSummaryResource
            {
                Id = config.Id,
                Title = config.Title,
                SchoolYear = config.SchoolYear,
                StartDate = config.StartDate,
                EndDate = config.EndDate,
                IsActive = config.IsActive,
                PeriodCount = config.PeriodsPerDay,
                AssignedPeriods = config.PeriodAssignments.Count
            }).ToList();
        }

        // === SMART SCHOOL YEAR COMPUTATION ===

        /// <summary>
        /// Computes intelligent school year based on date range with flexible month matching
        /// </summary>
        private string ComputeSchoolYear(DateTime startDate, DateTime endDate)
        {
            int startMonth = startDate.Month;
            int endMonth = endDate.Month;
            int startYear = startDate.Year;
            int endYear = endDate.Year;

            // Academic Year: Aug/Sept start, May/June end, spans years
            if ((startMonth >= 8 || startMonth <= 9) &&
                (endMonth >= 5 && endMonth <= 6) &&
                endYear > startYear)
            {
                return $"{startYear}-{endYear}";
            }

            // Fall Semester: Aug/Sept/Oct start, Nov/Dec/Jan end
            if ((startMonth >= 8 && startMonth <= 10) &&
                (endMonth >= 11 || endMonth <= 1))
            {
                return $"Fall Semester {startYear}";
            }

            // Spring Semester: Jan/Feb start, May/June end
            if ((startMonth >= 1 && startMonth <= 2) &&
                (endMonth >= 5 && endMonth <= 6))
            {
                return $"Spring Semester {endYear}";
            }

            // Summer Session: May/June start, July/Aug end
            if ((startMonth >= 5 && startMonth <= 6) &&
                (endMonth >= 7 && endMonth <= 8))
            {
                return $"Summer Session {startYear}";
            }

            // Default fallback
            return $"Instructional Period {startYear} to {endYear}";
        }

        // === UPDATED CREATE METHOD ===

        public async Task<ScheduleConfigurationResource> CreateAsync(ScheduleConfigurationCreateResource resource, int userId)
        {
            _logger.LogDebug("Creating schedule configuration '{Title}' for user {UserId}", resource.Title, userId);

            var configuration = _mapper.Map<ScheduleConfiguration>(resource);
            configuration.UserId = userId; // Set ownership

            // Always auto-compute school year from dates
            configuration.SchoolYear = ComputeSchoolYear(configuration.StartDate, configuration.EndDate);
            _logger.LogDebug("Auto-computed school year: '{SchoolYear}' for configuration '{Title}'",
                configuration.SchoolYear, resource.Title);

            var created = await _repository.CreateAsync(configuration);

            _logger.LogInformation("Created schedule configuration {ConfigId} '{Title}' (School Year: {SchoolYear}) for user {UserId}",
                created.Id, resource.Title, created.SchoolYear, userId);

            return _mapper.Map<ScheduleConfigurationResource>(created);
        }

        // === UPDATED UPDATE METHOD ===

        public async Task<ScheduleConfigurationResource> UpdateAsync(int id, ScheduleConfigurationUpdateResource resource, int userId)
        {
            _logger.LogDebug("Updating schedule configuration {ConfigId} for user {UserId}", id, userId);

            // Validate ownership first
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
            {
                throw new ArgumentException("Schedule configuration not found");
            }

            if (existing.UserId != userId)
            {
                throw new UnauthorizedAccessException("Schedule configuration not owned by user");
            }

            var configuration = _mapper.Map<ScheduleConfiguration>(resource);
            configuration.UserId = userId; // Maintain ownership

            // Always auto-compute school year from dates
            configuration.SchoolYear = ComputeSchoolYear(configuration.StartDate, configuration.EndDate);
            _logger.LogDebug("Auto-computed school year: '{SchoolYear}' for configuration {ConfigId}",
                configuration.SchoolYear, id);

            var updated = await _repository.UpdateAsync(configuration);

            _logger.LogInformation("Updated schedule configuration {ConfigId} '{Title}' (School Year: {SchoolYear}) for user {UserId}",
                id, resource.Title, updated.SchoolYear, userId);

            return _mapper.Map<ScheduleConfigurationResource>(updated);
        }
    }
}
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
            _logger.LogInformation($"GetAllAsync: Fetching configurations for user {userId}");

            var configurations = await _repository.GetByUserIdAsync(userId);

            _logger.LogInformation($"GetAllAsync: Found {configurations.Count} configurations for user {userId}");
            return _mapper.Map<List<ScheduleConfigurationResource>>(configurations);
        }

        public async Task<ScheduleConfigurationResource?> GetByIdAsync(int id, int userId)
        {
            _logger.LogInformation($"GetByIdAsync: Fetching configuration {id} for user {userId}");

            var configuration = await _repository.GetByIdAsync(id);
            if (configuration == null)
            {
                _logger.LogInformation($"GetByIdAsync: ScheduleConfiguration {id} not found");
                return null;
            }

            if (configuration.UserId != userId)
            {
                _logger.LogWarning($"GetByIdAsync: ScheduleConfiguration {id} not owned by user {userId}");
                throw new UnauthorizedAccessException($"ScheduleConfiguration {id} not owned by user {userId}");
            }

            _logger.LogInformation($"GetByIdAsync: Found configuration {id} for user {userId}");
            return _mapper.Map<ScheduleConfigurationResource>(configuration);
        }

        public async Task<ScheduleConfigurationResource?> GetActiveAsync(int userId)
        {
            _logger.LogInformation($"GetActiveAsync: Fetching active configuration for user {userId}");

            var configuration = await _repository.GetActiveByUserIdAsync(userId);

            if (configuration != null)
            {
                _logger.LogInformation($"GetActiveAsync: Found active configuration {configuration.Id} for user {userId}");
            }
            else
            {
                _logger.LogInformation($"GetActiveAsync: No active configuration found for user {userId}");
            }

            return configuration != null ? _mapper.Map<ScheduleConfigurationResource>(configuration) : null;
        }

        public async Task<ScheduleConfigurationResource?> GetBySchoolYearAsync(int userId, string schoolYear)
        {
            _logger.LogInformation($"GetBySchoolYearAsync: Fetching configuration for user {userId}, school year {schoolYear}");

            var configuration = await _repository.GetByUserIdAndSchoolYearAsync(userId, schoolYear);

            if (configuration != null)
            {
                _logger.LogInformation($"GetBySchoolYearAsync: Found configuration {configuration.Id} for user {userId}, school year {schoolYear}");
            }
            else
            {
                _logger.LogInformation($"GetBySchoolYearAsync: No configuration found for user {userId}, school year {schoolYear}");
            }

            return configuration != null ? _mapper.Map<ScheduleConfigurationResource>(configuration) : null;
        }

        public async Task<List<ScheduleConfigurationResource>> GetTemplatesAsync(int userId)
        {
            _logger.LogInformation($"GetTemplatesAsync: Fetching templates for user {userId}");

            var templates = await _repository.GetTemplatesAsync(userId);

            _logger.LogInformation($"GetTemplatesAsync: Found {templates.Count} templates for user {userId}");
            return _mapper.Map<List<ScheduleConfigurationResource>>(templates);
        }

        public async Task DeleteAsync(int id, int userId)
        {
            _logger.LogInformation($"DeleteAsync: Deleting configuration {id} for user {userId}");

            // Validate ownership
            var configuration = await _repository.GetByIdAsync(id);
            if (configuration == null)
            {
                throw new ArgumentException($"ScheduleConfiguration {id} not found");
            }

            if (configuration.UserId != userId)
            {
                throw new UnauthorizedAccessException($"ScheduleConfiguration {id} not owned by user {userId}");
            }

            await _repository.DeleteAsync(id);

            _logger.LogInformation($"DeleteAsync: Deleted configuration {id} for user {userId}");
        }

        public async Task<ScheduleConfigurationResource> SetActiveAsync(int id, int userId)
        {
            _logger.LogInformation($"SetActiveAsync: Setting configuration {id} as active for user {userId}");

            var updated = await _repository.SetActiveConfigurationAsync(userId, id);

            _logger.LogInformation($"SetActiveAsync: Set configuration {id} as active for user {userId}");
            return _mapper.Map<ScheduleConfigurationResource>(updated);
        }

        public async Task<ScheduleConfigurationResource> CopyAsTemplateAsync(int id, CopyConfigurationRequest request, int userId)
        {
            _logger.LogInformation($"CopyAsTemplateAsync: Copying configuration {id} as template '{request.NewTitle}' for user {userId}");

            // Validate ownership of source
            var source = await _repository.GetByIdAsync(id);
            if (source == null)
            {
                throw new ArgumentException($"ScheduleConfiguration {id} not found");
            }

            if (source.UserId != userId)
            {
                throw new UnauthorizedAccessException($"ScheduleConfiguration {id} not owned by user {userId}");
            }

            var copied = await _repository.CopyAsTemplateAsync(id, request.NewTitle);

            _logger.LogInformation($"CopyAsTemplateAsync: Copied configuration {id} as template {copied.Id} '{request.NewTitle}' for user {userId}");
            return _mapper.Map<ScheduleConfigurationResource>(copied);
        }

        public async Task<ScheduleConfigurationValidationResource> ValidateAsync(int id, int userId)
        {
            _logger.LogInformation($"ValidateAsync: Validating configuration {id} for user {userId}");

            var configuration = await GetByIdAsync(id, userId);
            if (configuration == null)
            {
                throw new ArgumentException($"ScheduleConfiguration {id} not found");
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

            _logger.LogInformation($"ValidateAsync: Validation complete for configuration {id} - Valid: {validation.IsValid}, CanGenerate: {validation.CanGenerateSchedule}");
            return validation;
        }

        public async Task<List<ScheduleConfigurationSummaryResource>> GetSummariesAsync(int userId)
        {
            _logger.LogInformation($"GetSummariesAsync: Fetching summaries for user {userId}");

            var configurations = await _repository.GetByUserIdAsync(userId);

            // Map to summary resources manually since AutoMapper mapping would be complex
            var summaries = configurations.Select(config => new ScheduleConfigurationSummaryResource
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

            _logger.LogInformation($"GetSummariesAsync: Found {summaries.Count} summaries for user {userId}");
            return summaries;
        }

        public async Task<ScheduleConfigurationResource> CreateAsync(ScheduleConfigurationCreateResource resource, int userId)
        {
            _logger.LogInformation($"CreateAsync: Creating configuration '{resource.Title}' for user {userId}");

            var configuration = _mapper.Map<ScheduleConfiguration>(resource);
            configuration.UserId = userId; // Set ownership

            // Always auto-compute school year from dates
            configuration.SchoolYear = ComputeSchoolYear(configuration.StartDate, configuration.EndDate);
            _logger.LogDebug($"CreateAsync: Auto-computed school year '{configuration.SchoolYear}' for configuration '{resource.Title}'");

            var created = await _repository.CreateAsync(configuration);

            _logger.LogInformation($"CreateAsync: Created configuration {created.Id} '{resource.Title}' (School Year: {created.SchoolYear}) for user {userId}");
            return _mapper.Map<ScheduleConfigurationResource>(created);
        }

        public async Task<ScheduleConfigurationResource> UpdateAsync(int id, ScheduleConfigurationUpdateResource resource, int userId)
        {
            _logger.LogInformation($"UpdateAsync: Updating configuration {id} for user {userId}");

            // Validate ownership first
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
            {
                throw new ArgumentException($"ScheduleConfiguration {id} not found");
            }

            if (existing.UserId != userId)
            {
                throw new UnauthorizedAccessException($"ScheduleConfiguration {id} not owned by user {userId}");
            }

            var configuration = _mapper.Map<ScheduleConfiguration>(resource);
            configuration.UserId = userId; // Maintain ownership

            // Always auto-compute school year from dates
            configuration.SchoolYear = ComputeSchoolYear(configuration.StartDate, configuration.EndDate);
            _logger.LogDebug($"UpdateAsync: Auto-computed school year '{configuration.SchoolYear}' for configuration {id}");

            var updated = await _repository.UpdateAsync(configuration);

            _logger.LogInformation($"UpdateAsync: Updated configuration {id} '{resource.Title}' (School Year: {updated.SchoolYear}) for user {userId}");
            return _mapper.Map<ScheduleConfigurationResource>(updated);
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

    }
}
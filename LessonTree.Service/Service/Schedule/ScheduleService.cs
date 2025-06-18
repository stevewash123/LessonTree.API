using AutoMapper;
using LessonTree.BLL.Services;
using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
using LessonTree.Models.DTO;
using Microsoft.Extensions.Logging;

namespace LessonTree.BLL.Services
{
    /// <summary>
    /// Service for schedule operations
    /// Handles CRUD operations for user schedules and special days
    /// </summary>
    public class ScheduleService : IScheduleService
    {
        private readonly IScheduleRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<ScheduleService> _logger;

        public ScheduleService(IScheduleRepository repository, IMapper mapper, ILogger<ScheduleService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        // === CORE SCHEDULE OPERATIONS ===

        public async Task<ScheduleResource?> GetUserScheduleAsync(int userId)
        {
            _logger.LogDebug("Getting schedule for user {UserId}", userId);

            var schedule = await _repository.GetByUserIdAsync(userId);
            if (schedule == null)
            {
                _logger.LogInformation("No schedule found for user {UserId}", userId);
                return null;
            }

            _logger.LogDebug("Schedule {ScheduleId} retrieved for user {UserId}", schedule.Id, userId);
            return _mapper.Map<ScheduleResource>(schedule);
        }

        public async Task<ScheduleResource?> GetByIdAsync(int id, int userId)
        {
            _logger.LogDebug("Getting schedule {ScheduleId} for user {UserId}", id, userId);

            var schedule = await _repository.GetByIdAsync(id);
            if (schedule == null)
            {
                _logger.LogWarning("Schedule {ScheduleId} not found", id);
                return null;
            }

            // Validate ownership
            if (schedule.UserId != userId)
            {
                _logger.LogWarning("Schedule {ScheduleId} not owned by user {UserId}", id, userId);
                throw new UnauthorizedAccessException("Schedule not owned by user");
            }

            _logger.LogDebug("Schedule {ScheduleId} retrieved successfully for user {UserId}", id, userId);
            return _mapper.Map<ScheduleResource>(schedule);
        }

        public async Task<ScheduleResource> CreateScheduleAsync(ScheduleCreateResource createResource, int userId)
        {
            _logger.LogDebug("Creating schedule '{Title}' for user {UserId}", createResource.Title, userId);

            // Map events to domain objects
            var scheduleEvents = new List<ScheduleEvent>();
            if (createResource.ScheduleEvents != null)
            {
                scheduleEvents = _mapper.Map<List<ScheduleEvent>>(createResource.ScheduleEvents);
                _logger.LogDebug("Mapped {EventCount} schedule events for user {UserId}", scheduleEvents.Count, userId);
            }

            // Create or replace schedule with UI-generated events
            var createdSchedule = await _repository.CreateOrReplaceScheduleAsync(
                userId,
                scheduleEvents,
                createResource.ScheduleConfigurationId);

            _logger.LogInformation("Created schedule {ScheduleId} '{Title}' for user {UserId}",
                createdSchedule.Id, createResource.Title, userId);

            return _mapper.Map<ScheduleResource>(createdSchedule);
        }

        public async Task<ScheduleResource> UpdateScheduleEventsAsync(int scheduleId, List<ScheduleEventResource> events, int userId)
        {
            _logger.LogDebug("Updating events for schedule {ScheduleId}, user {UserId}", scheduleId, userId);

            // Validate ownership
            var existingSchedule = await _repository.GetByIdAsync(scheduleId);
            if (existingSchedule == null)
            {
                _logger.LogWarning("Schedule {ScheduleId} not found for update", scheduleId);
                throw new ArgumentException("Schedule not found");
            }

            if (existingSchedule.UserId != userId)
            {
                _logger.LogWarning("Schedule {ScheduleId} not owned by user {UserId}", scheduleId, userId);
                throw new UnauthorizedAccessException("Schedule not owned by user");
            }

            // Map events
            var scheduleEvents = _mapper.Map<List<ScheduleEvent>>(events);
            _logger.LogDebug("Mapped {EventCount} schedule events for update", scheduleEvents.Count);

            var updatedSchedule = await _repository.UpdateScheduleEventsAsync(scheduleId, scheduleEvents);

            _logger.LogInformation("Updated schedule events for schedule {ScheduleId}", scheduleId);
            return _mapper.Map<ScheduleResource>(updatedSchedule);
        }

        public async Task DeleteUserScheduleAsync(int userId)
        {
            _logger.LogDebug("Deleting schedule for user {UserId}", userId);

            await _repository.DeleteScheduleAsync(userId);

            _logger.LogInformation("Deleted schedule for user {UserId}", userId);
        }

        // === SPECIAL DAY OPERATIONS ===

        public async Task<List<SpecialDayResource>> GetSpecialDaysAsync(int scheduleId, int userId)
        {
            _logger.LogDebug("Getting special days for schedule {ScheduleId}, user {UserId}", scheduleId, userId);

            var schedule = await ValidateScheduleOwnershipAsync(scheduleId, userId);

            var specialDayResources = _mapper.Map<List<SpecialDayResource>>(schedule.SpecialDays);
            _logger.LogDebug("Found {SpecialDayCount} special days for schedule {ScheduleId}", specialDayResources.Count, scheduleId);

            return specialDayResources;
        }

        public async Task<SpecialDayResource?> GetSpecialDayAsync(int scheduleId, int specialDayId, int userId)
        {
            _logger.LogDebug("Getting special day {SpecialDayId} for schedule {ScheduleId}, user {UserId}",
                specialDayId, scheduleId, userId);

            var schedule = await ValidateScheduleOwnershipAsync(scheduleId, userId);

            var specialDay = schedule.SpecialDays.FirstOrDefault(sd => sd.Id == specialDayId);
            if (specialDay == null)
            {
                _logger.LogWarning("Special day {SpecialDayId} not found in schedule {ScheduleId}", specialDayId, scheduleId);
                return null;
            }

            _logger.LogDebug("Special day {SpecialDayId} retrieved successfully", specialDayId);
            return _mapper.Map<SpecialDayResource>(specialDay);
        }

        public async Task<SpecialDayResource> CreateSpecialDayAsync(int scheduleId, SpecialDayCreateResource createResource, int userId)
        {
            _logger.LogDebug("Creating special day for schedule {ScheduleId}, user {UserId}", scheduleId, userId);

            await ValidateScheduleOwnershipAsync(scheduleId, userId);

            var createdSpecialDay = await _repository.AddSpecialDayAsync(scheduleId, createResource);

            _logger.LogInformation("Created special day {SpecialDayId} for schedule {ScheduleId}",
                createdSpecialDay.Id, scheduleId);

            return _mapper.Map<SpecialDayResource>(createdSpecialDay);
        }

        public async Task<SpecialDayResource> UpdateSpecialDayAsync(int scheduleId, int specialDayId, SpecialDayUpdateResource updateResource, int userId)
        {
            _logger.LogDebug("Updating special day {SpecialDayId} for schedule {ScheduleId}, user {UserId}",
                specialDayId, scheduleId, userId);

            if (specialDayId != updateResource.Id)
            {
                throw new ArgumentException("Special day ID mismatch");
            }

            var schedule = await ValidateScheduleOwnershipAsync(scheduleId, userId);

            var existingSpecialDay = schedule.SpecialDays.FirstOrDefault(sd => sd.Id == specialDayId);
            if (existingSpecialDay == null)
            {
                _logger.LogWarning("Special day {SpecialDayId} not found in schedule {ScheduleId}", specialDayId, scheduleId);
                throw new ArgumentException("Special day not found in schedule");
            }

            var updatedSpecialDay = await _repository.UpdateSpecialDayAsync(updateResource);

            _logger.LogInformation("Updated special day {SpecialDayId} successfully", specialDayId);
            return _mapper.Map<SpecialDayResource>(updatedSpecialDay);
        }

        public async Task DeleteSpecialDayAsync(int scheduleId, int specialDayId, int userId)
        {
            _logger.LogDebug("Deleting special day {SpecialDayId} for schedule {ScheduleId}, user {UserId}",
                specialDayId, scheduleId, userId);

            var schedule = await ValidateScheduleOwnershipAsync(scheduleId, userId);

            var existingSpecialDay = schedule.SpecialDays.FirstOrDefault(sd => sd.Id == specialDayId);
            if (existingSpecialDay == null)
            {
                _logger.LogWarning("Special day {SpecialDayId} not found in schedule {ScheduleId}", specialDayId, scheduleId);
                throw new ArgumentException("Special day not found in schedule");
            }

            await _repository.DeleteSpecialDayAsync(specialDayId);

            _logger.LogInformation("Deleted special day {SpecialDayId} successfully", specialDayId);
        }

        // === CONFIGURATION-BASED SCHEDULE LOOKUP ===

        public async Task<ScheduleResource?> GetByConfigurationIdAsync(int configurationId, int userId)
        {
            _logger.LogDebug("Getting schedule for configuration {ConfigurationId}, user {UserId}", configurationId, userId);

            var schedule = await _repository.GetByConfigurationIdAsync(configurationId);
            if (schedule == null)
            {
                _logger.LogInformation("No schedule found for configuration {ConfigurationId}", configurationId);
                return null;
            }

            // Validate ownership
            if (schedule.UserId != userId)
            {
                _logger.LogWarning("Schedule {ScheduleId} for configuration {ConfigurationId} not owned by user {UserId}",
                    schedule.Id, configurationId, userId);
                throw new UnauthorizedAccessException("Schedule not owned by user");
            }

            _logger.LogDebug("Schedule {ScheduleId} retrieved for configuration {ConfigurationId}, user {UserId}",
                schedule.Id, configurationId, userId);
            return _mapper.Map<ScheduleResource>(schedule);
        }

        // === PRIVATE HELPER METHODS ===

        private async Task<Schedule> ValidateScheduleOwnershipAsync(int scheduleId, int userId)
        {
            var schedule = await _repository.GetByIdAsync(scheduleId);
            if (schedule == null)
            {
                _logger.LogWarning("Schedule {ScheduleId} not found", scheduleId);
                throw new ArgumentException("Schedule not found");
            }

            if (schedule.UserId != userId)
            {
                _logger.LogWarning("Schedule {ScheduleId} not owned by user {UserId}", scheduleId, userId);
                throw new UnauthorizedAccessException("Schedule not owned by user");
            }

            return schedule;
        }
    }
}
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
            _logger.LogInformation($"GetUserScheduleAsync: Fetching schedule for user {userId}");

            var schedule = await _repository.GetByUserIdAsync(userId);
            if (schedule == null)
            {
                _logger.LogInformation($"GetUserScheduleAsync: No schedule found for user {userId}");
                return null;
            }

            _logger.LogInformation($"GetUserScheduleAsync: Found schedule {schedule.Id} for user {userId}");
            return _mapper.Map<ScheduleResource>(schedule);
        }

        public async Task<ScheduleResource?> GetByIdAsync(int id, int userId)
        {
            _logger.LogInformation($"GetByIdAsync: Fetching schedule {id} for user {userId}");

            var schedule = await _repository.GetByIdAsync(id);
            if (schedule == null)
            {
                _logger.LogInformation($"GetByIdAsync: Schedule {id} not found");
                return null;
            }

            // Validate ownership
            if (schedule.UserId != userId)
            {
                _logger.LogWarning($"GetByIdAsync: Schedule {id} not owned by user {userId}");
                throw new UnauthorizedAccessException($"Schedule {id} not owned by user {userId}");
            }

            _logger.LogInformation($"GetByIdAsync: Found schedule {id} for user {userId}");
            return _mapper.Map<ScheduleResource>(schedule);
        }

        public async Task<ScheduleResource> CreateScheduleAsync(ScheduleCreateResource createResource, int userId)
        {
            _logger.LogInformation($"CreateScheduleAsync: Creating schedule '{createResource.Title}' for user {userId}");

            // Map events to domain objects
            var scheduleEvents = new List<ScheduleEvent>();
            if (createResource.ScheduleEvents != null)
            {
                scheduleEvents = _mapper.Map<List<ScheduleEvent>>(createResource.ScheduleEvents);
                _logger.LogDebug($"CreateScheduleAsync: Mapped {scheduleEvents.Count} schedule events for user {userId}");
            }

            // Create or replace schedule with UI-generated events
            var createdSchedule = await _repository.CreateOrReplaceScheduleAsync(
                userId,
                scheduleEvents,
                createResource.ScheduleConfigurationId);

            _logger.LogInformation($"CreateScheduleAsync: Created schedule {createdSchedule.Id} '{createResource.Title}' for user {userId}");
            return _mapper.Map<ScheduleResource>(createdSchedule);
        }

        public async Task<ScheduleResource> UpdateScheduleEventsAsync(int scheduleId, List<ScheduleEventResource> events, int userId)
        {
            _logger.LogInformation($"UpdateScheduleEventsAsync: Updating events for schedule {scheduleId}, user {userId}");

            // Validate ownership
            var existingSchedule = await _repository.GetByIdAsync(scheduleId);
            if (existingSchedule == null)
            {
                _logger.LogInformation($"UpdateScheduleEventsAsync: Schedule {scheduleId} not found");
                throw new ArgumentException($"Schedule {scheduleId} not found");
            }

            if (existingSchedule.UserId != userId)
            {
                _logger.LogWarning($"UpdateScheduleEventsAsync: Schedule {scheduleId} not owned by user {userId}");
                throw new UnauthorizedAccessException($"Schedule {scheduleId} not owned by user {userId}");
            }

            // Map events
            var scheduleEvents = _mapper.Map<List<ScheduleEvent>>(events);
            _logger.LogDebug($"UpdateScheduleEventsAsync: Mapped {scheduleEvents.Count} schedule events for update");

            var updatedSchedule = await _repository.UpdateScheduleEventsAsync(scheduleId, scheduleEvents);

            _logger.LogInformation($"UpdateScheduleEventsAsync: Updated {events.Count} events for schedule {scheduleId}");
            return _mapper.Map<ScheduleResource>(updatedSchedule);
        }

        public async Task DeleteUserScheduleAsync(int userId)
        {
            _logger.LogInformation($"DeleteUserScheduleAsync: Deleting schedule for user {userId}");

            await _repository.DeleteScheduleAsync(userId);

            _logger.LogInformation($"DeleteUserScheduleAsync: Deleted schedule for user {userId}");
        }

        // === SPECIAL DAY OPERATIONS ===

        public async Task<List<SpecialDayResource>> GetSpecialDaysAsync(int scheduleId, int userId)
        {
            _logger.LogInformation($"GetSpecialDaysAsync: Fetching special days for schedule {scheduleId}, user {userId}");

            var schedule = await ValidateScheduleOwnershipAsync(scheduleId, userId);

            var specialDayResources = _mapper.Map<List<SpecialDayResource>>(schedule.SpecialDays);

            _logger.LogInformation($"GetSpecialDaysAsync: Found {specialDayResources.Count} special days for schedule {scheduleId}");
            return specialDayResources;
        }

        public async Task<SpecialDayResource?> GetSpecialDayAsync(int scheduleId, int specialDayId, int userId)
        {
            _logger.LogInformation($"GetSpecialDayAsync: Fetching special day {specialDayId} for schedule {scheduleId}, user {userId}");

            var schedule = await ValidateScheduleOwnershipAsync(scheduleId, userId);

            var specialDay = schedule.SpecialDays.FirstOrDefault(sd => sd.Id == specialDayId);
            if (specialDay == null)
            {
                _logger.LogInformation($"GetSpecialDayAsync: SpecialDay {specialDayId} not found in schedule {scheduleId}");
                return null;
            }

            _logger.LogInformation($"GetSpecialDayAsync: Found special day {specialDayId} in schedule {scheduleId}");
            return _mapper.Map<SpecialDayResource>(specialDay);
        }

        public async Task<SpecialDayResource> CreateSpecialDayAsync(int scheduleId, SpecialDayCreateResource createResource, int userId)
        {
            _logger.LogInformation($"CreateSpecialDayAsync: Creating special day for schedule {scheduleId}, user {userId}");

            await ValidateScheduleOwnershipAsync(scheduleId, userId);

            var createdSpecialDay = await _repository.AddSpecialDayAsync(scheduleId, createResource);

            _logger.LogInformation($"CreateSpecialDayAsync: Created special day {createdSpecialDay.Id} for schedule {scheduleId}");
            return _mapper.Map<SpecialDayResource>(createdSpecialDay);
        }

        public async Task<SpecialDayResource> UpdateSpecialDayAsync(int scheduleId, int specialDayId, SpecialDayUpdateResource updateResource, int userId)
        {
            _logger.LogInformation($"UpdateSpecialDayAsync: Updating special day {specialDayId} for schedule {scheduleId}, user {userId}");

            if (specialDayId != updateResource.Id)
            {
                throw new ArgumentException($"SpecialDay ID mismatch: {specialDayId} vs {updateResource.Id}");
            }

            var schedule = await ValidateScheduleOwnershipAsync(scheduleId, userId);

            var existingSpecialDay = schedule.SpecialDays.FirstOrDefault(sd => sd.Id == specialDayId);
            if (existingSpecialDay == null)
            {
                _logger.LogInformation($"UpdateSpecialDayAsync: SpecialDay {specialDayId} not found in schedule {scheduleId}");
                throw new ArgumentException($"SpecialDay {specialDayId} not found in schedule {scheduleId}");
            }

            var updatedSpecialDay = await _repository.UpdateSpecialDayAsync(updateResource);

            _logger.LogInformation($"UpdateSpecialDayAsync: Updated special day {specialDayId} for schedule {scheduleId}");
            return _mapper.Map<SpecialDayResource>(updatedSpecialDay);
        }

        public async Task DeleteSpecialDayAsync(int scheduleId, int specialDayId, int userId)
        {
            _logger.LogInformation($"DeleteSpecialDayAsync: Deleting special day {specialDayId} for schedule {scheduleId}, user {userId}");

            var schedule = await ValidateScheduleOwnershipAsync(scheduleId, userId);

            var existingSpecialDay = schedule.SpecialDays.FirstOrDefault(sd => sd.Id == specialDayId);
            if (existingSpecialDay == null)
            {
                _logger.LogInformation($"DeleteSpecialDayAsync: SpecialDay {specialDayId} not found in schedule {scheduleId}");
                throw new ArgumentException($"SpecialDay {specialDayId} not found in schedule {scheduleId}");
            }

            await _repository.DeleteSpecialDayAsync(specialDayId);

            _logger.LogInformation($"DeleteSpecialDayAsync: Deleted special day {specialDayId} for schedule {scheduleId}");
        }

        // === CONFIGURATION-BASED SCHEDULE LOOKUP ===

        public async Task<ScheduleResource?> GetByConfigurationIdAsync(int configurationId, int userId)
        {
            _logger.LogInformation($"GetByConfigurationIdAsync: Fetching schedule for configuration {configurationId}, user {userId}");

            var schedule = await _repository.GetByConfigurationIdAsync(configurationId);
            if (schedule == null)
            {
                _logger.LogInformation($"GetByConfigurationIdAsync: No schedule found for configuration {configurationId}");
                return null;
            }

            // Validate ownership
            if (schedule.UserId != userId)
            {
                _logger.LogWarning($"GetByConfigurationIdAsync: Schedule {schedule.Id} for configuration {configurationId} not owned by user {userId}");
                throw new UnauthorizedAccessException($"Schedule {schedule.Id} not owned by user {userId}");
            }

            _logger.LogInformation($"GetByConfigurationIdAsync: Found schedule {schedule.Id} for configuration {configurationId}, user {userId}");
            return _mapper.Map<ScheduleResource>(schedule);
        }

        // === PRIVATE HELPER METHODS ===

        private async Task<Schedule> ValidateScheduleOwnershipAsync(int scheduleId, int userId)
        {
            var schedule = await _repository.GetByIdAsync(scheduleId);
            if (schedule == null)
            {
                _logger.LogInformation($"ValidateScheduleOwnershipAsync: Schedule {scheduleId} not found");
                throw new ArgumentException($"Schedule {scheduleId} not found");
            }

            if (schedule.UserId != userId)
            {
                _logger.LogWarning($"ValidateScheduleOwnershipAsync: Schedule {scheduleId} not owned by user {userId}");
                throw new UnauthorizedAccessException($"Schedule {scheduleId} not owned by user {userId}");
            }

            return schedule;
        }
    }
}
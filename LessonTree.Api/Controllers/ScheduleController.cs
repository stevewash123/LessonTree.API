// **COMPLETE FILE** - Updated ScheduleController for Master Schedule Support
// RESPONSIBILITY: Manages user's master schedule (cross-course) with JWT-based authentication
// DOES NOT: Handle course-specific schedules, individual event CRUD (separate controller)
// CALLED BY: Frontend calendar components for schedule management

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;
using LessonTree.DAL.Repositories;
using System.Security.Claims;

namespace LessonTree.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ScheduleController> _logger;

        public ScheduleController(
            IScheduleRepository scheduleRepository,
            IMapper mapper,
            ILogger<ScheduleController> logger)
        {
            _scheduleRepository = scheduleRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // GET /api/Schedule - Get user's master schedule (JWT-based)
        [HttpGet]
        public async Task<ActionResult<ScheduleResource>> GetUserSchedule()
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                _logger.LogWarning("GetUserSchedule: User ID not found in token");
                return Unauthorized("User ID not found in token");
            }

            _logger.LogInformation($"GetUserSchedule: Fetching schedule for user {userId}");

            var schedule = await _scheduleRepository.GetByUserIdAsync(userId.Value);
            if (schedule == null)
            {
                _logger.LogInformation($"GetUserSchedule: No schedule found for user {userId}");
                return NotFound($"No schedule found for user {userId}");
            }

            var scheduleResource = _mapper.Map<ScheduleResource>(schedule);
            _logger.LogInformation($"GetUserSchedule: Returning schedule {schedule.Id} for user {userId}");

            return Ok(scheduleResource);
        }

        // GET /api/Schedule/{id} - Get specific schedule by ID (with ownership check)
        [HttpGet("{id}")]
        public async Task<ActionResult<ScheduleResource>> GetSchedule(int id)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized("User ID not found in token");
            }

            _logger.LogInformation($"GetSchedule: Fetching schedule {id} for user {userId}");

            var schedule = await _scheduleRepository.GetByIdAsync(id);
            if (schedule == null)
            {
                _logger.LogWarning($"GetSchedule: Schedule {id} not found");
                return NotFound($"Schedule {id} not found");
            }

            // Verify ownership
            if (schedule.UserId != userId.Value)
            {
                _logger.LogWarning($"GetSchedule: User {userId} attempted to access schedule {id} owned by {schedule.UserId}");
                return Forbid("You can only access your own schedules");
            }

            var scheduleResource = _mapper.Map<ScheduleResource>(schedule);
            return Ok(scheduleResource);
        }

        // POST /api/Schedule - Create user's master schedule
        [HttpPost]
        public async Task<ActionResult<ScheduleResource>> CreateSchedule([FromBody] ScheduleCreateResource createResource)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized("User ID not found in token");
            }

            _logger.LogInformation($"CreateSchedule: Creating schedule for user {userId}");

            // Check if user already has a schedule
            var existingSchedule = await _scheduleRepository.GetByUserIdAsync(userId.Value);
            if (existingSchedule != null)
            {
                _logger.LogWarning($"CreateSchedule: User {userId} already has schedule {existingSchedule.Id}");
                return Conflict($"User already has a schedule (ID: {existingSchedule.Id}). Use PUT to update.");
            }

            // Validate date range
            if (createResource.StartDate >= createResource.EndDate)
            {
                return BadRequest("Start date must be before end date");
            }

            var schedule = _mapper.Map<Schedule>(createResource);
            schedule.UserId = userId.Value;

            var createdSchedule = await _scheduleRepository.CreateAsync(schedule);
            var scheduleResource = _mapper.Map<ScheduleResource>(createdSchedule);

            _logger.LogInformation($"CreateSchedule: Created schedule {createdSchedule.Id} for user {userId}");

            return CreatedAtAction(nameof(GetSchedule), new { id = createdSchedule.Id }, scheduleResource);
        }

        // PUT /api/Schedule/{id}/config - Update schedule configuration
        [HttpPut("{id}/config")]
        public async Task<ActionResult<ScheduleResource>> UpdateScheduleConfig(int id, [FromBody] ScheduleConfigUpdateResource updateResource)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized("User ID not found in token");
            }

            if (id != updateResource.Id)
            {
                return BadRequest("Schedule ID mismatch");
            }

            _logger.LogInformation($"UpdateScheduleConfig: Updating schedule {id} config for user {userId}");

            var existingSchedule = await _scheduleRepository.GetByIdAsync(id);
            if (existingSchedule == null)
            {
                return NotFound($"Schedule {id} not found");
            }

            // Verify ownership
            if (existingSchedule.UserId != userId.Value)
            {
                _logger.LogWarning($"UpdateScheduleConfig: User {userId} attempted to update schedule {id} owned by {existingSchedule.UserId}");
                return Forbid("You can only update your own schedules");
            }

            // Check if schedule is locked
            if (existingSchedule.IsLocked)
            {
                return BadRequest("Cannot update configuration of a locked schedule");
            }

            // Validate date range
            if (updateResource.StartDate >= updateResource.EndDate)
            {
                return BadRequest("Start date must be before end date");
            }

            // FIXED: Use AutoMapper instead of direct assignment for TeachingDays conversion
            _mapper.Map(updateResource, existingSchedule);

            var updatedSchedule = await _scheduleRepository.UpdateAsync(existingSchedule);
            var scheduleResource = _mapper.Map<ScheduleResource>(updatedSchedule);

            _logger.LogInformation($"UpdateScheduleConfig: Updated schedule {id} config for user {userId}");

            return Ok(scheduleResource);
        }

        // PUT /api/Schedule/{id}/events - Bulk update schedule events
        [HttpPut("{id}/events")]
        public async Task<ActionResult<ScheduleResource>> UpdateScheduleEvents(int id, [FromBody] ScheduleEventsUpdateResource eventsUpdate)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized("User ID not found in token");
            }

            if (id != eventsUpdate.ScheduleId)
            {
                return BadRequest("Schedule ID mismatch");
            }

            _logger.LogInformation($"UpdateScheduleEvents: Updating {eventsUpdate.ScheduleEvents.Count} events for schedule {id}");

            var existingSchedule = await _scheduleRepository.GetByIdAsync(id);
            if (existingSchedule == null)
            {
                return NotFound($"Schedule {id} not found");
            }

            // Verify ownership
            if (existingSchedule.UserId != userId.Value)
            {
                _logger.LogWarning($"UpdateScheduleEvents: User {userId} attempted to update schedule {id} owned by {existingSchedule.UserId}");
                return Forbid("You can only update your own schedules");
            }

            // Check if schedule is locked
            if (existingSchedule.IsLocked)
            {
                return BadRequest("Cannot update events of a locked schedule");
            }

            // Validate events
            var validationErrors = ValidateScheduleEvents(eventsUpdate.ScheduleEvents);
            if (validationErrors.Any())
            {
                return BadRequest($"Event validation failed: {string.Join(", ", validationErrors)}");
            }

            // Map events and assign schedule ID
            var scheduleEvents = _mapper.Map<List<ScheduleEvent>>(eventsUpdate.ScheduleEvents);
            foreach (var scheduleEvent in scheduleEvents)
            {
                scheduleEvent.ScheduleId = id;
            }

            var updatedSchedule = await _scheduleRepository.UpdateScheduleEventsAsync(id, scheduleEvents);
            var scheduleResource = _mapper.Map<ScheduleResource>(updatedSchedule);

            _logger.LogInformation($"UpdateScheduleEvents: Updated {scheduleEvents.Count} events for schedule {id}");

            return Ok(scheduleResource);
        }

        // DELETE /api/Schedule/{id} - Delete schedule
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteSchedule(int id)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized("User ID not found in token");
            }

            _logger.LogInformation($"DeleteSchedule: Deleting schedule {id} for user {userId}");

            var existingSchedule = await _scheduleRepository.GetByIdAsync(id);
            if (existingSchedule == null)
            {
                return NotFound($"Schedule {id} not found");
            }

            // Verify ownership
            if (existingSchedule.UserId != userId.Value)
            {
                _logger.LogWarning($"DeleteSchedule: User {userId} attempted to delete schedule {id} owned by {existingSchedule.UserId}");
                return Forbid("You can only delete your own schedules");
            }

            await _scheduleRepository.DeleteAsync(id);

            _logger.LogInformation($"DeleteSchedule: Deleted schedule {id} for user {userId}");

            return NoContent();
        }


        // === USER-BASED MASTER SCHEDULE ENDPOINTS ===

        // GET /api/User/masterSchedule - Get current user's master schedule
        [HttpGet("~/api/User/masterSchedule")]
        public async Task<ActionResult<ScheduleResource>> GetUserMasterSchedule()
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                _logger.LogWarning("GetUserMasterSchedule: User ID not found in token");
                return Unauthorized("User ID not found in token");
            }

            _logger.LogInformation($"GetUserMasterSchedule: Fetching master schedule for user {userId}");

            var schedule = await _scheduleRepository.GetByUserIdAsync(userId.Value);
            if (schedule == null)
            {
                _logger.LogInformation($"GetUserMasterSchedule: No master schedule found for user {userId}");
                return NotFound($"No master schedule found for user {userId}");
            }

            var scheduleResource = _mapper.Map<ScheduleResource>(schedule);
            _logger.LogInformation($"GetUserMasterSchedule: Returning master schedule {schedule.Id} with {schedule.ScheduleEvents.Count} events");

            return Ok(scheduleResource);
        }

        // POST /api/User/masterSchedule - Create master schedule with full event payload
        [HttpPost("~/api/User/masterSchedule")]
        public async Task<ActionResult<ScheduleResource>> CreateUserMasterSchedule([FromBody] ScheduleCreateResource createResource)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized("User ID not found in token");
            }

            _logger.LogInformation($"CreateUserMasterSchedule: Creating master schedule for user {userId} with {createResource.ScheduleEvents?.Count ?? 0} events");

            // Check if user already has a master schedule
            var existingSchedule = await _scheduleRepository.GetByUserIdAsync(userId.Value);
            if (existingSchedule != null)
            {
                _logger.LogWarning($"CreateUserMasterSchedule: User {userId} already has master schedule {existingSchedule.Id}");
                return Conflict($"User already has a master schedule (ID: {existingSchedule.Id}). Use PUT to update.");
            }

            // Validate date range
            if (createResource.StartDate >= createResource.EndDate)
            {
                return BadRequest("Start date must be before end date");
            }

            // Validate events if provided
            if (createResource.ScheduleEvents != null && createResource.ScheduleEvents.Any())
            {
                var validationErrors = ValidateScheduleEvents(createResource.ScheduleEvents);
                if (validationErrors.Any())
                {
                    return BadRequest($"Event validation failed: {string.Join(", ", validationErrors)}");
                }
            }

            var schedule = _mapper.Map<Schedule>(createResource);
            schedule.UserId = userId.Value;

            // Map events if provided
            if (createResource.ScheduleEvents != null)
            {
                var scheduleEvents = _mapper.Map<List<ScheduleEvent>>(createResource.ScheduleEvents);
                foreach (var evt in scheduleEvents)
                {
                    evt.ScheduleId = 0; // Will be set when schedule is created
                }
                schedule.ScheduleEvents = scheduleEvents;
            }

            var createdSchedule = await _scheduleRepository.CreateAsync(schedule);
            var scheduleResource = _mapper.Map<ScheduleResource>(createdSchedule);

            _logger.LogInformation($"CreateUserMasterSchedule: Created master schedule {createdSchedule.Id} for user {userId} with {createdSchedule.ScheduleEvents.Count} events");

            return CreatedAtAction(nameof(GetSchedule), new { id = createdSchedule.Id }, scheduleResource);
        }

        // PUT /api/User/masterSchedule/{id}/events - Replace all events (regeneration)
        [HttpPut("~/api/User/masterSchedule/{id}/events")]
        public async Task<ActionResult<ScheduleResource>> RegenerateUserMasterSchedule(int id, [FromBody] List<ScheduleEventResource> events)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized("User ID not found in token");
            }

            _logger.LogInformation($"RegenerateUserMasterSchedule: Regenerating {events.Count} events for master schedule {id}");

            var existingSchedule = await _scheduleRepository.GetByIdAsync(id);
            if (existingSchedule == null)
            {
                return NotFound($"Master schedule {id} not found");
            }

            // Verify ownership
            if (existingSchedule.UserId != userId.Value)
            {
                _logger.LogWarning($"RegenerateUserMasterSchedule: User {userId} attempted to regenerate schedule {id} owned by {existingSchedule.UserId}");
                return Forbid("You can only regenerate your own master schedule");
            }

            // Check if schedule is locked
            if (existingSchedule.IsLocked)
            {
                return BadRequest("Cannot regenerate events of a locked master schedule");
            }

            // Validate events
            var validationErrors = ValidateScheduleEvents(events);
            if (validationErrors.Any())
            {
                return BadRequest($"Event validation failed: {string.Join(", ", validationErrors)}");
            }

            // Map events and assign schedule ID
            var scheduleEvents = _mapper.Map<List<ScheduleEvent>>(events);
            foreach (var scheduleEvent in scheduleEvents)
            {
                scheduleEvent.ScheduleId = id;
            }

            var updatedSchedule = await _scheduleRepository.UpdateScheduleEventsAsync(id, scheduleEvents);
            var scheduleResource = _mapper.Map<ScheduleResource>(updatedSchedule);

            _logger.LogInformation($"RegenerateUserMasterSchedule: Regenerated {scheduleEvents.Count} events for master schedule {id}");

            return Ok(scheduleResource);
        }

        // DELETE /api/User/masterSchedule/{id} - Delete master schedule
        [HttpDelete("~/api/User/masterSchedule/{id}")]
        public async Task<ActionResult> DeleteUserMasterSchedule(int id)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized("User ID not found in token");
            }

            _logger.LogInformation($"DeleteUserMasterSchedule: Deleting master schedule {id} for user {userId}");

            var existingSchedule = await _scheduleRepository.GetByIdAsync(id);
            if (existingSchedule == null)
            {
                return NotFound($"Master schedule {id} not found");
            }

            // Verify ownership
            if (existingSchedule.UserId != userId.Value)
            {
                _logger.LogWarning($"DeleteUserMasterSchedule: User {userId} attempted to delete schedule {id} owned by {existingSchedule.UserId}");
                return Forbid("You can only delete your own master schedule");
            }

            await _scheduleRepository.DeleteAsync(id);

            _logger.LogInformation($"DeleteUserMasterSchedule: Deleted master schedule {id} for user {userId}");

            return NoContent();
        }

        // === PRIVATE HELPER METHODS ===

        private int? GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value
                           ?? User.FindFirst("userId")?.Value;

            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            _logger.LogWarning($"GetUserIdFromToken: Could not parse user ID from token. Claims: {string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"))}");
            return null;
        }

        private List<string> ValidateScheduleEvents(List<ScheduleEventResource> events)
        {
            var errors = new List<string>();

            foreach (var evt in events)
            {
                // Validate required EventType
                if (string.IsNullOrWhiteSpace(evt.EventType))
                {
                    errors.Add($"Event on {evt.Date:yyyy-MM-dd} Period {evt.Period}: EventType is required");
                    continue;
                }

                // Validate EventType/EventCategory combinations
                if (!IsValidEventTypeCategory(evt.EventType, evt.EventCategory))
                {
                    errors.Add($"Event on {evt.Date:yyyy-MM-dd} Period {evt.Period}: Invalid EventType '{evt.EventType}' for EventCategory '{evt.EventCategory}'");
                }

                // Validate Period range
                if (evt.Period < 1 || evt.Period > 10)
                {
                    errors.Add($"Event on {evt.Date:yyyy-MM-dd}: Period {evt.Period} must be between 1 and 10");
                }

                // Validate lesson events have CourseId
                if (evt.EventType == "Lesson" && evt.LessonId.HasValue && (!evt.CourseId.HasValue || evt.CourseId <= 0))
                {
                    errors.Add($"Event on {evt.Date:yyyy-MM-dd} Period {evt.Period}: Lesson events must have a valid CourseId");
                }
            }

            return errors;
        }

        private bool IsValidEventTypeCategory(string eventType, string? eventCategory)
        {
            return eventCategory switch
            {
                "Lesson" => eventType == "Lesson",
                "SpecialPeriod" => IsValidSpecialPeriodType(eventType),
                "SpecialDay" => IsValidSpecialDayType(eventType),
                null => eventType is "OverflowError" or "UnderflowError",
                _ => false
            };
        }

        private bool IsValidSpecialPeriodType(string eventType)
        {
            return eventType is "Lunch" or "HallDuty" or "CafeteriaDuty" or "StudyHall" or "Prep" or "OtherDuty";
        }

        private bool IsValidSpecialDayType(string eventType)
        {
            return eventType is "Assembly" or "Testing" or "Holiday" or "ProfessionalDevelopment" or "FieldTrip" or "WeatherDelay" or "EarlyDismissal";
        }
    }
}
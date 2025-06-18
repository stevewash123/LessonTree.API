using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LessonTree.Models.DTO;
using LessonTree.BLL.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using LessonTree.BLL.Services;

namespace LessonTree.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ScheduleController : BaseController
    {
        private readonly IScheduleService _scheduleService;
        private readonly ILogger<ScheduleController> _logger;

        public ScheduleController(
            IScheduleService scheduleService,
            ILogger<ScheduleController> logger)
        {
            _scheduleService = scheduleService;
            _logger = logger;
        }

        // GET /api/Schedule - Get user's schedule (JWT-based)
        [HttpGet]
        public async Task<ActionResult<ScheduleResource>> GetUserSchedule()
        {
            int userId = GetCurrentUserId();

            var schedule = await _scheduleService.GetUserScheduleAsync(userId);
            if (schedule == null)
            {
                return NotFound($"No schedule found for user {userId}");
            }

            return Ok(schedule);
        }

        // GET /api/Schedule/{id} - Get specific schedule by ID (with ownership check)
        [HttpGet("{id}")]
        public async Task<ActionResult<ScheduleResource>> GetSchedule(int id)
        {
            int userId = GetCurrentUserId();

            var schedule = await _scheduleService.GetByIdAsync(id, userId);
            if (schedule == null)
            {
                return NotFound($"Schedule {id} not found or not accessible");
            }

            return Ok(schedule);
        }

        // POST /api/Schedule - Create/replace user's schedule with UI-generated events
        [HttpPost]
        public async Task<ActionResult<ScheduleResource>> CreateSchedule([FromBody] ScheduleCreateResource createResource)
        {
            int userId = GetCurrentUserId();

            try
            {
                var createdSchedule = await _scheduleService.CreateScheduleAsync(createResource, userId);
                return CreatedAtAction(nameof(GetSchedule), new { id = createdSchedule.Id }, createdSchedule);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid schedule creation request for User ID: {UserId} - {Message}", userId, ex.Message);
                return BadRequest(new { status = "error", message = ex.Message });
            }
        }

        // PUT /api/Schedule/{id}/events - Replace all events with new UI-generated events
        [HttpPut("{id}/events")]
        public async Task<ActionResult<ScheduleResource>> UpdateScheduleEvents(int id, [FromBody] List<ScheduleEventResource> events)
        {
            int userId = GetCurrentUserId();

            try
            {
                var updatedSchedule = await _scheduleService.UpdateScheduleEventsAsync(id, events, userId);
                return Ok(updatedSchedule);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid schedule update request for Schedule ID: {ScheduleId}, User ID: {UserId} - {Message}", id, userId, ex.Message);
                return BadRequest(new { status = "error", message = ex.Message });
            }
        }

        // DELETE /api/Schedule - Delete user's schedule
        [HttpDelete]
        public async Task<ActionResult> DeleteUserSchedule()
        {
            int userId = GetCurrentUserId();

            await _scheduleService.DeleteUserScheduleAsync(userId);
            return NoContent();
        }

        // === SPECIAL DAY ENDPOINTS ===

        // GET /api/Schedule/{scheduleId}/specialDays
        [HttpGet("{scheduleId}/specialDays")]
        public async Task<ActionResult<List<SpecialDayResource>>> GetSpecialDays(int scheduleId)
        {
            int userId = GetCurrentUserId();

            try
            {
                var specialDays = await _scheduleService.GetSpecialDaysAsync(scheduleId, userId);
                return Ok(specialDays);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid special days request for Schedule ID: {ScheduleId}, User ID: {UserId} - {Message}", scheduleId, userId, ex.Message);
                return BadRequest(new { status = "error", message = ex.Message });
            }
        }

        // GET /api/Schedule/{scheduleId}/specialDays/{specialDayId}
        [HttpGet("{scheduleId}/specialDays/{specialDayId}")]
        public async Task<ActionResult<SpecialDayResource>> GetSpecialDay(int scheduleId, int specialDayId)
        {
            int userId = GetCurrentUserId();

            try
            {
                var specialDay = await _scheduleService.GetSpecialDayAsync(scheduleId, specialDayId, userId);
                if (specialDay == null)
                {
                    return NotFound($"Special day {specialDayId} not found in schedule {scheduleId}");
                }

                return Ok(specialDay);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid special day request for Schedule ID: {ScheduleId}, Special Day ID: {SpecialDayId}, User ID: {UserId} - {Message}",
                    scheduleId, specialDayId, userId, ex.Message);
                return BadRequest(new { status = "error", message = ex.Message });
            }
        }

        // POST /api/Schedule/{scheduleId}/specialDays
        [HttpPost("{scheduleId}/specialDays")]
        public async Task<ActionResult<SpecialDayResource>> CreateSpecialDay(int scheduleId, [FromBody] SpecialDayCreateResource createResource)
        {
            int userId = GetCurrentUserId();

            try
            {
                var createdSpecialDay = await _scheduleService.CreateSpecialDayAsync(scheduleId, createResource, userId);
                return CreatedAtAction(nameof(GetSpecialDay),
                    new { scheduleId = scheduleId, specialDayId = createdSpecialDay.Id },
                    createdSpecialDay);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid special day creation for Schedule ID: {ScheduleId}, User ID: {UserId} - {Message}", scheduleId, userId, ex.Message);
                return BadRequest(new { status = "error", message = ex.Message });
            }
        }

        // PUT /api/Schedule/{scheduleId}/specialDays/{specialDayId}
        [HttpPut("{scheduleId}/specialDays/{specialDayId}")]
        public async Task<ActionResult<SpecialDayResource>> UpdateSpecialDay(int scheduleId, int specialDayId, [FromBody] SpecialDayUpdateResource updateResource)
        {
            int userId = GetCurrentUserId();

            try
            {
                var updatedSpecialDay = await _scheduleService.UpdateSpecialDayAsync(scheduleId, specialDayId, updateResource, userId);
                return Ok(updatedSpecialDay);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid special day update for Schedule ID: {ScheduleId}, Special Day ID: {SpecialDayId}, User ID: {UserId} - {Message}",
                    scheduleId, specialDayId, userId, ex.Message);
                return BadRequest(new { status = "error", message = ex.Message });
            }
        }

        // DELETE /api/Schedule/{scheduleId}/specialDays/{specialDayId}
        [HttpDelete("{scheduleId}/specialDays/{specialDayId}")]
        public async Task<ActionResult> DeleteSpecialDay(int scheduleId, int specialDayId)
        {
            int userId = GetCurrentUserId();

            try
            {
                await _scheduleService.DeleteSpecialDayAsync(scheduleId, specialDayId, userId);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid special day deletion for Schedule ID: {ScheduleId}, Special Day ID: {SpecialDayId}, User ID: {UserId} - {Message}",
                    scheduleId, specialDayId, userId, ex.Message);
                return BadRequest(new { status = "error", message = ex.Message });
            }
        }

        // GET /api/Schedule/byConfiguration/{configurationId} - Get schedule by configuration ID
        [HttpGet("byConfiguration/{configurationId}")]
        public async Task<ActionResult<ScheduleResource>> GetScheduleByConfigurationId(int configurationId)
        {
            int userId = GetCurrentUserId();

            try
            {
                var schedule = await _scheduleService.GetByConfigurationIdAsync(configurationId, userId);
                if (schedule == null)
                {
                    return NotFound($"No schedule found for configuration {configurationId}");
                }

                return Ok(schedule);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid schedule request for Configuration ID: {ConfigurationId}, User ID: {UserId} - {Message}",
                    configurationId, userId, ex.Message);
                return BadRequest(new { status = "error", message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized schedule access for Configuration ID: {ConfigurationId}, User ID: {UserId} - {Message}",
                    configurationId, userId, ex.Message);
                return Forbid();
            }
        }
    }
}
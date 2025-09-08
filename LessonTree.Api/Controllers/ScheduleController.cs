// **MODIFIED FILE** - Complete Enhanced ScheduleController.cs with Date Range and Continuation Endpoints
// INTEGRATION: Complete ScheduleController.cs with all new endpoints added

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

        // === EXISTING ENDPOINTS ===

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

        // === NEW DATE RANGE LOADING ENDPOINTS (Phase 2 - Paginated Calendar Support) ===

        /// <summary>
        /// Get schedule events for specific date range (paginated calendar loading)
        /// </summary>
        /// <param name="scheduleId">Schedule ID</param>
        /// <param name="startDate">Start date for range</param>
        /// <param name="endDate">End date for range</param>
        /// <returns>Schedule events in date range</returns>
        [HttpGet("{scheduleId}/events/dateRange")]
        public async Task<ActionResult<List<ScheduleEventResource>>> GetEventsByDateRange(
            int scheduleId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int? courseId = null)
        {
            try
            {
                int userId = GetCurrentUserId();

                // Validate schedule ownership
                var schedule = await _scheduleService.GetByIdAsync(scheduleId, userId);
                if (schedule == null)
                {
                    return NotFound($"Schedule {scheduleId} not found or not accessible");
                }

                // Get events (already includes special days from inline generation)
                var events = await _scheduleService.GetEventsByDateRangeAsync(scheduleId, startDate, endDate, userId, courseId);

                _logger.LogInformation($"GetEventsByDateRange: Returning {events.Count} events for schedule {scheduleId} between {startDate:yyyy-MM-dd} and {endDate:yyyy-MM-dd}");

                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events by date range for schedule {ScheduleId}", scheduleId);
                return StatusCode(500, new { status = "error", message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get current week events for calendar (optimized for calendar navigation)
        /// </summary>
        /// <param name="scheduleId">Schedule ID</param>
        /// <param name="weekStartDate">Week start date (typically Monday)</param>
        /// <returns>Events for the week</returns>
        [HttpGet("{scheduleId}/events/week/{weekStartDate}")]
        public async Task<ActionResult<List<ScheduleEventResource>>> GetWeekEvents(int scheduleId, DateTime weekStartDate)
        {
            try
            {
                int userId = GetCurrentUserId();
                var weekEndDate = weekStartDate.AddDays(6); // 7-day week

                return await GetEventsByDateRange(scheduleId, weekStartDate, weekEndDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting week events for schedule {ScheduleId}", scheduleId);
                return StatusCode(500, new { status = "error", message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get current month events for calendar
        /// </summary>
        /// <param name="scheduleId">Schedule ID</param>
        /// <param name="year">Year</param>
        /// <param name="month">Month (1-12)</param>
        /// <returns>Events for the month</returns>
        [HttpGet("{scheduleId}/events/month/{year}/{month}")]
        public async Task<ActionResult<List<ScheduleEventResource>>> GetMonthEvents(int scheduleId, int year, int month)
        {
            try
            {
                int userId = GetCurrentUserId();
                var monthStart = new DateTime(year, month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                return await GetEventsByDateRange(scheduleId, monthStart, monthEnd);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting month events for schedule {ScheduleId}", scheduleId);
                return StatusCode(500, new { status = "error", message = "Internal server error" });
            }
        }

        // === NEW SEQUENCE CONTINUATION ENDPOINTS ===

        /// <summary>
        /// Analyze sequence state for existing schedule
        /// </summary>
        /// <param name="scheduleId">Schedule ID</param>
        /// <param name="afterDate">Date to analyze from</param>
        /// <returns>Sequence analysis result</returns>
        [HttpGet("{scheduleId}/sequenceAnalysis")]
        public async Task<ActionResult<SequenceAnalysisResult>> AnalyzeSequences(
            int scheduleId,
            [FromQuery] DateTime afterDate)
        {
            try
            {
                int userId = GetCurrentUserId();

                var analysis = await _scheduleService.AnalyzeSequenceStateAsync(scheduleId, afterDate, userId);

                return Ok(analysis);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { status = "error", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing sequences for schedule {ScheduleId}", scheduleId);
                return StatusCode(500, new { status = "error", message = "Internal server error" });
            }
        }

        /// <summary>
        /// Continue lesson sequences in existing schedule
        /// </summary>
        /// <param name="scheduleId">Schedule ID</param>
        /// <param name="continuationRequest">Continuation parameters</param>
        /// <returns>Updated schedule with continuation events</returns>
        [HttpPost("{scheduleId}/continueSequences")]
        public async Task<ActionResult<ScheduleResource>> ContinueSequences(
            int scheduleId,
            [FromBody] SequenceContinuationRequest continuationRequest)
        {
            try
            {
                int userId = GetCurrentUserId();

                var updatedSchedule = await _scheduleService.ContinueSequencesAsync(scheduleId, continuationRequest, userId);

                return Ok(updatedSchedule);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { status = "error", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error continuing sequences for schedule {ScheduleId}", scheduleId);
                return StatusCode(500, new { status = "error", message = "Internal server error" });
            }
        }

        // === NEW SCHEDULE STATISTICS ENDPOINTS ===

        /// <summary>
        /// Get schedule statistics and summary
        /// </summary>
        /// <param name="scheduleId">Schedule ID</param>
        /// <returns>Schedule statistics</returns>
        [HttpGet("{scheduleId}/statistics")]
        public async Task<ActionResult<ScheduleStatistics>> GetScheduleStatistics(int scheduleId)
        {
            try
            {
                int userId = GetCurrentUserId();

                var schedule = await _scheduleService.GetByIdAsync(scheduleId, userId);
                if (schedule == null)
                {
                    return NotFound($"Schedule {scheduleId} not found or not accessible");
                }

                var stats = new ScheduleStatistics
                {
                    ScheduleId = scheduleId,
                    TotalEvents = schedule.ScheduleEvents.Count,
                    LessonEvents = schedule.ScheduleEvents.Count(e => e.EventType == "Lesson"),
                    SpecialDayEvents = schedule.ScheduleEvents.Count(e => e.EventCategory == "SpecialDay"),
                    ErrorEvents = schedule.ScheduleEvents.Count(e => e.EventType == "Error"),
                    EventsByPeriod = schedule.ScheduleEvents
                        .GroupBy(e => e.Period)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    DateRange = new
                    {
                        StartDate = schedule.ScheduleEvents.Any() ? schedule.ScheduleEvents.Min(e => e.Date) : (DateTime?)null,
                        EndDate = schedule.ScheduleEvents.Any() ? schedule.ScheduleEvents.Max(e => e.Date) : (DateTime?)null,
                        TotalDays = schedule.ScheduleEvents.Any() ?
                            (schedule.ScheduleEvents.Max(e => e.Date) - schedule.ScheduleEvents.Min(e => e.Date)).Days + 1 : 0
                    }
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics for schedule {ScheduleId}", scheduleId);
                return StatusCode(500, new { status = "error", message = "Internal server error" });
            }
        }
    }

    // === SUPPORTING CLASSES ===

    public class ScheduleStatistics
    {
        public int ScheduleId { get; set; }
        public int TotalEvents { get; set; }
        public int LessonEvents { get; set; }
        public int SpecialDayEvents { get; set; }
        public int ErrorEvents { get; set; }
        public Dictionary<int, int> EventsByPeriod { get; set; } = new();
        public object? DateRange { get; set; }
    }
}
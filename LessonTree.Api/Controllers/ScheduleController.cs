using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
using LessonTree.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System.Threading.Tasks;

namespace LessonTree.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : BaseController
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

        [HttpGet("{scheduleId}")]
        public async Task<IActionResult> GetSchedule(int scheduleId)
        {
            _logger.LogInformation("GET schedule/{scheduleId} called", scheduleId);
            var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
            if (schedule == null)
            {
                _logger.LogWarning("Schedule ID {ScheduleId} not found", scheduleId);
                return NotFound();
            }

            var resource = _mapper.Map<ScheduleResource>(schedule);
            return Ok(resource);
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetSchedulesByCourse(int courseId)
        {
            _logger.LogInformation("GET schedule/course/{courseId} called", courseId);
            var schedules = await _scheduleRepository.GetByCourseIdAsync(courseId);
            var resources = _mapper.Map<List<ScheduleResource>>(schedules);
            return Ok(resources);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSchedule([FromBody] ScheduleCreateResource createResource)
        {
            _logger.LogInformation("POST schedule called for course ID {CourseId} with title {Title}", createResource.CourseId, createResource.Title);

            var schedule = _mapper.Map<Schedule>(createResource);
            // Set UserId from current user context (implement as needed)
            // schedule.UserId = GetCurrentUserId();

            var created = await _scheduleRepository.CreateAsync(schedule);
            var resource = _mapper.Map<ScheduleResource>(created);
            return CreatedAtAction(nameof(GetSchedule), new { scheduleId = created.Id }, resource);
        }

        [HttpPut("{scheduleId}/config")]
        public async Task<IActionResult> UpdateScheduleConfig(int scheduleId, [FromBody] ScheduleConfigUpdateResource config)
        {
            _logger.LogInformation("PUT schedule/{scheduleId}/config called", scheduleId);

            if (scheduleId != config.Id)
            {
                _logger.LogWarning("Schedule ID mismatch: URL {UrlId} vs Body {BodyId}", scheduleId, config.Id);
                return BadRequest("Schedule ID mismatch");
            }

            var updated = await _scheduleRepository.UpdateConfigAsync(scheduleId, config);
            if (updated == null)
            {
                _logger.LogWarning("Schedule ID {ScheduleId} not found for config update", scheduleId);
                return NotFound();
            }

            _logger.LogInformation("Schedule ID {ScheduleId} config updated successfully", scheduleId);
            var resource = _mapper.Map<ScheduleResource>(updated);
            return Ok(resource);
        }

        [HttpPut("{scheduleId}/events")]
        public async Task<IActionResult> UpdateScheduleEvents(int scheduleId, [FromBody] ScheduleEventsUpdateResource scheduleEvents)
        {
            _logger.LogInformation("PUT schedule/{scheduleId}/events called with {EventCount} events", scheduleId, scheduleEvents.ScheduleEvents.Count);

            if (scheduleId != scheduleEvents.ScheduleId)
            {
                _logger.LogWarning("Schedule ID mismatch: URL {UrlId} vs Body {BodyId}", scheduleId, scheduleEvents.ScheduleId);
                return BadRequest("Schedule ID mismatch");
            }

            var updated = await _scheduleRepository.UpdateScheduleEventsAsync(scheduleId, scheduleEvents.ScheduleEvents);
            if (updated == null)
            {
                _logger.LogWarning("Schedule ID {ScheduleId} not found for events update", scheduleId);
                return NotFound();
            }

            _logger.LogInformation("Schedule ID {ScheduleId} events updated successfully", scheduleId);
            var resource = _mapper.Map<ScheduleResource>(updated);
            return Ok(resource);
        }
    }
}
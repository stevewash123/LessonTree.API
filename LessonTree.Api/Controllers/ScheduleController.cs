using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace LessonTree.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleRepository _scheduleRepository;
        private readonly ILogger<ScheduleController> _logger;

        public ScheduleController(IScheduleRepository scheduleRepository, ILogger<ScheduleController> logger)
        {
            _scheduleRepository = scheduleRepository;
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
            return Ok(schedule);
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetSchedulesByCourse(int courseId)
        {
            _logger.LogInformation("GET schedule/course/{courseId} called", courseId);
            var schedules = await _scheduleRepository.GetByCourseIdAsync(courseId);
            return Ok(schedules);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSchedule([FromBody] Schedule schedule)
        {
            _logger.LogInformation("POST schedule called for course ID {CourseId} with title {Title}", schedule.CourseId, schedule.Title);
            var created = await _scheduleRepository.CreateAsync(schedule);
            return CreatedAtAction(nameof(GetSchedule), new { scheduleId = created.Id }, created);
        }

        [HttpPost("day")]
        public async Task<IActionResult> AddScheduleDay([FromBody] ScheduleDay scheduleDay)
        {
            _logger.LogInformation("POST schedule/day called for schedule ID {ScheduleId}", scheduleDay.ScheduleId);
            var created = await _scheduleRepository.AddScheduleDayAsync(scheduleDay);
            return Ok(created);
        }

        [HttpPut("day")]
        public async Task<IActionResult> UpdateScheduleDay([FromBody] ScheduleDay scheduleDay)
        {
            _logger.LogInformation("PUT schedule/day called for schedule day ID {ScheduleDayId}", scheduleDay.Id);
            var updated = await _scheduleRepository.UpdateScheduleDayAsync(scheduleDay);
            if (updated == null)
            {
                _logger.LogWarning("Schedule day ID {ScheduleDayId} not found", scheduleDay.Id);
                return NotFound();
            }
            return Ok(updated);
        }
    }
}
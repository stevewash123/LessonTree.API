using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LessonTree.BLL.Services;
using LessonTree.API.Controllers;
using System;
using System.Threading.Tasks;

namespace LessonTree.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController : BaseController
    {
        private readonly IReportGenerationService _reportService;

        public ReportsController(IReportGenerationService reportService)
        {
            _reportService = reportService;
        }

        [HttpPost("weekly-lesson-plan")]
        public async Task<IActionResult> GenerateWeeklyLessonPlan([FromBody] WeeklyReportRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _reportService.GenerateWeeklyLessonPlanAsync(userId, request.WeekStart);

                if (!result.Success)
                {
                    return BadRequest(new { errors = result.Errors, warnings = result.Warnings });
                }

                var fileName = $"lesson-plan-{request.WeekStart:yyyy-MM-dd}.pdf";
                return File(result.PdfContent, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to generate report", details = ex.Message });
            }
        }

        [HttpGet("weekly-lesson-plan/{weekStart:datetime}")]
        public async Task<IActionResult> GenerateWeeklyLessonPlanGet(DateTime weekStart)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _reportService.GenerateWeeklyLessonPlanAsync(userId, weekStart);

                if (!result.Success)
                {
                    return BadRequest(new { errors = result.Errors, warnings = result.Warnings });
                }

                var fileName = $"lesson-plan-{weekStart:yyyy-MM-dd}.pdf";
                return File(result.PdfContent, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to generate report", details = ex.Message });
            }
        }
    }

    public class WeeklyReportRequest
    {
        public DateTime WeekStart { get; set; }
    }
}
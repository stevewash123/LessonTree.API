using System;
using System.Threading.Tasks;
using LessonTree.Models.Reports;

namespace LessonTree.BLL.Services
{
    public interface IReportGenerationService
    {
        Task<ReportGenerationResult> GenerateWeeklyLessonPlanAsync(int userId, DateTime weekStart);
        Task<WeeklyLessonPlanReport> GetWeeklyReportDataAsync(int userId, DateTime weekStart);
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Layout.Borders;
using iText.Kernel.Geom;
using LessonTree.BLL.Services;
using LessonTree.DAL;
using LessonTree.Models.Reports;

namespace LessonTree.BLL.Services
{
    public class ReportGenerationService : IReportGenerationService
    {
        private readonly LessonTreeContext _context;
        private readonly ILogger<ReportGenerationService> _logger;

        public ReportGenerationService(
            LessonTreeContext context,
            ILogger<ReportGenerationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ReportGenerationResult> GenerateWeeklyLessonPlanAsync(int userId, DateTime weekStart)
        {
            try
            {
                _logger.LogInformation($"GenerateWeeklyLessonPlanAsync: Starting report generation for user {userId}, week {weekStart:yyyy-MM-dd}");

                // Get report data
                var reportData = await GetWeeklyReportDataAsync(userId, weekStart);

                if (reportData.Days.Count == 0)
                {
                    return new ReportGenerationResult
                    {
                        Success = false,
                        Errors = { "No schedule data found for the specified week" }
                    };
                }

                // Generate PDF
                var pdfContent = GeneratePdfReport(reportData);

                _logger.LogInformation($"GenerateWeeklyLessonPlanAsync: Successfully generated PDF report ({pdfContent.Length} bytes)");

                return new ReportGenerationResult
                {
                    Success = true,
                    PdfContent = pdfContent,
                    Metadata = reportData.Metadata
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GenerateWeeklyLessonPlanAsync: Failed to generate report for user {userId}");
                return new ReportGenerationResult
                {
                    Success = false,
                    Errors = { $"Report generation failed: {ex.Message}" }
                };
            }
        }

        public async Task<WeeklyLessonPlanReport> GetWeeklyReportDataAsync(int userId, DateTime weekStart)
        {
            var weekEnd = weekStart.AddDays(7);
            _logger.LogInformation($"GetWeeklyReportDataAsync: Fetching data for user {userId}, week {weekStart:yyyy-MM-dd} to {weekEnd:yyyy-MM-dd}");

            // Get user information
            var user = await _context.Users
                .Include(u => u.School)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new ArgumentException($"User {userId} not found");
            }

            // Get schedule events for the week
            var scheduleEvents = await _context.ScheduleEvents
                .Where(se => se.Schedule.UserId == userId 
                    && se.Date >= weekStart 
                    && se.Date < weekEnd
                    && se.EventType == "Lesson")
                .Include(se => se.Schedule)
                .ThenInclude(s => s.ScheduleConfiguration)
                .ThenInclude(sc => sc.PeriodAssignments)
                .Include(se => se.Lesson)
                .ThenInclude(l => l.Topic)
                .ThenInclude(t => t.Course)
                .OrderBy(se => se.Date)
                .ThenBy(se => se.Period)
                .ToListAsync();

            _logger.LogInformation($"GetWeeklyReportDataAsync: Found {scheduleEvents.Count} schedule events");

            // Group by date
            var dailyReports = new List<DailyScheduleReport>();
            var currentDate = weekStart;

            while (currentDate < weekEnd)
            {
                var dayEvents = scheduleEvents.Where(se => se.Date.Date == currentDate.Date).ToList();
                
                var dailyReport = new DailyScheduleReport
                {
                    Date = currentDate,
                    DayName = currentDate.ToString("dddd"),
                    Periods = new List<PeriodLessonReport>()
                };

                foreach (var evt in dayEvents)
                {
                    var periodAssignment = evt.Schedule.ScheduleConfiguration?.PeriodAssignments
                        .FirstOrDefault(pa => pa.Period == evt.Period);

                    var periodReport = new PeriodLessonReport
                    {
                        Period = evt.Period,
                        StartTime = GetPeriodStartTime(evt.Period),
                        EndTime = GetPeriodEndTime(evt.Period),
                        CourseName = evt.Lesson?.Topic?.Course?.Title ?? "Unknown Course",
                        LessonTitle = evt.Lesson?.Title ?? "No Lesson",
                        Objective = evt.Lesson?.Objective ?? "",
                        TeachingMethod = evt.Lesson?.Methods ?? "",
                        Materials = evt.Lesson?.Materials ?? "",
                        Assessment = evt.Lesson?.Assessment ?? "",
                        Room = periodAssignment?.Room ?? "",
                        SpecialNotes = evt.Comment ?? ""
                    };

                    dailyReport.Periods.Add(periodReport);
                }

                dailyReports.Add(dailyReport);
                currentDate = currentDate.AddDays(1);
            }

            var report = new WeeklyLessonPlanReport
            {
                UserId = userId,
                TeacherName = $"{user.FirstName} {user.LastName}",
                WeekStartDate = weekStart,
                WeekEndDate = weekEnd.AddDays(-1),
                SchoolName = user.School?.Name ?? "Unknown School",
                Days = dailyReports,
                Metadata = new ReportMetadata
                {
                    GeneratedAt = DateTime.UtcNow,
                    TotalDays = dailyReports.Count,
                    TotalPeriods = dailyReports.Sum(d => d.Periods.Count),
                    TotalLessons = dailyReports.Sum(d => d.Periods.Count(p => !string.IsNullOrEmpty(p.LessonTitle) && p.LessonTitle != "No Lesson"))
                }
            };

            return report;
        }

        private byte[] GeneratePdfReport(WeeklyLessonPlanReport reportData)
        {
            using var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            
            // Use landscape orientation to fit the weekly grid
            var document = new Document(pdf, PageSize.A4.Rotate());
            document.SetMargins(20, 20, 20, 20);

            // Header
            AddReportHeader(document, reportData);

            // Weekly grid table
            AddWeeklyGridTable(document, reportData);

            // Footer
            AddReportFooter(document, reportData.Metadata);

            document.Close();
            return stream.ToArray();
        }

        private void AddReportHeader(Document document, WeeklyLessonPlanReport reportData)
        {
            // Title
            var title = new Paragraph("WEEKLY LESSON PLAN SUMMARY")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(16)
                .SetBold()
                .SetMarginBottom(10);
            document.Add(title);

            // Header info table
            var headerTable = new Table(new float[] { 1, 1 })
                .SetWidth(UnitValue.CreatePercentValue(100));

            // Left column
            var leftInfo = new Paragraph()
                .Add($"Teacher: {reportData.TeacherName}\n")
                .Add($"School: {reportData.SchoolName}\n");

            // Right column  
            var rightInfo = new Paragraph()
                .Add($"Week: {reportData.WeekStartDate:MMM d} - {reportData.WeekEndDate:MMM d, yyyy}\n")
                .Add($"Generated: {reportData.Metadata.GeneratedAt:MMM d, yyyy 'at' h:mm tt}\n");

            headerTable.AddCell(new Cell().Add(leftInfo).SetBorder(Border.NO_BORDER));
            headerTable.AddCell(new Cell().Add(rightInfo).SetBorder(Border.NO_BORDER));

            document.Add(headerTable);
            document.Add(new Paragraph().SetMarginBottom(15)); // Spacer
        }

        private void AddWeeklyGridTable(Document document, WeeklyLessonPlanReport reportData)
        {
            // Find maximum periods across all days
            int maxPeriods = reportData.Days.Max(d => d.Periods.Count);
            if (maxPeriods == 0) maxPeriods = 1;

            // Create table with columns: Period + 5 weekdays + Notes
            var table = new Table(new float[] { 1, 2, 2, 2, 2, 2, 1.5f })
                .SetWidth(UnitValue.CreatePercentValue(100));

            // Header row
            table.AddHeaderCell(CreateHeaderCell("Period"));
            
            // Only include weekdays (Monday-Friday) 
            var weekdays = reportData.Days.Where(d => d.Date.DayOfWeek >= DayOfWeek.Monday && d.Date.DayOfWeek <= DayOfWeek.Friday).ToList();
            
            foreach (var day in weekdays)
            {
                table.AddHeaderCell(CreateHeaderCell($"{day.DayName}\n{day.Date:M/d}"));
            }
            
            table.AddHeaderCell(CreateHeaderCell("Notes"));

            // Data rows - one row per period
            for (int period = 1; period <= maxPeriods; period++)
            {
                // Period column
                table.AddCell(CreateDataCell($"Period {period}\n{GetPeriodStartTime(period)}-\n{GetPeriodEndTime(period)}"));

                // Weekday columns
                foreach (var day in weekdays)
                {
                    var periodData = day.Periods.FirstOrDefault(p => p.Period == period);
                    if (periodData != null)
                    {
                        var content = $"Course: {periodData.CourseName}\n" +
                                    $"Lesson: {periodData.LessonTitle}\n" +
                                    $"Objective: {TruncateText(periodData.Objective, 80)}\n" +
                                    $"Method: {TruncateText(periodData.TeachingMethod, 60)}";
                        
                        table.AddCell(CreateDataCell(content));
                    }
                    else
                    {
                        table.AddCell(CreateDataCell("No class"));
                    }
                }

                // Notes column
                var notes = weekdays
                    .SelectMany(d => d.Periods.Where(p => p.Period == period))
                    .Where(p => !string.IsNullOrEmpty(p.Room) || !string.IsNullOrEmpty(p.SpecialNotes))
                    .Select(p => $"Room: {p.Room}\n{p.SpecialNotes}".Trim())
                    .FirstOrDefault() ?? "";
                
                table.AddCell(CreateDataCell(notes));
            }

            document.Add(table);
        }

        private Cell CreateHeaderCell(string text)
        {
            return new Cell()
                .Add(new Paragraph(text)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(10)
                    .SetBold())
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                .SetBorder(new SolidBorder(1));
        }

        private Cell CreateDataCell(string text)
        {
            return new Cell()
                .Add(new Paragraph(text).SetFontSize(8))
                .SetBorder(new SolidBorder(1))
                .SetVerticalAlignment(VerticalAlignment.TOP);
        }

        private void AddReportFooter(Document document, ReportMetadata metadata)
        {
            document.Add(new Paragraph().SetMarginTop(15)); // Spacer

            var footerInfo = new Paragraph()
                .Add($"Report Summary: {metadata.TotalLessons} lessons across {metadata.TotalDays} days\n")
                .Add($"Generated by LessonTree v{metadata.Version} on {metadata.GeneratedAt:yyyy-MM-dd 'at' HH:mm} UTC")
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(ColorConstants.GRAY);

            document.Add(footerInfo);
        }

        private string GetPeriodStartTime(int period)
        {
            // Default period times - could be made configurable
            return period switch
            {
                1 => "8:00",
                2 => "8:50",
                3 => "9:40",
                4 => "10:45",
                5 => "11:35",
                6 => "12:25",
                7 => "1:15",
                8 => "2:05",
                _ => $"{7 + period}:00"
            };
        }

        private string GetPeriodEndTime(int period)
        {
            // Default period times - could be made configurable
            return period switch
            {
                1 => "8:45",
                2 => "9:35",
                3 => "10:25",
                4 => "11:30",
                5 => "12:20",
                6 => "1:10",
                7 => "2:00",
                8 => "2:50",
                _ => $"{7 + period}:45"
            };
        }

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text ?? "";
            
            return text.Substring(0, maxLength - 3) + "...";
        }
    }
}
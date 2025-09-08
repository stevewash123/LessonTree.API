using System;
using System.Collections.Generic;

namespace LessonTree.Models.Reports
{
    public class WeeklyLessonPlanReport
    {
        public int UserId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public List<DailyScheduleReport> Days { get; set; } = new List<DailyScheduleReport>();
        public ReportMetadata Metadata { get; set; } = new ReportMetadata();
    }

    public class DailyScheduleReport
    {
        public DateTime Date { get; set; }
        public string DayName { get; set; } = string.Empty;
        public List<PeriodLessonReport> Periods { get; set; } = new List<PeriodLessonReport>();
        public List<SpecialEventReport> SpecialEvents { get; set; } = new List<SpecialEventReport>();
    }

    public class PeriodLessonReport
    {
        public int Period { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string LessonTitle { get; set; } = string.Empty;
        public string Objective { get; set; } = string.Empty;
        public string TeachingMethod { get; set; } = string.Empty;
        public string Materials { get; set; } = string.Empty;
        public string Assessment { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
        public string SpecialNotes { get; set; } = string.Empty;
    }

    public class SpecialEventReport
    {
        public string EventType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class ReportMetadata
    {
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public string GeneratedBy { get; set; } = "LessonTree System";
        public string Version { get; set; } = "1.0";
        public int TotalDays { get; set; }
        public int TotalPeriods { get; set; }
        public int TotalLessons { get; set; }
    }
}
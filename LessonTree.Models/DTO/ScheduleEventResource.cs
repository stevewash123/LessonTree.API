using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LessonTree.Models.DTO
{
    public class ScheduleEventResource
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public DateTime Date { get; set; }

        [Range(1, 10, ErrorMessage = "Period must be between 1 and 10")]
        public int Period { get; set; }

        public int? LessonId { get; set; }
        public string? SpecialCode { get; set; }
        public string? Comment { get; set; }
    }

    public class ScheduleEventCreateResource
    {
        public int ScheduleId { get; set; }
        public DateTime Date { get; set; }

        [Range(1, 10, ErrorMessage = "Period must be between 1 and 10")]
        public int Period { get; set; }

        public int? LessonId { get; set; }
        public string? SpecialCode { get; set; }
        public string? Comment { get; set; }
    }

    public class ScheduleEventUpdateResource
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }

        [Range(1, 10, ErrorMessage = "Period must be between 1 and 10")]
        public int Period { get; set; }

        public int? LessonId { get; set; }
        public string? SpecialCode { get; set; }
        public string? Comment { get; set; }
    }

    public class ScheduleEventsUpdateResource
    {
        public int ScheduleId { get; set; }
        public List<ScheduleEventResource> ScheduleEvents { get; set; } = new List<ScheduleEventResource>();
    }

    public class ScheduleConfigUpdateResource
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string TeachingDays { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
    }

    // Schedule creation resource
    public class ScheduleCreateResource
    {
        public string Title { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string>? TeachingDays { get; set; }
    }

    // CLEAN: Schedule Resource with ScheduleEvents only
    public class ScheduleResource
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsLocked { get; set; }
        public List<string>? TeachingDays { get; set; }
        public List<ScheduleEventResource>? ScheduleEvents { get; set; }
    }

    // Consolidated Period Assignment DTOs - matches UserResource.cs structure
    public class PeriodAssignmentResource
    {
        public int Id { get; set; }  // For database persistence
        public int Period { get; set; }
        public int? CourseId { get; set; }
        public string? SectionName { get; set; }  // From UserResource
        public string? Room { get; set; }
        public string? Notes { get; set; }  // From UserResource
        public string BackgroundColor { get; set; } = "#FFFFFF";  // For UI display
        public string FontColor { get; set; } = "#000000";  // For UI display
    }

}
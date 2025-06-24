// **COMPLETE FILE** - Schedule Event DTOs (no TeachingDays - events are for specific dates)
// RESPONSIBILITY: Schedule event operations and batch updates
// DOES NOT: Handle schedule configuration (see ScheduleResource.cs) or business logic
// CALLED BY: Controllers for schedule event operations

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LessonTree.Models.DTO
{
    public class ScheduleEventResource
    {
        // Existing properties...
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public int? CourseId { get; set; }
        public DateTime Date { get; set; }
        public int Period { get; set; }
        public int? LessonId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string? EventCategory { get; set; }
        public string? Comment { get; set; }

        // **NEW** - Rich lesson display properties
        public string? LessonTitle { get; set; }
        public string? LessonObjective { get; set; }
        public string? LessonMethods { get; set; }
        public string? LessonMaterials { get; set; }
        public string? LessonAssessment { get; set; }
        public int? LessonSort { get; set; }
        public int ScheduleSort { get; set; }
    }

    public class ScheduleEventCreateResource
    {
        public int ScheduleId { get; set; }
        public int? CourseId { get; set; }
        public DateTime Date { get; set; }
        public int Period { get; set; }
        public int? LessonId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string? EventCategory { get; set; }
        public string? Comment { get; set; }
    }

    public class ScheduleEventUpdateResource
    {
        public int Id { get; set; }
        public int? CourseId { get; set; }
        public DateTime Date { get; set; }

        [Range(1, 10, ErrorMessage = "Period must be between 1 and 10")]
        public int Period { get; set; }

        public int? LessonId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string? EventCategory { get; set; }
        public string? Comment { get; set; }
    }

    public class ScheduleEventsUpdateResource
    {
        public int ScheduleId { get; set; }
        public List<ScheduleEventResource> ScheduleEvents { get; set; } = new List<ScheduleEventResource>();
    }
}
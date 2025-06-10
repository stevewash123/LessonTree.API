// **COMPLETE FILE** - Schedule-related DTOs with standardized string[] TeachingDays
// RESPONSIBILITY: Schedule configuration and creation resources
// DOES NOT: Handle events (see ScheduleEventResource.cs) or business logic
// CALLED BY: Controllers for schedule operations

using System;
using System.Collections.Generic;

namespace LessonTree.Models.DTO
{
    public class ScheduleResource
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsLocked { get; set; }
        public string[] TeachingDays { get; set; } = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
        public List<ScheduleEventResource>? ScheduleEvents { get; set; }
    }

    public class ScheduleCreateResource
    {
        public string Title { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string[] TeachingDays { get; set; } = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };

        // Support for batch event creation
        public List<ScheduleEventResource>? ScheduleEvents { get; set; }
    }

    public class ScheduleConfigUpdateResource
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string[] TeachingDays { get; set; } = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
        public bool IsLocked { get; set; }
    }
}
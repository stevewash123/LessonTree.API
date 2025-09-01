using LessonTree.Models.DTO;

namespace LessonTree.BLL.Services
{
    // === SCHEDULE GENERATION RESULT (NEW - not in existing DTOs) ===

    /// <summary>
    /// Result of schedule generation operation
    /// </summary>
    public class ScheduleGenerationResult
    {
        public bool Success { get; set; }
        public ScheduleResource? Schedule { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();

        // Generation statistics
        public int TotalEventsGenerated { get; set; }
        public Dictionary<int, int> EventsByPeriod { get; set; } = new();
        public Dictionary<string, int> EventsByType { get; set; } = new();

        // Processing details
        public DateTime GenerationStarted { get; set; } = DateTime.UtcNow;
        public DateTime GenerationCompleted { get; set; } = DateTime.UtcNow;
        public TimeSpan ProcessingTime => GenerationCompleted - GenerationStarted;
    }

    // === SCHEDULE UPDATE RESULT (NEW - for smart updates) ===

    /// <summary>
    /// Result of smart schedule update operations (lesson added/moved)
    /// </summary>
    public class ScheduleUpdateResult
    {
        public bool Success { get; set; }
        public int EventsUpdated { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    // === SEQUENCE ANALYSIS RESULT (NEW - for continuation logic) ===

    /// <summary>
    /// Result of analyzing lesson sequence continuation state
    /// </summary>
    public class SequenceAnalysisResult
    {
        public int TotalCoursesInScope { get; set; }
        public int TotalLessonsInScope { get; set; }
        public List<ContinuationPoint> ContinuationPoints { get; set; } = new();
        public List<CoursePeriodDetail> CoursePeriodDetails { get; set; } = new();
    }

    // === CONTINUATION POINT (NEW - for sequence continuation) ===

    /// <summary>
    /// Represents a point where lesson sequence needs to continue
    /// </summary>
    public class ContinuationPoint
    {
        public int Period { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public int LastAssignedLessonIndex { get; set; }
        public DateTime ContinuationDate { get; set; }
        public int TotalLessons { get; set; }
        public int RemainingLessons { get; set; }
        public PeriodAssignmentResource PeriodAssignment { get; set; } = new();
    }

    // === COURSE PERIOD DETAIL (NEW - for analysis) ===

    /// <summary>
    /// Detailed information about course-period assignment for analysis
    /// </summary>
    public class CoursePeriodDetail
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public int Period { get; set; }
        public int TotalLessons { get; set; }
        public int AssignedLessons { get; set; }
        public bool NeedsContinuation { get; set; }
        public DateTime? LastAssignedDate { get; set; }
    }

    // === SEQUENCE CONTINUATION REQUEST (NEW - for API requests) ===

    /// <summary>
    /// Request parameters for continuing lesson sequences
    /// </summary>
    public class SequenceContinuationRequest
    {
        public DateTime AfterDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<int>? SpecificCourseIds { get; set; } // null = all courses
        public List<int>? SpecificPeriods { get; set; } // null = all periods
        public bool SkipCompletedCourses { get; set; } = true;
        public int? MaxEventsToGenerate { get; set; } // null = no limit
    }

    // === ENHANCED VALIDATION RESULT (extends existing ScheduleConfigurationValidationResource) ===

    /// <summary>
    /// Enhanced validation result with generation-specific details
    /// Extends the existing ScheduleConfigurationValidationResource
    /// </summary>
    public class ScheduleValidationResult : ScheduleConfigurationValidationResource
    {
        // Additional generation-specific properties
        public int TotalPeriodsConfigured { get; set; }
        public int CourseAssignments { get; set; }
        public int SpecialPeriodAssignments { get; set; }
        public int UnassignedPeriods { get; set; }
    }

    // === GENERATION PREVIEW (NEW - for preview functionality) ===

    /// <summary>
    /// Preview information for schedule generation
    /// </summary>
    public class ScheduleGenerationPreview
    {
        public bool CanGenerate { get; set; }
        public ScheduleValidationResult ValidationResult { get; set; } = new();
        public int EstimatedEventCount { get; set; }
        public Dictionary<int, int> EstimatedEventsByPeriod { get; set; } = new();
        public DateRange? DateRange { get; set; }
    }

    /// <summary>
    /// Date range information for generation preview
    /// </summary>
    public class DateRange
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TeachingDaysCount { get; set; }
    }
}
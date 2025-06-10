// UPDATED: UserConfiguration.cs - Add schedule properties and teaching days
using LessonTree.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace LessonTree.DAL.Domain
{
    public class UserConfiguration
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public string? SettingsJson { get; set; }
        public DateTime LastUpdated { get; set; }
 
        [MaxLength(20)]
        public string SchoolYear { get; set; } = string.Empty; // e.g., "2024-2025"

        [Range(1, 10, ErrorMessage = "PeriodsPerDay must be between 1 and 10")]
        public int PeriodsPerDay { get; set; } = 6; // Default to 6 periods

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public List<PeriodAssignment> PeriodAssignments { get; set; } = new List<PeriodAssignment>();
    }

    public class PeriodAssignment
    {
        public int Id { get; set; }
        public int UserConfigurationId { get; set; }
        public virtual UserConfiguration UserConfiguration { get; set; }

        [Range(1, 10, ErrorMessage = "Period must be between 1 and 10")]
        public int Period { get; set; }

        public int? CourseId { get; set; }  // Course teaching periods
        public SpecialPeriodType? SpecialPeriodType { get; set; }  // Duty periods

        // NEW: Teaching days for this specific assignment
        [MaxLength(100)]
        public string TeachingDays { get; set; } = string.Empty; // e.g., "Monday,Wednesday,Friday"

        [MaxLength(50)]
        public string? Room { get; set; }
        [MaxLength(500)]
        public string? Notes { get; set; }
        [MaxLength(7)]
        public string BackgroundColor { get; set; } = "#FFFFFF";
        [MaxLength(7)]
        public string FontColor { get; set; } = "#000000";
    }
}
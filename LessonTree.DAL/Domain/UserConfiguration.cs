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

        // NEW: Period-based scheduling support  
        [MaxLength(20)]
        public string SchoolYear { get; set; } = string.Empty; // e.g., "2024-2025"

        [Range(1, 10, ErrorMessage = "PeriodsPerDay must be between 1 and 10")]
        public int PeriodsPerDay { get; set; } = 6; // Default to 6 periods

        public List<PeriodAssignment> PeriodAssignments { get; set; } = new List<PeriodAssignment>();
    }

    public class PeriodAssignment
    {
        public int Id { get; set; }
        public int UserConfigurationId { get; set; }
        public virtual UserConfiguration UserConfiguration { get; set; }

        [Range(1, 10, ErrorMessage = "Period must be between 1 and 10")]
        public int Period { get; set; }

        public int? CourseId { get; set; }  // negative values allowed, they link to hard coded fixed values

        [MaxLength(100)]
        public string? SectionName { get; set; } // Course section identifier

        [MaxLength(50)]
        public string? Room { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; } // Additional period notes

        [MaxLength(7)] // Hex color format #RRGGBB
        public string BackgroundColor { get; set; } = "#FFFFFF";

        [MaxLength(7)] // Hex color format #RRGGBB  
        public string FontColor { get; set; } = "#000000";
    }
}
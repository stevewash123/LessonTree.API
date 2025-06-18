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

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Basic user preferences
        public string? SettingsJson { get; set; } // For future user preferences

    }
}
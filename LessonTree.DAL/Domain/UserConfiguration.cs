using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LessonTree.DAL.Domain
{
    public class UserConfiguration
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public string? SettingsJson { get; set; } // Flexible storage for configuration data (e.g., JSON string)
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}

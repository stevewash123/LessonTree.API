using System.ComponentModel.DataAnnotations;

namespace LessonTree.DAL.Domain
{
    public class SystemConfig
    {
        [Key]
        public string Key { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;

        public string? Description { get; set; }
    }
}
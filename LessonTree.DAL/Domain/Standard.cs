using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LessonTree.DAL.Domain
{
    public class Standard
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int TopicId { get; set; }
        public virtual Topic Topic { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }  

        [StringLength(20)]
        public string? StandardType { get; set; }  


        // Collection for many-to-many relationship with Lessons (optional, depending on use case)
        public virtual List<LessonStandard> LessonStandards { get; set; } = new List<LessonStandard>();
    }
}
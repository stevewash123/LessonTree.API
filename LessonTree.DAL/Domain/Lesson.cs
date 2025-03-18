using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LessonTree.DAL.Domain
{
    public class Lesson
    {
        public int Id { get; set; }

        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string Objective { get; set; }


        [MaxLength(100)]
        public string? Level { get; set; }

        [MaxLength(200)]
        public string? Materials { get; set; }

        [MaxLength(100)]
        public string? ClassTime { get; set; }

        [MaxLength(500)]
        public string? Methods { get; set; }

        [MaxLength(500)]
        public string? SpecialNeeds { get; set; }

        [MaxLength(250)]
        public string? Assessment { get; set; }

        // Collection for many-to-many relationship with Standards
        public virtual List<LessonStandard> LessonStandards { get; set; } = new List<LessonStandard>();
        public int SubTopicId { get; set; }
        public virtual SubTopic SubTopic { get; set; }
        public List<LessonAttachment> LessonAttachments { get; set; } = new List<LessonAttachment>();
    }
}
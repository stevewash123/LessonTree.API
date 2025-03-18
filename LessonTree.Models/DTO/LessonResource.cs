using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LessonTree.Models.DTO
{
    public class LessonResource
    {
        public int Id { get; set; }
        public int SubTopicId { get; set; }
        public int CourseId { get; set; }   
        public string NodeId { get; set; }
        public string Title { get; set; }
        public string Objective { get; set; }
    }

    public class LessonCreateResource
    {
        public string Title { get; set; }
        public int SubTopicId { get; set; }
        public string? Level { get; set; }
        public string Objective { get; set; }
        public string? Materials { get; set; }
        public string? ClassTime { get; set; }
        public string? Methods { get; set; }
        public string? SpecialNeeds { get; set; }
        public string? Assessment { get; set; }
    }

    public class LessonUpdateResource
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Level { get; set; }
        public string Objective { get; set; }
        public string? Materials { get; set; }
        public string? ClassTime { get; set; }
        public string? Methods { get; set; }
        public string? SpecialNeeds { get; set; }
        public string? Assessment { get; set; }
    }

    // DTO for the payload
    public class LessonMoveResource
    {
        public int LessonId { get; set; }
        public int NewSubTopicId { get; set; }
    }

    public class LessonDetailResource
    {
        public int Id { get; set; }
        public int SubTopicId { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string Objective { get; set; }
        public string? Level { get; set; }
        public string? Materials { get; set; }
        public string? ClassTime { get; set; }
        public string? Methods { get; set; }
        public string? SpecialNeeds { get; set; }
        public string? Assessment { get; set; }
        public List<StandardResource> Standards { get; set; } = new List<StandardResource>();
        public List<AttachmentResource> Attachments { get; set; }
    }
}

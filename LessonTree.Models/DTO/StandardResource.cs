using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LessonTree.Models.DTO
{
    public class StandardResource
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int TopicId { get; set; }
        public string? Description { get; set; }  
        public string? StandardType { get; set; }  
    }

    public class StandardCreateResource
    {
        public string Title { get; set; }
        public int TopicId { get; set; }
        public string? Description { get; set; }  // New property
        public string? StandardType { get; set; }  // New property
    }

    public class StandardUpdateResource
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int TopicId { get; set; }
        public string? Description { get; set; }  // New property
        public string? StandardType { get; set; }  // New property
    }
}
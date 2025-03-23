using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LessonTree.Models.DTO
{
    public class SubTopicResource // New
    {
        public int Id { get; set; }
        public string NodeId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int TopicId { get; set; }
        public int CourseId { get; set; }
        public Boolean IsDefault { get; set; }
        public Boolean hasChildren { get; set; }
    }
    public class SubTopicCreateResource // New
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int TopicId { get; set; }
    }

    public class SubTopicUpdateResource // New
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public class SubTopicMoveResource
    {
        public int SubTopicId { get; set; }
        public int NewTopicId { get; set; }
    }
}

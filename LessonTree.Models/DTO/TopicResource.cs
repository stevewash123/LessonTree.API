using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LessonTree.Models.DTO
{
    public class TopicResource
    {
        public int Id { get; set; }
        public string NodeId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Boolean HasSubTopics { get; set; }
        public int CourseId { get; set; }
        public Boolean hasChildren { get; set; }
        public List<SubTopicResource> SubTopics { get; set; }
        public List<LessonResource> Lessons { get; set; }

    }

    public class TopicCreateResource
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int CourseId { get; set; }
    }

    public class TopicUpdateResource
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool HasSubTopics { get; set; }
    }

    public class TopicMoveResource
    {
        public int TopicId { get; set; }
        public int NewCourseId { get; set; }
    }
}

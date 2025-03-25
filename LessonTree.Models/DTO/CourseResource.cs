using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LessonTree.Models.DTO
{
    public class CourseResource
    {
        public int Id { get; set; }
        public string NodeId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Boolean hasChildren { get; set; }
        public List<TopicResource> Topics { get; set; }
}
    public class CourseCreateResource
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public class CourseUpdateResource
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}

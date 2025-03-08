using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LessonTree.DAL.Domain
{
    public class SubTopic
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public int TopicId { get; set; }
        public virtual Topic Topic { get; set; }
        public virtual List<Lesson> Lessons { get; set; } = new List<Lesson>();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LessonTree.DAL.Domain
{
    public class LessonDocument
    {
        public int LessonId { get; set; }
        public Lesson Lesson { get; set; }
        public int DocumentId { get; set; }
        public Document Document { get; set; }
    }
}

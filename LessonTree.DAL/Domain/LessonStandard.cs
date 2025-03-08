using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LessonTree.DAL.Domain
{
    public class LessonStandard
    {
        public int LessonId { get; set; }
        public virtual Lesson Lesson { get; set; }
        public int StandardId { get; set; }
        public virtual Standard Standard { get; set; }
    }
}
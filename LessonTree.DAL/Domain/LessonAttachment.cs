using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LessonTree.DAL.Domain
{
    public class LessonAttachment
    {
        public int LessonId { get; set; }
        public Lesson Lesson { get; set; }
        public int AttachmentId { get; set; }
        public Attachment Attachment { get; set; }
    }
}

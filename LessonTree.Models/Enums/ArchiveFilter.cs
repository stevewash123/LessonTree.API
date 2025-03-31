using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LessonTree.Models.Enums
{
    public enum ArchiveFilter
    {
        Active,  // Archived = false
        Archived, // Archived = true
        Both     // No filter on Archived
    }
}

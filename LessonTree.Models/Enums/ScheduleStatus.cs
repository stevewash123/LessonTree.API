using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LessonTree.Models.Enums
{
    public enum ScheduleStatus
    {
        Active,      // Current active schedule, fully editable
        Archived,    // Manually archived by user/admin, read-only
        Historical   // Auto-locked after grace period, read-only
    }
}
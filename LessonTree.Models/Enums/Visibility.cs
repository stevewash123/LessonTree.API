using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LessonTree.Models.Enums 
{
    public enum VisibilityType
    {
        Private,  // Only the creator can see it
        Public,   // Anyone can see it
        Team      // Visible to the user’s team (team feature TBD)
    }
}

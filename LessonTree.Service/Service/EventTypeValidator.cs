using LessonTree.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LessonTree.BLL.Service
{
    public class EventTypeValidator
    {
        public static bool IsValidEventType(string eventType, string? eventCategory)
        {
            return eventCategory switch
            {
                "Lesson" => eventType == "Lesson",
                "SpecialPeriod" => Enum.TryParse<SpecialPeriodType>(eventType, out _),
                "SpecialDay" => Enum.TryParse<SpecialDayType>(eventType, out _),
                null => eventType is "OverflowError" or "UnderflowError",
                _ => false
            };
        }
    }
}

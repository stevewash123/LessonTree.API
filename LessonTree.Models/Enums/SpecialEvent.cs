using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LessonTree.Models.Enums
{
    public enum SpecialDayType
    {
        Assembly,
        Testing,
        Holiday,
        ProfessionalDevelopment,
        FieldTrip,
        WeatherDelay,
        EarlyDismissal
    }

    public enum SpecialPeriodType
    {
        Lunch,
        HallDuty,
        CafeteriaDuty,
        StudyHall,
        Prep,
        OtherDuty
    }

    public enum EventCategory
    {
        Lesson,
        SpecialPeriod,
        SpecialDay
        // Note: Error events have null EventCategory
    }
}

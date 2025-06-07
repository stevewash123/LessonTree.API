// LessonTree.Models/Enums/FixedPeriodType.cs
// RESPONSIBILITY: Define system-level period types that don't correspond to actual courses
// DOES NOT: Handle user-created courses (positive IDs)
// CALLED BY: UserService for validation

namespace LessonTree.Models.Enums
{
    public enum FixedPeriodType
    {
        Lunch = -1,
        HallDuty = -2,
        CafeteriaDuty = -3,
        StudyHall = -4,
        Prep = -5,
        OtherDuty = -6
    }

    public static class FixedPeriodTypeExtensions
    {
        public static string GetDisplayName(this FixedPeriodType type)
        {
            return type switch
            {
                FixedPeriodType.Lunch => "Lunch",
                FixedPeriodType.HallDuty => "Hall Duty",
                FixedPeriodType.CafeteriaDuty => "Cafeteria Duty",
                FixedPeriodType.StudyHall => "Study Hall",
                FixedPeriodType.Prep => "Teacher Prep",
                FixedPeriodType.OtherDuty => "Other Duty",
                _ => type.ToString()
            };
        }

        public static bool IsValidFixedPeriodId(int courseId)
        {
            return courseId < 0 && Enum.IsDefined(typeof(FixedPeriodType), courseId);
        }
    }
}
// **STATIC VALIDATOR** - EventTypeValidator for event type and category validation
// RESPONSIBILITY: Validates event type and category combinations for schedule events
// DOES NOT: Handle database operations (pure validation logic)
// CALLED BY: Services and controllers for event validation

using LessonTree.Models.Enums;

namespace LessonTree.BLL.Validation
{
    public static class EventTypeValidator
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

        public static ValidationResult ValidateEventType(string eventType, string? eventCategory)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(eventType))
            {
                result.AddError("EventType cannot be empty");
                return result;
            }

            if (!IsValidEventType(eventType, eventCategory))
            {
                result.AddError($"Invalid event type '{eventType}' for category '{eventCategory ?? "null"}'");
            }

            return result;
        }

        public static List<string> GetValidEventTypesForCategory(string? eventCategory)
        {
            return eventCategory switch
            {
                "Lesson" => new List<string> { "Lesson" },
                "SpecialPeriod" => Enum.GetNames<SpecialPeriodType>().ToList(),
                "SpecialDay" => Enum.GetNames<SpecialDayType>().ToList(),
                null => new List<string> { "OverflowError", "UnderflowError" },
                _ => new List<string>()
            };
        }
    }
}
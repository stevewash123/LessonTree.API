// **NEW UTILITY** - TeachingDaysValidator.cs
// RESPONSIBILITY: Validate teaching days hierarchy and subset relationships
// DOES NOT: Handle database operations (pure validation logic)
// CALLED BY: Controllers and services for teaching days validation

namespace LessonTree.BLL.Validation
{
    public static class TeachingDaysValidator
    {
        private static readonly HashSet<string> ValidDayNames = new HashSet<string>
        {
            "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"
        };

        /// <summary>
        /// Validates that teaching days string contains only valid day names
        /// </summary>
        public static List<string> ValidateTeachingDaysFormat(string teachingDays, string fieldName = "TeachingDays")
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(teachingDays))
            {
                errors.Add($"{fieldName} cannot be empty");
                return errors;
            }

            var days = ParseTeachingDays(teachingDays);

            if (days.Count == 0)
            {
                errors.Add($"{fieldName} must contain at least one valid day");
                return errors;
            }

            var invalidDays = days.Except(ValidDayNames).ToList();
            if (invalidDays.Any())
            {
                errors.Add($"{fieldName} contains invalid day names: {string.Join(", ", invalidDays)}. Valid days: {string.Join(", ", ValidDayNames)}");
            }

            return errors;
        }

        /// <summary>
        /// Validates that period assignment teaching days are a subset of schedule teaching days
        /// </summary>
        public static List<string> ValidateTeachingDaysSubset(
            string scheduleTeachingDays,
            string assignmentTeachingDays,
            string assignmentDescription = "Assignment")
        {
            var errors = new List<string>();

            var scheduleDays = ParseTeachingDays(scheduleTeachingDays);
            var assignmentDays = ParseTeachingDays(assignmentTeachingDays);

            if (scheduleDays.Count == 0)
            {
                errors.Add("Schedule teaching days cannot be empty");
                return errors;
            }

            if (assignmentDays.Count == 0)
            {
                errors.Add($"{assignmentDescription} teaching days cannot be empty");
                return errors;
            }

            var invalidDays = assignmentDays.Except(scheduleDays).ToList();
            if (invalidDays.Any())
            {
                errors.Add($"{assignmentDescription} teaching days [{string.Join(", ", invalidDays)}] are not enabled in the schedule. Schedule allows: [{string.Join(", ", scheduleDays)}]");
            }

            return errors;
        }

        /// <summary>
        /// Validates that array format teaching days are a subset of schedule teaching days
        /// </summary>
        public static List<string> ValidateTeachingDaysSubset(
            string[] scheduleTeachingDays,
            string[] assignmentTeachingDays,
            string assignmentDescription = "Assignment")
        {
            var scheduleString = string.Join(",", scheduleTeachingDays?.Where(d => !string.IsNullOrEmpty(d)) ?? Array.Empty<string>());
            var assignmentString = string.Join(",", assignmentTeachingDays?.Where(d => !string.IsNullOrEmpty(d)) ?? Array.Empty<string>());

            return ValidateTeachingDaysSubset(scheduleString, assignmentString, assignmentDescription);
        }

        /// <summary>
        /// Parse comma-delimited teaching days string into HashSet
        /// </summary>
        public static HashSet<string> ParseTeachingDays(string teachingDays)
        {
            if (string.IsNullOrWhiteSpace(teachingDays))
                return new HashSet<string>();

            return teachingDays
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(day => day.Trim())
                .Where(day => !string.IsNullOrEmpty(day))
                .ToHashSet();
        }

        /// <summary>
        /// Parse array teaching days into HashSet
        /// </summary>
        public static HashSet<string> ParseTeachingDays(string[] teachingDays)
        {
            if (teachingDays == null)
                return new HashSet<string>();

            return teachingDays
                .Where(day => !string.IsNullOrEmpty(day))
                .ToHashSet();
        }

        /// <summary>
        /// Convert HashSet to comma-delimited string
        /// </summary>
        public static string FormatTeachingDays(HashSet<string> teachingDays)
        {
            if (teachingDays == null || teachingDays.Count == 0)
                return "Monday,Tuesday,Wednesday,Thursday,Friday"; // Default

            return string.Join(",", teachingDays.OrderBy(GetDayOrder));
        }

        /// <summary>
        /// Convert array to comma-delimited string
        /// </summary>
        public static string FormatTeachingDays(string[] teachingDays)
        {
            if (teachingDays == null || teachingDays.Length == 0)
                return "Monday,Tuesday,Wednesday,Thursday,Friday"; // Default

            var validDays = teachingDays.Where(day => !string.IsNullOrEmpty(day));
            return string.Join(",", validDays.OrderBy(GetDayOrder));
        }

        /// <summary>
        /// Get day order for sorting (Monday = 1, Sunday = 7)
        /// </summary>
        private static int GetDayOrder(string day)
        {
            return day switch
            {
                "Monday" => 1,
                "Tuesday" => 2,
                "Wednesday" => 3,
                "Thursday" => 4,
                "Friday" => 5,
                "Saturday" => 6,
                "Sunday" => 7,
                _ => 8 // Invalid days sort last
            };
        }

        /// <summary>
        /// Check if teaching days string is valid format
        /// </summary>
        public static bool IsValidTeachingDaysFormat(string teachingDays)
        {
            return ValidateTeachingDaysFormat(teachingDays).Count == 0;
        }

        /// <summary>
        /// Check if assignment days are valid subset of schedule days
        /// </summary>
        public static bool IsValidTeachingDaysSubset(string scheduleTeachingDays, string assignmentTeachingDays)
        {
            return ValidateTeachingDaysSubset(scheduleTeachingDays, assignmentTeachingDays).Count == 0;
        }

        /// <summary>
        /// Get default teaching days (Monday-Friday)
        /// </summary>
        public static string GetDefaultTeachingDays()
        {
            return "Monday,Tuesday,Wednesday,Thursday,Friday";
        }

        /// <summary>
        /// Get default teaching days as array
        /// </summary>
        public static string[] GetDefaultTeachingDaysArray()
        {
            return new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
        }
    }
}
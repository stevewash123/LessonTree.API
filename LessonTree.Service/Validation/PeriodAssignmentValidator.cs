// **STATIC VALIDATOR** - PeriodAssignmentValidator for period assignment validation
// RESPONSIBILITY: Validates period assignment coverage and conflicts for master schedule generation
// DOES NOT: Handle UI logic or database operations - pure validation logic
// CALLED BY: Services and controllers before saving period assignments

using LessonTree.Models.DTO;
using System.ComponentModel.DataAnnotations;

namespace LessonTree.BLL.Validation
{
    public static class PeriodAssignmentValidator
    {
        private static readonly string[] ValidDayNames = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

        public static ValidationResult ValidatePeriodAssignments(List<PeriodAssignmentResource> assignments, int periodsPerDay)
        {
            var result = new ValidationResult();

            // Validate teaching days format first
            var formatResult = ValidateTeachingDaysFormat(assignments);
            result.AddErrors(formatResult.Errors);

            if (!formatResult.IsValid)
            {
                return result; // Don't continue if format is invalid
            }

            // Validate no conflicts
            var conflictResult = ValidateNoConflicts(assignments);
            result.AddErrors(conflictResult.Errors);

            // Validate complete coverage
            var coverageResult = ValidateCompleteCoverage(assignments, periodsPerDay);
            result.AddErrors(coverageResult.Errors);

            return result;
        }

        public static ValidationResult ValidateCompleteCoverage(List<PeriodAssignmentResource> assignments, int periodsPerDay)
        {
            var result = new ValidationResult();

            // Get all unique teaching days from all assignments
            var allTeachingDays = GetAllUniqueTeachingDays(assignments);

            if (!allTeachingDays.Any())
            {
                result.AddError("No teaching days found in period assignments");
                return result;
            }

            // Check each period has complete coverage for all teaching days
            for (int period = 1; period <= periodsPerDay; period++)
            {
                var periodAssignments = assignments.Where(a => a.Period == period).ToList();

                if (!periodAssignments.Any())
                {
                    result.AddError($"Period {period} has no assignments");
                    continue;
                }

                // Get all days covered by this period's assignments
                var coveredDays = new HashSet<string>();
                foreach (var assignment in periodAssignments)
                {
                    foreach (var day in assignment.TeachingDays)
                    {
                        if (!string.IsNullOrWhiteSpace(day))
                        {
                            coveredDays.Add(day.Trim());
                        }
                    }
                }

                // Check if all teaching days are covered
                var uncoveredDays = allTeachingDays.Except(coveredDays).ToList();
                if (uncoveredDays.Any())
                {
                    result.AddError($"Period {period} missing coverage for: {string.Join(", ", uncoveredDays)}");
                }
            }

            return result;
        }

        public static ValidationResult ValidateNoConflicts(List<PeriodAssignmentResource> assignments)
        {
            var result = new ValidationResult();

            // Group by period
            var periodGroups = assignments.GroupBy(a => a.Period);

            foreach (var periodGroup in periodGroups)
            {
                var period = periodGroup.Key;
                var periodAssignments = periodGroup.ToList();

                if (periodAssignments.Count <= 1)
                {
                    continue; // No conflicts possible with single assignment
                }

                // Check for day overlaps within this period
                var dayAssignments = new Dictionary<string, List<PeriodAssignmentResource>>();

                foreach (var assignment in periodAssignments)
                {
                    foreach (var day in assignment.TeachingDays)
                    {
                        if (string.IsNullOrWhiteSpace(day)) continue;

                        var trimmedDay = day.Trim();
                        if (!dayAssignments.ContainsKey(trimmedDay))
                        {
                            dayAssignments[trimmedDay] = new List<PeriodAssignmentResource>();
                        }
                        dayAssignments[trimmedDay].Add(assignment);
                    }
                }

                // Report conflicts
                foreach (var dayGroup in dayAssignments.Where(kvp => kvp.Value.Count > 1))
                {
                    var day = dayGroup.Key;
                    var conflictingAssignments = dayGroup.Value;

                    var assignmentDescriptions = conflictingAssignments.Select(a =>
                        a.CourseId.HasValue ? $"Course {a.CourseId}" : $"{a.SpecialPeriodType}").ToList();

                    result.AddError($"Period {period} has conflicting assignments on {day}: {string.Join(", ", assignmentDescriptions)}");
                }
            }

            return result;
        }

        public static ValidationResult ValidateTeachingDaysFormat(List<PeriodAssignmentResource> assignments)
        {
            var result = new ValidationResult();

            foreach (var assignment in assignments)
            {
                if (assignment.TeachingDays == null || assignment.TeachingDays.Length == 0)
                {
                    result.AddError($"Period {assignment.Period}: TeachingDays cannot be empty");
                    continue;
                }

                var validTeachingDays = assignment.TeachingDays
                    .Where(day => !string.IsNullOrWhiteSpace(day))
                    .Select(day => day.Trim())
                    .ToList();

                if (!validTeachingDays.Any())
                {
                    result.AddError($"Period {assignment.Period}: No valid teaching days found");
                    continue;
                }

                // Check for invalid day names
                var invalidDays = validTeachingDays.Except(ValidDayNames).ToList();
                if (invalidDays.Any())
                {
                    result.AddError($"Period {assignment.Period}: Invalid day names: {string.Join(", ", invalidDays)}");
                }

                // Check for duplicates
                if (validTeachingDays.Count != validTeachingDays.Distinct().Count())
                {
                    result.AddError($"Period {assignment.Period}: Duplicate days found in TeachingDays");
                }
            }

            return result;
        }

        // === PRIVATE HELPER METHODS ===

        private static HashSet<string> GetAllUniqueTeachingDays(List<PeriodAssignmentResource> assignments)
        {
            var allDays = new HashSet<string>();

            foreach (var assignment in assignments)
            {
                foreach (var day in assignment.TeachingDays)
                {
                    if (!string.IsNullOrWhiteSpace(day))
                    {
                        allDays.Add(day.Trim());
                    }
                }
            }

            return allDays;
        }
    }
}
// **COMPLETE FILE** - Updated PeriodAssignmentValidationService for string[] TeachingDays
// RESPONSIBILITY: Validates period assignment coverage and conflicts for master schedule generation
// DOES NOT: Handle UI logic or database operations - pure validation logic
// CALLED BY: UserConfigurationController before saving period assignments

using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;

namespace LessonTree.API.Services
{
    public interface IPeriodAssignmentValidationService
    {
        ValidationResult ValidatePeriodAssignments(List<PeriodAssignmentResource> assignments, int periodsPerDay);
        ValidationResult ValidateCompleteCoverage(List<PeriodAssignmentResource> assignments, int periodsPerDay);
        ValidationResult ValidateNoConflicts(List<PeriodAssignmentResource> assignments);
        ValidationResult ValidateTeachingDaysFormat(List<PeriodAssignmentResource> assignments);
    }

    public class PeriodAssignmentValidationService : IPeriodAssignmentValidationService
    {
        private readonly string[] ValidDayNames = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

        public ValidationResult ValidatePeriodAssignments(List<PeriodAssignmentResource> assignments, int periodsPerDay)
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

        public ValidationResult ValidateCompleteCoverage(List<PeriodAssignmentResource> assignments, int periodsPerDay)
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
                    // UPDATED: TeachingDays is now string[], so we can iterate directly
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

        public ValidationResult ValidateNoConflicts(List<PeriodAssignmentResource> assignments)
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
                    // UPDATED: TeachingDays is now string[], iterate directly
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

        public ValidationResult ValidateTeachingDaysFormat(List<PeriodAssignmentResource> assignments)
        {
            var result = new ValidationResult();

            foreach (var assignment in assignments)
            {
                // UPDATED: TeachingDays is now string[], check if null or empty
                if (assignment.TeachingDays == null || assignment.TeachingDays.Length == 0)
                {
                    result.AddError($"Period {assignment.Period}: TeachingDays cannot be empty");
                    continue;
                }

                // UPDATED: Filter out null/empty strings and get valid days
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

        // UPDATED: Removed ParseTeachingDays method since TeachingDays is now string[]

        private HashSet<string> GetAllUniqueTeachingDays(List<PeriodAssignmentResource> assignments)
        {
            var allDays = new HashSet<string>();

            foreach (var assignment in assignments)
            {
                // UPDATED: TeachingDays is now string[], iterate directly
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

    // Validation result model (unchanged)
    public class ValidationResult
    {
        public bool IsValid => !Errors.Any();
        public List<string> Errors { get; set; } = new List<string>();

        public void AddError(string error)
        {
            Errors.Add(error);
        }

        public void AddErrors(List<string> errors)
        {
            Errors.AddRange(errors);
        }
    }
}
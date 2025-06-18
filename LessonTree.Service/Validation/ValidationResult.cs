
// **SHARED MODEL** - ValidationResult for all validation operations
// RESPONSIBILITY: Standard validation result pattern across all validators
// DOES NOT: Handle validation logic (pure data model)
// CALLED BY: All validation classes in LessonTree.BLL.Validation

namespace LessonTree.BLL.Validation
{
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

        public void AddErrors(IEnumerable<string> errors)
        {
            Errors.AddRange(errors);
        }
    }
}
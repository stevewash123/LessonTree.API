namespace LessonTree.Models.DTO
{
    public class UserResource
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        // ADDED: Computed in mapping - matches TypeScript expectation
        public string FullName { get; set; } = string.Empty;

        // ADDED: District to match TypeScript User interface
        public int? District { get; set; }

        // NOTE: Roles and Claims removed - extract from JWT headers in UI instead
    }
}
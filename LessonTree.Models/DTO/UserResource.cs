namespace LessonTree.Models.DTO
{
    public class UserResource
    {
        public int Id { get; set; } // Added to match expected payload
        public string Username { get; set; }
        public string Password { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
using Microsoft.AspNetCore.Identity;

namespace LessonTree.DAL.Domain
{
    public class User : IdentityUser<int> // Use int as key type
    {
        // Username and PasswordHash inherited from IdentityUser as UserName and PasswordHash
        public virtual UserConfiguration? Configuration { get; set; }
        public virtual List<Course> Courses { get; set; } = new List<Course>();
        public virtual List<Topic> Topics { get; set; } = new List<Topic>();
        public virtual List<SubTopic> SubTopics { get; set; } = new List<SubTopic>();
        public virtual List<Lesson> Lessons { get; set; } = new List<Lesson>();
        public virtual List<UserTeam> UserTeams { get; set; } = new List<UserTeam>(); // New: Team membership
    }
}
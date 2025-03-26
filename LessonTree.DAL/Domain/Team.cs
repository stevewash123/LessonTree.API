namespace LessonTree.DAL.Domain
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int OwnerId { get; set; } // The user who created/manages the team
        public virtual User Owner { get; set; }
        public virtual List<UserTeam> UserTeams { get; set; } = new List<UserTeam>(); // Many-to-many with User
    }
}
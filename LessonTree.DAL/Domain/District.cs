// Full File: LessonTree.DAL/Domain/District.cs
namespace LessonTree.DAL.Domain
{
    public class District
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public virtual List<School> Schools { get; set; } = new List<School>();
        public virtual List<User> Staff { get; set; } = new List<User>();
    }
}
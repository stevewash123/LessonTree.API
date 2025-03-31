// Full File: LessonTree.DAL/Domain/Department.cs
namespace LessonTree.DAL.Domain
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int SchoolId { get; set; }
        public virtual School School { get; set; }
        public virtual List<User> Members { get; set; } = new List<User>();
    }
}
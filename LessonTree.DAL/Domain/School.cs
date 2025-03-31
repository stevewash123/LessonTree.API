// Full File: LessonTree.DAL/Domain/School.cs
namespace LessonTree.DAL.Domain
{
    public class School
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int DistrictId { get; set; }
        public virtual District District { get; set; }
        public virtual List<Department> Departments { get; set; } = new List<Department>();
        public virtual List<User> Teachers { get; set; } = new List<User>();
    }
}
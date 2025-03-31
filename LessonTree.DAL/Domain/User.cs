using Microsoft.AspNetCore.Identity;

namespace LessonTree.DAL.Domain
{
    public class User : IdentityUser<int> // Use int as key type
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? DistrictId { get; set; }
        public virtual District? District { get; set; }
        public int? SchoolId { get; set; }
        public virtual School? School { get; set; }
        public virtual List<Department> Departments { get; set; } = new List<Department>();
        public virtual UserConfiguration? Configuration { get; set; }
        public virtual List<Course> Courses { get; set; } = new List<Course>();
        public virtual List<Topic> Topics { get; set; } = new List<Topic>();
        public virtual List<SubTopic> SubTopics { get; set; } = new List<SubTopic>();
        public virtual List<Lesson> Lessons { get; set; } = new List<Lesson>();
    }
}
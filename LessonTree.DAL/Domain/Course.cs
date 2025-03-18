namespace LessonTree.DAL.Domain
{
    public class Course
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public virtual List<Topic> Topics { get; set; } = new List<Topic>();
    }
}

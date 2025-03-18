namespace LessonTree.DAL.Domain
{
    public class Topic
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }
        public virtual List<SubTopic> SubTopics { get; set; } = new List<SubTopic>(); 
        public bool HasSubTopics { get; set; } = false; 
    }
}

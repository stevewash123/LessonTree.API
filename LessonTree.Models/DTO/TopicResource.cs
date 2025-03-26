namespace LessonTree.Models.DTO
{
    public class TopicResource
    {
        public int Id { get; set; }
        public string NodeId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int CourseId { get; set; } // Read-only, set via Move
        public bool hasChildren { get; set; }
        public List<SubTopicResource> SubTopics { get; set; }
        public List<LessonResource> Lessons { get; set; }
    }

    public class TopicCreateResource
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int CourseId { get; set; } // Required for creation, not editable later
        public string Visibility { get; set; } = "Private";
        public int? TeamId { get; set; }
    }

    public class TopicUpdateResource
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Visibility { get; set; }
        public int? TeamId { get; set; }
        // Removed CourseId - not editable via Update, only by Move
    }

    public class TopicMoveResource
    {
        public int TopicId { get; set; }
        public int NewCourseId { get; set; }
    }
}
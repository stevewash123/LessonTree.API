namespace LessonTree.Models.DTO
{
    public class SubTopicResource
    {
        public int Id { get; set; }
        public string NodeId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int TopicId { get; set; } // Read-only, set via Move
        public int CourseId { get; set; }
        public bool hasChildren { get; set; }
        public List<LessonResource> Lessons { get; set; }
    }

    public class SubTopicCreateResource
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public int TopicId { get; set; } // Required for creation, not editable later
        public string Visibility { get; set; } = "Private";
        public int? TeamId { get; set; }
    }

    public class SubTopicUpdateResource
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string Visibility { get; set; }
        public int? TeamId { get; set; }
        // Removed TopicId - not editable via Update, only by move
    }

    public class SubTopicMoveResource
    {
        public int SubTopicId { get; set; }
        public int NewTopicId { get; set; }
    }
}
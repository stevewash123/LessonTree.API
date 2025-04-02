// File: SubTopicResource.cs
using LessonTree.Models.Enums;

namespace LessonTree.Models.DTO
{
    public class SubTopicResource
    {
        public int Id { get; set; }
        public string NodeId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TopicId { get; set; }
        public int CourseId { get; set; }
        public bool HasChildren { get; set; }
        public bool Archived { get; set; }
        public int SortOrder { get; set; }
        public VisibilityType Visibility { get; set; }
        public List<LessonResource> Lessons { get; set; } = new List<LessonResource>();
        public List<NoteResource> Notes { get; set; } = new List<NoteResource>();
    }

    public class SubTopicCreateResource
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TopicId { get; set; }
        public VisibilityType Visibility { get; set; } = VisibilityType.Private;
        public int SortOrder { get; set; }
    }

    // no links, links can only be changed by move
    public class SubTopicUpdateResource
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public VisibilityType Visibility { get; set; }
        public bool Archived { get; set; }
        public int SortOrder { get; set; }
    }

    public class SubTopicMoveResource
    {
        public int SubTopicId { get; set; }
        public int NewTopicId { get; set; }
    }
}
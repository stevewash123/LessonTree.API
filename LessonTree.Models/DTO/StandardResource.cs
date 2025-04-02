// File: StandardResource.cs
namespace LessonTree.Models.DTO
{
    public class StandardResource
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int CourseId { get; set; }  // Added
        public int? TopicId { get; set; }  // Changed to nullable to match domain model
        public string? Description { get; set; }  
        public string? StandardType { get; set; }  
    }

    public class StandardCreateResource
    {
        public string Title { get; set; }
        public int CourseId { get; set; }  // Added
        public int? TopicId { get; set; }  // Changed to nullable to match domain model
        public string? Description { get; set; }
        public string? StandardType { get; set; }
    }

    public class StandardUpdateResource
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int CourseId { get; set; }  // Added
        public int? TopicId { get; set; }  // Changed to nullable to match domain model
        public string? Description { get; set; }
        public string? StandardType { get; set; }
    }
}
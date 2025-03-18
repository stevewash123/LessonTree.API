
namespace LessonTree.Models.DTO
{
    public class AttachmentResource
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FileSize { get; set; }
        public string ContentType { get; set; }
        public int LessonId { get; set; }
    }
}

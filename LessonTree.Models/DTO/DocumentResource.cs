
namespace LessonTree.Models.DTO
{
    public class DocumentResource
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public byte[] Blob { get; set; } // Renamed from Content
        public int LessonId { get; set; }
    }
}

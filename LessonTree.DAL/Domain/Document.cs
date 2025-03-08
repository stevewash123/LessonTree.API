namespace LessonTree.DAL.Domain
{
    public class Document
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public byte[] Blob { get; set; } // Renamed from Content
        public List<LessonDocument> LessonDocuments { get; set; } = new List<LessonDocument>();
    }
}

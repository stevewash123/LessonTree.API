namespace LessonTree.DAL.Domain
{
    public class Attachment
    {
        public int Id { get; set; }
        public AttachmentType Type { get; set; } // Distinguishes between uploaded files and Google Docs
        public string FileName { get; set; }    // Used for uploaded files
        public string? ContentType { get; set; } // Used for uploaded files
        public byte[] Blob { get; set; }        // Used for uploaded files
        public string? GoogleDocUrl { get; set; } // Stores the Google Doc URL
        public string? GoogleDocId { get; set; }  // Optional: Stores the Google Doc ID for API interactions
        public bool Shared { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<LessonAttachment> LessonAttachments { get; set; } = new List<LessonAttachment>();
    }

    public enum AttachmentType
    {
        UploadedFile,
        GoogleDoc
    }
}

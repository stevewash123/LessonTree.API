using System.Collections.Generic;

namespace LessonTree.Models.Reports
{
    public class ReportGenerationResult
    {
        public bool Success { get; set; }
        public byte[] PdfContent { get; set; } = new byte[0];
        public string HtmlContent { get; set; } = string.Empty;
        public ReportMetadata Metadata { get; set; } = new ReportMetadata();
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }
}
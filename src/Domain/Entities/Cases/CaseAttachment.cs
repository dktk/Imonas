using Domain.Common;

namespace Domain.Entities.Cases
{
    public class CaseAttachment : AuditableEntity
    {
        public int CaseId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string UploadedBy { get; set; } = string.Empty;
        public byte[] FileContent { get; set; } = Array.Empty<byte>();

        public virtual ExceptionCase Case { get; set; } = null!;
    }
}

using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class UploadedFile : AuditableEntity
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string? FileHash { get; set; }
        public FileStatus Status { get; set; }
        public DateTime UploadedAt { get; set; }
        public string UploadedBy { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public string? RejectionReason { get; set; }
        public int? PspId { get; set; }
        public string? SourceSystem { get; set; }

        public virtual Psp? Psp { get; set; }
    }
}

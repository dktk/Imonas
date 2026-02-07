using Imonas.V9.Domain.Common;
using Imonas.V9.Domain.Enums;

namespace Imonas.V9.Domain.Entities.Bronze;

public class BronzeFile : AuditableEntity
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string FileHash { get; set; } = string.Empty;
    public FileStatus Status { get; set; }
    public DateTime UploadedAt { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string? RejectionReason { get; set; }
    public Guid? PspProfileId { get; set; }
    public string SourceSystem { get; set; } = string.Empty;

    public virtual ICollection<BronzeRecord> Records { get; set; } = new List<BronzeRecord>();
}

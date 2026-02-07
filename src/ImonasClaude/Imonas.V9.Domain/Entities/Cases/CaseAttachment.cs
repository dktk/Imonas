using Imonas.V9.Domain.Common;

namespace Imonas.V9.Domain.Entities.Cases;

public class CaseAttachment : BaseEntity
{
    public Guid CaseId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;

    public virtual Case Case { get; set; } = null!;
}

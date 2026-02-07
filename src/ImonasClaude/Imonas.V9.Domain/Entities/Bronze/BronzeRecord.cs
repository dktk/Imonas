using Imonas.V9.Domain.Common;

namespace Imonas.V9.Domain.Entities.Bronze;

public class BronzeRecord : AuditableEntity
{
    public Guid BronzeFileId { get; set; }
    public int RowNumber { get; set; }
    public string RawData { get; set; } = string.Empty;
    public string? ParsedData { get; set; }
    public bool IsValid { get; set; }
    public string? ValidationErrors { get; set; }

    public virtual BronzeFile BronzeFile { get; set; } = null!;
}

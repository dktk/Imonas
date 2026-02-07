using Imonas.V9.Domain.Common;

namespace Imonas.V9.Domain.Entities.Cases;

public class CaseLabel : BaseEntity
{
    public Guid CaseId { get; set; }
    public string LabelName { get; set; } = string.Empty;
    public string? LabelColor { get; set; }

    public virtual Case Case { get; set; } = null!;
}

using Imonas.V9.Domain.Common;

namespace Imonas.V9.Domain.Entities.Cases;

public class CaseComment : BaseEntity
{
    public Guid CaseId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string CommentedBy { get; set; } = string.Empty;

    public virtual Case Case { get; set; } = null!;
}

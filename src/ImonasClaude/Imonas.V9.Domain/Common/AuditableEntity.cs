namespace Imonas.V9.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    public string SourceId { get; set; } = string.Empty;
    public Guid RunId { get; set; }
    public string RulePackVersion { get; set; } = string.Empty;
    public string ContentHash { get; set; } = string.Empty;
}

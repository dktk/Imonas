using Imonas.V9.Domain.Common;

namespace Imonas.V9.Domain.Entities.Configuration;

public class FieldMapping : BaseEntity
{
    public Guid PspProfileId { get; set; }
    public string SourceField { get; set; } = string.Empty;
    public string TargetField { get; set; } = string.Empty;
    public string? TransformExpression { get; set; }
    public string Version { get; set; } = string.Empty;
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
}

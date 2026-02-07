using Domain.Common;

namespace Domain.Entities.Configuration
{
    public class FieldMapping : AuditableEntity
    {
        public int PspId { get; set; }
        public string SourceField { get; set; } = string.Empty;
        public string TargetField { get; set; } = string.Empty;
        public string? TransformExpression { get; set; }
        public string Version { get; set; } = string.Empty;
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsActive { get; set; }

        public virtual Psp Psp { get; set; } = null!;
    }
}

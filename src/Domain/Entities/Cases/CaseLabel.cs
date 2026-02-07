using Domain.Common;

namespace Domain.Entities.Cases
{
    public class CaseLabel : AuditableEntity
    {
        public int CaseId { get; set; }
        public string LabelName { get; set; } = string.Empty;
        public string? LabelColor { get; set; }

        public virtual ExceptionCase Case { get; set; } = null!;
    }
}

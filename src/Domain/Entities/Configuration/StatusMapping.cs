using Domain.Common;

namespace Domain.Entities.Configuration
{
    public class StatusMapping : AuditableEntity
    {
        public int PspId { get; set; }
        public string PspStatus { get; set; } = string.Empty;
        public string CanonicalStatus { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Version { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        public virtual Psp Psp { get; set; } = null!;
    }
}

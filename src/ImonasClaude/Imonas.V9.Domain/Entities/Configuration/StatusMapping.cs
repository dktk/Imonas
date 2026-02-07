using Imonas.V9.Domain.Common;

namespace Imonas.V9.Domain.Entities.Configuration;

public class StatusMapping : BaseEntity
{
    public Guid PspProfileId { get; set; }
    public string PspStatus { get; set; } = string.Empty;
    public string CanonicalStatus { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Version { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

using Imonas.V9.Domain.Common;

namespace Imonas.V9.Domain.Entities.Finance;

public class FeeContract : BaseEntity
{
    public Guid PspProfileId { get; set; }
    public string ContractName { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string FeeStructure { get; set; } = string.Empty;
    public decimal? FixedFee { get; set; }
    public decimal? PercentageFee { get; set; }
    public decimal? MinimumFee { get; set; }
    public decimal? MaximumFee { get; set; }
    public string? TieredStructure { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
}

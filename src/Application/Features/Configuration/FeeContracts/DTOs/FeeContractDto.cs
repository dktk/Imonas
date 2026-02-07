using Domain.Entities.Configuration;

namespace Application.Features.Configuration.FeeContracts.DTOs
{
    public class FeeContractDto : IMapFrom<FeeContract>
    {
        public int Id { get; set; }
        public int PspId { get; set; }
        public string PspName { get; set; } = string.Empty;
        public string ContractName { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public FeeStructureType FeeStructure { get; set; }
        public decimal? FixedFee { get; set; }
        public decimal? PercentageFee { get; set; }
        public decimal? MinimumFee { get; set; }
        public decimal? MaximumFee { get; set; }
        public string? TieredStructure { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string Version { get; set; } = string.Empty;
        public ContractStatus Status { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? LastModified { get; set; }
    }
}

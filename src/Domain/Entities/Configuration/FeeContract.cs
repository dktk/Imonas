namespace Domain.Entities.Configuration
{
    public class FeeContract : AuditableEntity
    {
        public int PspId { get; set; }
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
        public ContractStatus Status { get; set; } = ContractStatus.Draft;
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? RejectionReason { get; set; }

        public virtual Psp Psp { get; set; } = null!;
    }
}

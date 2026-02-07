using Imonas.V9.Domain.Common;

namespace Imonas.V9.Domain.Entities.Finance;

public class Settlement : BaseEntity
{
    public string PspName { get; set; } = string.Empty;
    public string MerchantId { get; set; } = string.Empty;
    public DateTime SettlementDate { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal GrossAmount { get; set; }
    public decimal RefundsAmount { get; set; }
    public decimal ChargebacksAmount { get; set; }
    public decimal FeesAmount { get; set; }
    public decimal ReservesAmount { get; set; }
    public decimal FxAdjustmentAmount { get; set; }
    public decimal NetExpectedAmount { get; set; }
    public decimal? ActualAmount { get; set; }
    public decimal? VarianceAmount { get; set; }
    public Guid? BankTransactionId { get; set; }
    public bool IsReconciled { get; set; }
    public DateTime? ReconciledAt { get; set; }
}

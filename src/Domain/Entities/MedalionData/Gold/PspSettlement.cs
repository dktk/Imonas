using Domain.Entities.MedalionData.Silver;

namespace Domain.Entities.MedalionData.Gold;

public enum ReconciliationStatus
{
    Successful = 1,
    Pending = 2,
    Failed = 0
}

/// <summary>
/// Golden Medallion.
/// </summary>
public class PspSettlement : AuditableEntity
{
    public ReconciliationStatus ReconciliationStatus { get; set; }
    public List<ReconciliationComment> ReconciliationComments { get; set; }

    public DateTime TxDate { get; set; }

    public int PspId { get; set; } = default!;

    public string CurrencyCode { get; set; } = default!;
    public int Amount { get; set; }

    public decimal TotalFees { get; set; }

    public decimal NetSettlement { get; set; }
    public InternalPayment InternalPayment { get; set; }
    public int InternalPaymentId { get; set; }
    public ExternalPayment ExternalPayment { get; set; }
    public int ExternalPaymentId { get; set; }
    public ReconciliationRun ReconciliationRun { get; set; }
    public int ReconciliationRunId { get; set; }

}

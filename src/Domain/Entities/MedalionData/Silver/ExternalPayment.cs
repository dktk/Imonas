using Domain.Entities.MedalionData.Bronze;

namespace Domain.Entities.MedalionData.Silver;

/// <summary>
/// Silver Medallion.
/// </summary>
public class ExternalPayment : AuditableEntity
{
    private string status = default!;
    private string currencyCode = default!;

    public required PaymentAction Action { get; set; }

    public required string ExternalSystem { get; set; } = default!;

    public required string ExternalPaymentId { get; set; } = default!;

    public required string PlayerId { get; set; } = default!;

    public string BrandId { get; set; } = default!;

    public required int PspId { get; set; } = default!;

    public required decimal Amount { get; set; }

    public required string CurrencyCode { get => currencyCode; set => currencyCode = value.ToUpper(); }
    public required DateTime TxDate { get; set; }
    public required string TxId { get; set; }

    public required string Status { get => status; set => status = value.ToUpper(); }
    public required int RawPaymentId { get; set; }
    public RawPayment RawPayment { get; set; } = default!;
    public Psp Psp { get; set; } = default!;
    public int Id { get; set; }
    public required int Hash { get; set; }
}

/// <summary>
/// Silver Medallion.
/// </summary>
public class InternalPayment : AuditableEntity
{
    private string status;
    private string currencyCode;

    /// <summary>
    /// The Id of the Internal System transaction.
    /// </summary>
    public string TxId { get; set; }

    /// <summary>
    /// The Id of the PSP transaction.
    /// </summary>
    public string ProviderTxId { get; set; }

    // todo: should we remove
    public required string RefNumber { get; set; }
    public required DateTimeOffset TxDate { get; set; }

    public required string Status
    {
        get => status; set => status = value.ToUpper();
    }

    public required decimal Amount { get; set; }
    public required string CurrencyCode { get => currencyCode; set => currencyCode = value.ToUpper(); }
    public required string UserEmail { get; set; }
    public string? Description { get; set; }
    public required string System { get; set; }
    public required int Hash { get; set; }
}

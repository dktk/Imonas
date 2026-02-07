using Imonas.V9.Domain.Common;

namespace Imonas.V9.Domain.Entities.Silver;

public class SilverTransaction : AuditableEntity
{
    public string TransactionId { get; set; } = string.Empty;
    public string? ExternalTransactionId { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? AuthCode { get; set; }
    public string? Reference { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? MerchantId { get; set; }
    public string? Psp { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public Guid BronzeRecordId { get; set; }
}

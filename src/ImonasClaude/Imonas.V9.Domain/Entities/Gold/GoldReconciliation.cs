using Imonas.V9.Domain.Common;
using Imonas.V9.Domain.Enums;

namespace Imonas.V9.Domain.Entities.Gold;

public class GoldReconciliation : AuditableEntity
{
    public Guid InternalTransactionId { get; set; }
    public Guid? PspTransactionId { get; set; }
    public Guid? BankTransactionId { get; set; }
    public MatchStatus MatchStatus { get; set; }
    public string? MatchRuleApplied { get; set; }
    public decimal? MatchScore { get; set; }
    public decimal? AmountVariance { get; set; }
    public int? DateVarianceDays { get; set; }
    public bool IsManualMatch { get; set; }
    public string? MatchedBy { get; set; }
    public DateTime? MatchedAt { get; set; }
    public string? Notes { get; set; }
}

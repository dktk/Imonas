using Imonas.V9.Domain.Common;
using Imonas.V9.Domain.Enums;

namespace Imonas.V9.Domain.Entities.Reconciliation;

public class ReconciliationRun : BaseEntity
{
    public string RunName { get; set; } = string.Empty;
    public RunStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string RulePackVersion { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public int MatchedRecords { get; set; }
    public int UnmatchedRecords { get; set; }
    public int PartialMatchRecords { get; set; }
    public decimal MatchPercentage { get; set; }
    public string? ErrorMessage { get; set; }
    public string? EvidencePackPath { get; set; }
    public string? EvidencePackHash { get; set; }
    public bool IsReplayable { get; set; }

    public virtual ICollection<RunMetric> Metrics { get; set; } = new List<RunMetric>();
}

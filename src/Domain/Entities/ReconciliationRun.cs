namespace Domain.Entities
{
    public class ReconciliationRun : AuditableEntity
    {
        public string RunName { get; set; } = string.Empty;

        public RunStatus Status { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string RulePackVersion { get; set; } = string.Empty;
        public int InternalMatchedRecordsCount { get; set; }
        public int ExternalMatchedRecordsCount { get; set; }

        public int InternalUnmatchedRecordsCount { get; set; }
        public int ExternalUnmatchedRecordsCount { get; set; }
        public int PartialMatchRecordsCount { get; set; }
        public decimal MatchPercentage { get; set; }
        public string? ErrorMessage { get; set; }
        public string? EvidencePackPath { get; set; }
        public string? EvidencePackHash { get; set; }
        public bool IsReplayable { get; set; }

        public bool IsArchived { get; set; }
        public DateTime? ArchivedAt { get; set; }
        public string? ArchivedBy { get; set; }
        public string? ArchiveComment { get; set; }

        public virtual ICollection<RunMetric> Metrics { get; set; } = new List<RunMetric>();
    }
}

using Domain.Entities.Identity;

namespace Domain.Entities.Cases
{
    public class ExceptionCase : AuditableEntity
    {
        public string CaseNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public CaseStatus Status { get; set; }
        public CaseSeverity Severity { get; set; }
        public VarianceType VarianceType { get; set; }
        public string? AssignedToId { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolvedBy { get; set; }
        public string? ResolutionNotes { get; set; }
        public int? ExternalTransactionId { get; set; }
        public int? InternalTransactionId { get; set; }
        public decimal? VarianceAmount { get; set; }
        public string? RootCauseCode { get; set; }
        public int? ReconciliationRunId { get; set; }
        public ReconciliationRun ReconciliationRun { get; set; }
        public User? AssignedTo { get; set; }
        public virtual ICollection<CaseComment> Comments { get; set; } = new List<CaseComment>();
        public virtual ICollection<CaseAttachment> Attachments { get; set; } = new List<CaseAttachment>();
        public virtual ICollection<CaseLabel> Labels { get; set; } = new List<CaseLabel>();
    }
}

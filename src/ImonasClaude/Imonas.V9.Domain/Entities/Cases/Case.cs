using Imonas.V9.Domain.Common;
using Imonas.V9.Domain.Enums;

namespace Imonas.V9.Domain.Entities.Cases;

public class Case : BaseEntity
{
    public string CaseNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CaseStatus Status { get; set; }
    public CaseSeverity Severity { get; set; }
    public VarianceType VarianceType { get; set; }
    public string? AssignedTo { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolvedBy { get; set; }
    public string? ResolutionNotes { get; set; }
    public Guid? LinkedTransactionId { get; set; }
    public decimal? VarianceAmount { get; set; }
    public string? RootCauseCode { get; set; }

    public virtual ICollection<CaseComment> Comments { get; set; } = new List<CaseComment>();
    public virtual ICollection<CaseAttachment> Attachments { get; set; } = new List<CaseAttachment>();
    public virtual ICollection<CaseLabel> Labels { get; set; } = new List<CaseLabel>();
}

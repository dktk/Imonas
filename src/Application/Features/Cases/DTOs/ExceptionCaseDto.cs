using Application.Common.Mappings;
using Domain.Entities.Cases;
using Domain.Enums;

namespace Application.Features.Cases.DTOs
{
    public class ExceptionCaseDto : IMapFrom<ExceptionCase>
    {
        public int Id { get; set; }
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
        public int? LinkedTransactionId { get; set; }
        public decimal? VarianceAmount { get; set; }
        public string? RootCauseCode { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class CaseCommentDto : IMapFrom<CaseComment>
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string CommentedBy { get; set; } = string.Empty;
        public DateTime Created { get; set; }
    }

    public class CaseAttachmentDto : IMapFrom<CaseAttachment>
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string UploadedBy { get; set; } = string.Empty;
        public DateTime Created { get; set; }
    }

    public class CaseLabelDto : IMapFrom<CaseLabel>
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public string LabelName { get; set; } = string.Empty;
        public string? LabelColor { get; set; }
    }

    public class ExceptionCaseDetailsDto : IMapFrom<ExceptionCase>
    {
        public int Id { get; set; }
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
        public int? LinkedTransactionId { get; set; }
        public decimal? VarianceAmount { get; set; }
        public string? RootCauseCode { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public ICollection<CaseCommentDto> Comments { get; set; } = new List<CaseCommentDto>();
        public ICollection<CaseAttachmentDto> Attachments { get; set; } = new List<CaseAttachmentDto>();
        public ICollection<CaseLabelDto> Labels { get; set; } = new List<CaseLabelDto>();
    }
}

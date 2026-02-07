using Application.Common.Mappings;
using Domain.Enums;

namespace Application.Features.ReconciliationRuns.DTOs
{
    public class ReconciliationRunDto : IMapFrom<ReconciliationRun>
    {
        public int Id { get; set; }
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
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class RunMetricDto : IMapFrom<RunMetric>
    {
        public int Id { get; set; }
        public int RunId { get; set; }
        public string MetricName { get; set; } = string.Empty;
        public string MetricValue { get; set; } = string.Empty;
        public string? MetricCategory { get; set; }
    }

    public class ReconciliationRunDetailsDto : IMapFrom<ReconciliationRun>
    {
        public int Id { get; set; }
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
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public ICollection<RunMetricDto> Metrics { get; set; } = new List<RunMetricDto>();
    }
}

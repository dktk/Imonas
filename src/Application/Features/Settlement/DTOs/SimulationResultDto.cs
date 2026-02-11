namespace Application.Features.Settlement.DTOs
{
    public class SimulationResultDto
    {
        public TimeSpan Duration { get; set; }
        public DateTime SimulatedAt { get; set; }
        public int TotalInternalTransactions { get; set; }
        public int TotalExternalTransactions { get; set; }

        public SimulationRuleSetResultDto Baseline { get; set; } = new();
        public SimulationRuleSetResultDto Candidate { get; set; } = new();
        public SimulationLiftDto Lift { get; set; } = new();
        public FalsePositiveAnalysisDto FalsePositives { get; set; } = new();

        public List<int> BaselineRuleIds { get; set; } = new();
        public List<int> CandidateRuleIds { get; set; } = new();
    }

    public class SimulationRuleSetResultDto
    {
        public int MatchedCount { get; set; }
        public int PartialMatchCount { get; set; }
        public int InternalUnmatchedCount { get; set; }
        public int ExternalUnmatchedCount { get; set; }
        public decimal MatchPercentage { get; set; }
        public List<SimulationMatchDto> Matches { get; set; } = new();
    }

    public class SimulationMatchDto
    {
        public int InternalPaymentId { get; set; }
        public int ExternalPaymentId { get; set; }
        public string InternalTxId { get; set; } = string.Empty;
        public string ExternalTxId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string RuleApplied { get; set; } = string.Empty;
        public decimal MatchScore { get; set; }
        public decimal? AmountVariance { get; set; }
        public int? DateVarianceDays { get; set; }
        public string? Notes { get; set; }
    }

    public class SimulationLiftDto
    {
        public int MatchedDelta { get; set; }
        public int PartialMatchDelta { get; set; }
        public int UnmatchedDelta { get; set; }
        public decimal MatchPercentageDelta { get; set; }
        public List<SimulationMatchDto> NewMatches { get; set; } = new();
        public List<SimulationMatchDto> LostMatches { get; set; } = new();
        public List<SimulationMatchDiffDto> ChangedMatches { get; set; } = new();
    }

    public class SimulationMatchDiffDto
    {
        public int InternalPaymentId { get; set; }
        public int ExternalPaymentId { get; set; }
        public string BaselineRule { get; set; } = string.Empty;
        public decimal BaselineScore { get; set; }
        public string CandidateRule { get; set; } = string.Empty;
        public decimal CandidateScore { get; set; }
        public decimal ScoreDelta { get; set; }
    }

    public class FalsePositiveAnalysisDto
    {
        public List<SimulationMatchDto> UnverifiedMatches { get; set; } = new();
        public List<SimulationMatchDto> LowConfidenceMatches { get; set; } = new();
        public int UnverifiedCount { get; set; }
        public int LowConfidenceCount { get; set; }
        public decimal FalsePositiveThreshold { get; set; }
    }
}

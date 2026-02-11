using Domain.Entities.MedalionData.Gold;

namespace Application.Features.Settlement.DTOs
{
    public class SettlementMatchDto
    {
        public int InternalPaymentId { get; set; }
        public int ExternalPaymentId { get; set; }
        public string RuleApplied { get; set; } = string.Empty;
        public decimal MatchScore { get; set; }
        public ReconciliationStatus Status { get; set; }
        public decimal? AmountVariance { get; set; }
        public int? DateVarianceDays { get; set; }
        public string? Notes { get; set; }
    }

    public class SettlementResultDto
    {
        public int RunId { get; set; }
        public string RunName { get; set; } = string.Empty;
        public int TotalInternalTransactions { get; set; }
        public int TotalExternalTransactions { get; set; }
        public int MatchedCount => Matches.Count;
        public int PartialMatchCount => PartialMatches.Count;
        public int InternalUnmatchedCount => UnmatchedInternal.Count;
        public int ExternalUnmatchedCount => UnmatchedExternal.Count;
        public decimal MatchPercentage { get; set; }
        public TimeSpan Duration { get; set; }
        public List<SettlementMatchDto> Matches { get; set; } = new();
        public List<UnmatchedTransactionDto> UnmatchedInternal { get; set; } = new();
        public List<UnmatchedTransactionDto> UnmatchedExternal { get; set; } = new();

        /// <summary>
        /// A list of Internal, External pairs, partially mapped
        /// </summary>
        public List<(UnmatchedTransactionDto, UnmatchedTransactionDto)> PartialMatches { get; set; } = new();
    }

    public class UnmatchedTransactionDto
    {
        public int Id { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty; // "Internal" or "External"
    }

    public enum RuleMatchResultType
    {
        NoMatch,
        Match,
        PartialMatch,
    }

    public class RuleMatchResult
    {
        public RuleMatchResultType Value { get; set; }

        public bool IsMatch => Value == RuleMatchResultType.Match;
        public bool IsNotMatch => Value == RuleMatchResultType.NoMatch;
        public bool IsPartialMatch => Value == RuleMatchResultType.PartialMatch;

        public decimal Score { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public decimal? AmountVariance { get; set; }
        public int? DateVarianceDays { get; set; }
        public string? Notes { get; set; }
        public List<(string, string, string)> UnmatchedFields { get; set; } = new List<(string, string, string)>();
    }

    public class SettlementRunOptionsDto
    {
        public int? PspId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CurrencyCode { get; set; }
        public bool StopAtFirstMatch { get; set; } = true;
        public bool CreateCasesForExceptions { get; set; } = true;
        public decimal? MinimumMatchScore { get; set; } = 0.8m;

        /// <summary>
        /// The user ID of the person running the settlement. Used for case assignment.
        /// </summary>
        public string? CurrentUserId { get; set; }
    }
}

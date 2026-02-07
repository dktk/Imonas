using Imonas.V9.Domain.Common;
using Imonas.V9.Domain.Enums;

namespace Imonas.V9.Domain.Entities.Rules;

public class MatchingRule : BaseEntity
{
    public string RuleName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RuleType RuleType { get; set; }
    public string RuleDefinition { get; set; } = string.Empty;
    public int Priority { get; set; }
    public bool IsActive { get; set; }
    public bool StopAtFirstMatch { get; set; }
    public decimal? ToleranceAmount { get; set; }
    public int? ToleranceWindowDays { get; set; }
    public decimal? MinimumScore { get; set; }
    public string Version { get; set; } = string.Empty;
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}

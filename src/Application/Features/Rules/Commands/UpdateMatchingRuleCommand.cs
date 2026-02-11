using SG.Common;

namespace Application.Features.Rules.Commands
{
    public class UpdateMatchingRuleCommand : IRequest<Result<MatchingRuleResultDto>>
    {
        public int Id { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public RuleType RuleType { get; set; }
        public string RuleDefinition { get; set; } = "{}";
        public int Priority { get; set; } = 100;
        public bool IsActive { get; set; } = true;
        public bool StopAtFirstMatch { get; set; }
        public decimal? ToleranceAmount { get; set; }
        public int? ToleranceWindowDays { get; set; }
        public decimal? MinimumScore { get; set; }
        public string Version { get; set; } = "1.0.0";
        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
        public DateTime? EffectiveTo { get; set; }
    }

    public class UpdateMatchingRuleCommandHandler(
        IApplicationDbContext context) :
        IRequestHandler<UpdateMatchingRuleCommand, Result<MatchingRuleResultDto>>
    {
        public async Task<Result<MatchingRuleResultDto>> Handle(UpdateMatchingRuleCommand request, CancellationToken cancellationToken)
        {
            var rule = await context.MatchingRules
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (rule == null)
            {
                return Result<MatchingRuleResultDto>.BuildFailure("Rule not found.");
            }

            // Check for duplicate rule name (excluding current rule)
            var existingRule = await context.MatchingRules
                .Where(r => r.RuleName == request.RuleName && r.IsActive && r.Id != request.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingRule != null)
            {
                return Result<MatchingRuleResultDto>.BuildFailure($"An active rule with name '{request.RuleName}' already exists.");
            }

            // Validate rule definition JSON
            try
            {
                System.Text.Json.JsonDocument.Parse(request.RuleDefinition);
            }
            catch
            {
                return Result<MatchingRuleResultDto>.BuildFailure("Rule definition must be valid JSON.");
            }

            rule.RuleName = request.RuleName;
            rule.Description = request.Description;
            rule.RuleType = request.RuleType;
            rule.RuleDefinition = request.RuleDefinition;
            rule.Priority = request.Priority;
            rule.IsActive = request.IsActive;
            rule.StopAtFirstMatch = request.StopAtFirstMatch;
            rule.ToleranceAmount = request.ToleranceAmount;
            rule.ToleranceWindowDays = request.ToleranceWindowDays;
            rule.MinimumScore = request.MinimumScore;
            rule.Version = request.Version;
            rule.EffectiveFrom = request.EffectiveFrom;
            rule.EffectiveTo = request.EffectiveTo;

            await context.SaveChangesAsync(cancellationToken);

            var dto = new MatchingRuleResultDto
            {
                Id = rule.Id,
                RuleName = rule.RuleName,
                RuleType = rule.RuleType,
                Priority = rule.Priority
            };

            return Result<MatchingRuleResultDto>.BuildSuccess(dto, $"Matching rule '{request.RuleName}' updated successfully.");
        }
    }
}

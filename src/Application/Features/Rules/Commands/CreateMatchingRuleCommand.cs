using Domain.Entities.Rules;
using SG.Common;

namespace Application.Features.Rules.Commands
{
    public class CreateMatchingRuleCommand : IRequest<Result<MatchingRuleResultDto>>
    {
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

    public class MatchingRuleResultDto
    {
        public int Id { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public RuleType RuleType { get; set; }
        public int Priority { get; set; }
    }

    public class CreateMatchingRuleCommandHandler(
        IApplicationDbContext context) :
        IRequestHandler<CreateMatchingRuleCommand, Result<MatchingRuleResultDto>>
    {
        public async Task<Result<MatchingRuleResultDto>> Handle(CreateMatchingRuleCommand request, CancellationToken cancellationToken)
        {
            // Check for duplicate rule name
            var existingRule = await context.MatchingRules
                .Where(r => r.RuleName == request.RuleName && r.IsActive)
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

            var rule = new MatchingRule
            {
                RuleName = request.RuleName,
                Description = request.Description,
                RuleType = request.RuleType,
                RuleDefinition = request.RuleDefinition,
                Priority = request.Priority,
                IsActive = request.IsActive,
                StopAtFirstMatch = request.StopAtFirstMatch,
                ToleranceAmount = request.ToleranceAmount,
                ToleranceWindowDays = request.ToleranceWindowDays,
                MinimumScore = request.MinimumScore,
                Version = request.Version,
                EffectiveFrom = request.EffectiveFrom,
                EffectiveTo = request.EffectiveTo
            };

            context.MatchingRules.Add(rule);
            await context.SaveChangesAsync(cancellationToken);

            var dto = new MatchingRuleResultDto
            {
                Id = rule.Id,
                RuleName = rule.RuleName,
                RuleType = rule.RuleType,
                Priority = rule.Priority
            };

            return Result<MatchingRuleResultDto>.BuildSuccess(dto, $"Matching rule '{request.RuleName}' created successfully.");
        }
    }
}

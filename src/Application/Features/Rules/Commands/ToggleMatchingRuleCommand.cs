using SG.Common;

namespace Application.Features.Rules.Commands
{
    public class ToggleMatchingRuleCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }

    public class ToggleMatchingRuleCommandHandler(
        IApplicationDbContext context) :
        IRequestHandler<ToggleMatchingRuleCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(ToggleMatchingRuleCommand request, CancellationToken cancellationToken)
        {
            var rule = await context.MatchingRules
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (rule == null)
            {
                return Result<bool>.BuildFailure($"Matching rule with ID {request.Id} not found.");
            }

            rule.IsActive = !rule.IsActive;
            await context.SaveChangesAsync(cancellationToken);

            var status = rule.IsActive ? "activated" : "deactivated";
            return Result<bool>.BuildSuccess(rule.IsActive, $"Rule '{rule.RuleName}' {status} successfully.");
        }
    }
}

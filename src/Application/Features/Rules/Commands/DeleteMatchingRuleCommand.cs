using SG.Common;

namespace Application.Features.Rules.Commands
{
    public class DeleteMatchingRuleCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }

    public class DeleteMatchingRuleCommandHandler(
        IApplicationDbContext context) :
        IRequestHandler<DeleteMatchingRuleCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteMatchingRuleCommand request, CancellationToken cancellationToken)
        {
            var rule = await context.MatchingRules
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (rule == null)
            {
                return Result<bool>.BuildFailure($"Matching rule with ID {request.Id} not found.");
            }

            var ruleName = rule.RuleName;
            context.MatchingRules.Remove(rule);
            await context.SaveChangesAsync(cancellationToken);

            return Result<bool>.BuildSuccess(true, $"Rule '{ruleName}' deleted successfully.");
        }
    }
}

using Domain.Entities.Rules;

namespace Application.Features.Rules.Queries
{
    public class MatchingRuleDto : IMapFrom<MatchingRule>
    {
        public int Id { get; set; }
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
        public DateTime Created { get; set; }
    }

    public class GetMatchingRulesQuery : IRequest<IEnumerable<MatchingRuleDto>>
    {
    }

    public class GetMatchingRulesQueryHandler(
        IApplicationDbContext context,
        IMapper mapper) :
        IRequestHandler<GetMatchingRulesQuery, IEnumerable<MatchingRuleDto>>
    {
        public async Task<IEnumerable<MatchingRuleDto>> Handle(GetMatchingRulesQuery request, CancellationToken cancellationToken)
        {
            return await context.MatchingRules
                .OrderBy(r => r.Priority)
                .ProjectTo<MatchingRuleDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }

    public class GetActiveRulesCountQuery : IRequest<int>
    {
    }

    public class GetActiveRulesCountQueryHandler(IApplicationDbContext context) :
        IRequestHandler<GetActiveRulesCountQuery, int>
    {
        public async Task<int> Handle(GetActiveRulesCountQuery request, CancellationToken cancellationToken)
        {
            return await context.MatchingRules
                .Where(r => r.IsActive)
                .CountAsync(cancellationToken);
        }
    }
}

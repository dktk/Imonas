namespace Application.Features.Rules.Queries
{
    public class GetMatchingRuleByIdQuery : IRequest<MatchingRuleDto?>
    {
        public int Id { get; set; }
    }

    public class GetMatchingRuleByIdQueryHandler(
        IApplicationDbContext context,
        IMapper mapper) :
        IRequestHandler<GetMatchingRuleByIdQuery, MatchingRuleDto?>
    {
        public async Task<MatchingRuleDto?> Handle(GetMatchingRuleByIdQuery request, CancellationToken cancellationToken)
        {
            return await context.MatchingRules
                .Where(r => r.Id == request.Id)
                .ProjectTo<MatchingRuleDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}

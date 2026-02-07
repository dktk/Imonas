using Application.Features.Cases.DTOs;

namespace Application.Features.Cases.Queries
{
    public class GetCasesQuery : IRequest<IEnumerable<ExceptionCaseDto>>
    {
        public int PageSize { get; set; } = 100;
    }

    public class GetCasesQueryHandler(
        IApplicationDbContext context,
        IMapper mapper) :
        IRequestHandler<GetCasesQuery, IEnumerable<ExceptionCaseDto>>
    {
        public async Task<IEnumerable<ExceptionCaseDto>> Handle(GetCasesQuery request, CancellationToken cancellationToken)
        {
            return await context.ExceptionCases
                .OrderByDescending(c => c.Created)
                .Take(request.PageSize)
                .ProjectTo<ExceptionCaseDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}

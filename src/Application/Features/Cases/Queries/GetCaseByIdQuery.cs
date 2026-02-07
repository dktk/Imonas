using Application.Features.Cases.DTOs;

namespace Application.Features.Cases.Queries
{
    public class GetCaseByIdQuery : IRequest<ExceptionCaseDetailsDto?>
    {
        public int Id { get; set; }
    }

    public class GetCaseByIdQueryHandler(
        IApplicationDbContext context,
        IMapper mapper) :
        IRequestHandler<GetCaseByIdQuery, ExceptionCaseDetailsDto?>
    {
        public async Task<ExceptionCaseDetailsDto?> Handle(GetCaseByIdQuery request, CancellationToken cancellationToken)
        {
            var caseEntity = await context.ExceptionCases
                .Include(c => c.Comments)
                .Include(c => c.Attachments)
                .Include(c => c.Labels)
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (caseEntity == null)
                return null;

            return mapper.Map<ExceptionCaseDetailsDto>(caseEntity);
        }
    }
}

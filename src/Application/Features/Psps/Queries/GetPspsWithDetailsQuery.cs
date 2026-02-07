namespace Application.Features.Psps.Queries
{
    public class PspDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsCsvBased { get; set; }
        public int InternalSystemId { get; set; }
        public string InternalSystemName { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public DateTime? LastModified { get; set; }
        public int TransactionCount { get; set; }
        public int FieldMappingCount { get; set; }
    }

    public class GetPspsWithDetailsQuery : IRequest<List<PspDetailDto>>
    {
    }

    public class GetPspsWithDetailsQueryHandler(
        IApplicationDbContext context) : IRequestHandler<GetPspsWithDetailsQuery, List<PspDetailDto>>
    {
        public async Task<List<PspDetailDto>> Handle(GetPspsWithDetailsQuery request, CancellationToken cancellationToken)
        {
            var psps = await context.Psps
                .Include(p => p.InternalSystem)
                .Select(p => new PspDetailDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Code = p.Code,
                    IsActive = p.IsActive,
                    IsCsvBased = p.IsCsvBased,
                    InternalSystemId = p.InternalSystemId,
                    InternalSystemName = p.InternalSystem.Name,
                    Created = p.Created,
                    LastModified = p.LastModified,
                    TransactionCount = context.ExternalPayments.Count(ep => ep.PspId == p.Id),
                    FieldMappingCount = context.FieldMappings.Count(fm => fm.PspId == p.Id)
                })
                .OrderBy(p => p.Name)
                .ToListAsync(cancellationToken);

            return psps;
        }
    }
}

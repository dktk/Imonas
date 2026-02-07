namespace Application.Features.Psps.Queries
{
    public class InternalSystemListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int PspCount { get; set; }
        public DateTime Created { get; set; }
    }

    public class GetInternalSystemsWithCountQuery : IRequest<List<InternalSystemListDto>>
    {
    }

    public class GetInternalSystemsWithCountQueryHandler(
        IApplicationDbContext context) : IRequestHandler<GetInternalSystemsWithCountQuery, List<InternalSystemListDto>>
    {
        public async Task<List<InternalSystemListDto>> Handle(GetInternalSystemsWithCountQuery request, CancellationToken cancellationToken)
        {
            return await context.InternalSystems
                .Include(s => s.Psps)
                .Select(s => new InternalSystemListDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    IsActive = s.IsActive,
                    PspCount = s.Psps != null ? s.Psps.Count : 0,
                    Created = s.Created
                })
                .OrderBy(s => s.Name)
                .ToListAsync(cancellationToken);
        }
    }
}

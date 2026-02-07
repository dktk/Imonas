namespace Application.Features.Psps.Queries
{
    public class InternalSystemDetailDto : IMapFrom<InternalSystem>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int PspCount { get; set; }
        public DateTime Created { get; set; }
        public DateTime? LastModified { get; set; }
    }

    public class GetInternalSystemByIdQuery : IRequest<InternalSystemDetailDto?>
    {
        public int Id { get; set; }
    }

    public class GetInternalSystemByIdQueryHandler(
        IApplicationDbContext context) : IRequestHandler<GetInternalSystemByIdQuery, InternalSystemDetailDto?>
    {
        public async Task<InternalSystemDetailDto?> Handle(GetInternalSystemByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await context.InternalSystems
                .Include(s => s.Psps)
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (entity == null)
            {
                return null;
            }

            return new InternalSystemDetailDto
            {
                Id = entity.Id,
                Name = entity.Name,
                IsActive = entity.IsActive,
                PspCount = entity.Psps?.Count ?? 0,
                Created = entity.Created,
                LastModified = entity.LastModified
            };
        }
    }
}

using Application.Common.Mappings;
using Domain.Entities.Configuration;

namespace Application.Features.Configuration.Queries
{
    public class StatusMappingDto : IMapFrom<StatusMapping>
    {
        public int Id { get; set; }
        public int PspId { get; set; }
        public string PspName { get; set; } = string.Empty;
        public string PspStatus { get; set; } = string.Empty;
        public string CanonicalStatus { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Version { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class GetStatusMappingsQuery : IRequest<IEnumerable<StatusMappingDto>>
    {
    }

    public class GetStatusMappingsQueryHandler(
        IApplicationDbContext context,
        IMapper mapper) :
        IRequestHandler<GetStatusMappingsQuery, IEnumerable<StatusMappingDto>>
    {
        public async Task<IEnumerable<StatusMappingDto>> Handle(GetStatusMappingsQuery request, CancellationToken cancellationToken)
        {
            var mappings = await context.StatusMappings
                .Include(s => s.Psp)
                .OrderBy(s => s.Psp.Name)
                .ThenBy(s => s.PspStatus)
                .ToListAsync(cancellationToken);

            return mappings.Select(m => new StatusMappingDto
            {
                Id = m.Id,
                PspId = m.PspId,
                PspName = m.Psp?.Name ?? "Unknown",
                PspStatus = m.PspStatus,
                CanonicalStatus = m.CanonicalStatus,
                Description = m.Description,
                Version = m.Version,
                IsActive = m.IsActive
            });
        }
    }
}

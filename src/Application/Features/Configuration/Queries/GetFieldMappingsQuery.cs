using Application.Common.Mappings;
using Domain.Entities.Configuration;

namespace Application.Features.Configuration.Queries
{
    public class FieldMappingDto : IMapFrom<FieldMapping>
    {
        public int Id { get; set; }
        public int PspId { get; set; }
        public string PspName { get; set; } = string.Empty;
        public string SourceField { get; set; } = string.Empty;
        public string TargetField { get; set; } = string.Empty;
        public string? TransformExpression { get; set; }
        public string Version { get; set; } = string.Empty;
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsActive { get; set; }
    }

    public class GetFieldMappingsQuery : IRequest<IEnumerable<FieldMappingDto>>
    {
    }

    public class GetFieldMappingsQueryHandler(
        IApplicationDbContext context,
        IMapper mapper) :
        IRequestHandler<GetFieldMappingsQuery, IEnumerable<FieldMappingDto>>
    {
        public async Task<IEnumerable<FieldMappingDto>> Handle(GetFieldMappingsQuery request, CancellationToken cancellationToken)
        {
            var mappings = await context.FieldMappings
                .Include(f => f.Psp)
                .OrderBy(f => f.SourceField)
                .ToListAsync(cancellationToken);

            return mappings.Select(m => new FieldMappingDto
            {
                Id = m.Id,
                PspId = m.PspId,
                PspName = m.Psp?.Name ?? "Unknown",
                SourceField = m.SourceField,
                TargetField = m.TargetField,
                TransformExpression = m.TransformExpression,
                Version = m.Version,
                EffectiveFrom = m.EffectiveFrom,
                EffectiveTo = m.EffectiveTo,
                IsActive = m.IsActive
            });
        }
    }
}

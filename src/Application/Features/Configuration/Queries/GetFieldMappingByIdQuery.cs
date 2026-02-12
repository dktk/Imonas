namespace Application.Features.Configuration.Queries
{
    public class GetFieldMappingByIdQuery : IRequest<FieldMappingDto?>
    {
        public int Id { get; set; }
    }

    public class GetFieldMappingByIdQueryHandler(
        IApplicationDbContext context) :
        IRequestHandler<GetFieldMappingByIdQuery, FieldMappingDto?>
    {
        public async Task<FieldMappingDto?> Handle(GetFieldMappingByIdQuery request, CancellationToken cancellationToken)
        {
            var mapping = await context.FieldMappings
                .Include(f => f.Psp)
                .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

            if (mapping == null) return null;

            return new FieldMappingDto
            {
                Id = mapping.Id,
                PspId = mapping.PspId,
                PspName = mapping.Psp?.Name ?? "Unknown",
                SourceField = mapping.SourceField,
                TargetField = mapping.TargetField,
                TransformExpression = mapping.TransformExpression,
                Version = mapping.Version,
                EffectiveFrom = mapping.EffectiveFrom,
                EffectiveTo = mapping.EffectiveTo,
                IsActive = mapping.IsActive
            };
        }
    }
}

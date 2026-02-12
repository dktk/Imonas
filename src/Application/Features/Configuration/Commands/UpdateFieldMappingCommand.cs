using SG.Common;

namespace Application.Features.Configuration.Commands
{
    public class UpdateFieldMappingCommand : IRequest<Result<FieldMappingResultDto>>
    {
        public int Id { get; set; }
        public string SourceField { get; set; } = string.Empty;
        public string TargetField { get; set; } = string.Empty;
        public string? TransformExpression { get; set; }
        public string Version { get; set; } = string.Empty;
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateFieldMappingCommandHandler(
        IApplicationDbContext context) :
        IRequestHandler<UpdateFieldMappingCommand, Result<FieldMappingResultDto>>
    {
        public async Task<Result<FieldMappingResultDto>> Handle(UpdateFieldMappingCommand request, CancellationToken cancellationToken)
        {
            var mapping = await context.FieldMappings
                .Include(f => f.Psp)
                .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

            if (mapping == null)
                return Result<FieldMappingResultDto>.BuildFailure("Field mapping not found.");

            // Check for duplicate active mapping (excluding current)
            var duplicate = await context.FieldMappings
                .Where(m => m.Id != request.Id
                    && m.PspId == mapping.PspId
                    && m.SourceField == request.SourceField
                    && m.TargetField == request.TargetField
                    && m.IsActive
                    && request.IsActive)
                .AnyAsync(cancellationToken);

            if (duplicate)
                return Result<FieldMappingResultDto>.BuildFailure("An active mapping already exists for this source/target field combination.");

            mapping.SourceField = request.SourceField;
            mapping.TargetField = request.TargetField;
            mapping.TransformExpression = string.IsNullOrWhiteSpace(request.TransformExpression) ? null : request.TransformExpression;
            mapping.Version = request.Version;
            mapping.EffectiveFrom = request.EffectiveFrom;
            mapping.EffectiveTo = request.EffectiveTo;
            mapping.IsActive = request.IsActive;

            await context.SaveChangesAsync(cancellationToken);

            var dto = new FieldMappingResultDto
            {
                Id = mapping.Id,
                PspId = mapping.PspId,
                PspName = mapping.Psp?.Name ?? string.Empty,
                SourceField = mapping.SourceField,
                TargetField = mapping.TargetField
            };

            return Result<FieldMappingResultDto>.BuildSuccess(dto, $"Field mapping updated: {request.SourceField} → {request.TargetField}");
        }
    }
}

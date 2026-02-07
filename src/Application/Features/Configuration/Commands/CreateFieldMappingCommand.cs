using Domain.Entities.Configuration;
using SG.Common;

namespace Application.Features.Configuration.Commands
{
    public class CreateFieldMappingCommand : IRequest<Result<FieldMappingResultDto>>
    {
        public int PspId { get; set; }
        public string SourceField { get; set; } = string.Empty;
        public string TargetField { get; set; } = string.Empty;
        public string? TransformExpression { get; set; }
        public string Version { get; set; } = "1.0.0";
        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
        public DateTime? EffectiveTo { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class FieldMappingResultDto
    {
        public int Id { get; set; }
        public int PspId { get; set; }
        public string PspName { get; set; } = string.Empty;
        public string SourceField { get; set; } = string.Empty;
        public string TargetField { get; set; } = string.Empty;
    }

    public class CreateFieldMappingCommandHandler(
        IApplicationDbContext context) :
        IRequestHandler<CreateFieldMappingCommand, Result<FieldMappingResultDto>>
    {
        public async Task<Result<FieldMappingResultDto>> Handle(CreateFieldMappingCommand request, CancellationToken cancellationToken)
        {
            // Check if mapping already exists for this PSP and source/target combination
            var existingMapping = await context.FieldMappings
                .Where(m => m.PspId == request.PspId
                    && m.SourceField == request.SourceField
                    && m.TargetField == request.TargetField
                    && m.IsActive)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingMapping != null)
            {
                return Result<FieldMappingResultDto>.BuildFailure("An active mapping already exists for this source/target field combination.");
            }

            var psp = await context.Psps.FindAsync(new object[] { request.PspId }, cancellationToken);
            if (psp == null)
            {
                return Result<FieldMappingResultDto>.BuildFailure("PSP not found.");
            }

            var mapping = new FieldMapping
            {
                PspId = request.PspId,
                SourceField = request.SourceField,
                TargetField = request.TargetField,
                TransformExpression = request.TransformExpression,
                Version = request.Version,
                EffectiveFrom = request.EffectiveFrom,
                EffectiveTo = request.EffectiveTo,
                IsActive = request.IsActive
            };

            context.FieldMappings.Add(mapping);
            await context.SaveChangesAsync(cancellationToken);

            var dto = new FieldMappingResultDto
            {
                Id = mapping.Id,
                PspId = mapping.PspId,
                PspName = psp.Name,
                SourceField = mapping.SourceField,
                TargetField = mapping.TargetField
            };

            return Result<FieldMappingResultDto>.BuildSuccess(dto, $"Field mapping created: {request.SourceField} → {request.TargetField}");
        }
    }
}

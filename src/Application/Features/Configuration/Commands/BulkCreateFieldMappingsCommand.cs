using Domain.Entities.Configuration;
using SG.Common;

namespace Application.Features.Configuration.Commands
{
    public class FieldMappingInput
    {
        public string SourceField { get; set; } = string.Empty;
        public string TargetField { get; set; } = string.Empty;
        public string? TransformExpression { get; set; }
    }

    public class BulkCreateFieldMappingsCommand : IRequest<Result<int>>
    {
        public int PspId { get; set; }
        public List<FieldMappingInput> Mappings { get; set; } = new();
        public string Version { get; set; } = "1.0.0";
        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
        public DateTime? EffectiveTo { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class BulkCreateFieldMappingsCommandHandler(
        IApplicationDbContext context) :
        IRequestHandler<BulkCreateFieldMappingsCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(BulkCreateFieldMappingsCommand request, CancellationToken cancellationToken)
        {
            if (request.Mappings.Count == 0)
                return Result<int>.CreateFailure("No mappings provided.");

            var psp = await context.Psps.FindAsync(new object[] { request.PspId }, cancellationToken);
            if (psp == null)
                return Result<int>.CreateFailure("PSP not found.");

            // Check for duplicates in the request
            var duplicates = request.Mappings
                .GroupBy(m => new { m.SourceField, m.TargetField })
                .Where(g => g.Count() > 1)
                .Select(g => $"{g.Key.SourceField} -> {g.Key.TargetField}")
                .ToList();

            if (duplicates.Any())
                return Result<int>.CreateFailure($"Duplicate mappings found: {string.Join(", ", duplicates)}");

            // Check for existing active mappings
            var sourceFields = request.Mappings.Select(m => m.SourceField).ToList();
            var existingMappings = await context.FieldMappings
                .Where(m => m.PspId == request.PspId && m.IsActive && sourceFields.Contains(m.SourceField))
                .Select(m => m.SourceField)
                .ToListAsync(cancellationToken);

            if (existingMappings.Any())
                return Result<int>.CreateFailure($"Active mappings already exist for: {string.Join(", ", existingMappings)}");

            // Create all mappings
            var entities = request.Mappings.Select(m => new FieldMapping
            {
                PspId = request.PspId,
                SourceField = m.SourceField,
                TargetField = m.TargetField,
                TransformExpression = string.IsNullOrWhiteSpace(m.TransformExpression) ? null : m.TransformExpression,
                Version = request.Version,
                EffectiveFrom = request.EffectiveFrom,
                EffectiveTo = request.EffectiveTo,
                IsActive = request.IsActive
            }).ToList();

            context.FieldMappings.AddRange(entities);
            await context.SaveChangesAsync(cancellationToken);

            return Result<int>.CreateSuccess(entities.Count, $"Successfully created {entities.Count} field mapping(s).");
        }
    }
}

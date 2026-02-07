using Domain.Entities.Configuration;
using SG.Common;

namespace Application.Features.Configuration.Commands
{
    public class CreateStatusMappingCommand : IRequest<Result<StatusMappingDto>>
    {
        public int PspId { get; set; }
        public string PspStatus { get; set; } = string.Empty;
        public string CanonicalStatus { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Version { get; set; } = "1.0.0";
        public bool IsActive { get; set; } = true;
    }

    public class StatusMappingDto
    {
        public int Id { get; set; }
        public int PspId { get; set; }
        public string PspName { get; set; } = string.Empty;
        public string PspStatus { get; set; } = string.Empty;
        public string CanonicalStatus { get; set; } = string.Empty;
    }

    public class CreateStatusMappingCommandHandler(
        IApplicationDbContext context) :
        IRequestHandler<CreateStatusMappingCommand, Result<StatusMappingDto>>
    {
        public async Task<Result<StatusMappingDto>> Handle(CreateStatusMappingCommand request, CancellationToken cancellationToken)
        {
            // Check if mapping already exists for this PSP and status
            var existingMapping = await context.StatusMappings
                .Where(m => m.PspId == request.PspId && m.PspStatus == request.PspStatus && m.IsActive)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingMapping != null)
            {
                return Result<StatusMappingDto>.BuildFailure("An active mapping already exists for this PSP status.");
            }

            var psp = await context.Psps.FindAsync(new object[] { request.PspId }, cancellationToken);
            if (psp == null)
            {
                return Result<StatusMappingDto>.BuildFailure("PSP not found.");
            }

            var mapping = new StatusMapping
            {
                PspId = request.PspId,
                PspStatus = request.PspStatus,
                CanonicalStatus = request.CanonicalStatus,
                Description = request.Description,
                Version = request.Version,
                IsActive = request.IsActive
            };

            context.StatusMappings.Add(mapping);
            await context.SaveChangesAsync(cancellationToken);

            var dto = new StatusMappingDto
            {
                Id = mapping.Id,
                PspId = mapping.PspId,
                PspName = psp.Name,
                PspStatus = mapping.PspStatus,
                CanonicalStatus = mapping.CanonicalStatus
            };

            return Result<StatusMappingDto>.BuildSuccess(dto, $"Status mapping created: {request.PspStatus} → {request.CanonicalStatus}");
        }
    }
}

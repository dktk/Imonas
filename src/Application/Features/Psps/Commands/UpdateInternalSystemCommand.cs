using SG.Common;

namespace Application.Features.Psps.Commands
{
    public class UpdateInternalSystemCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class UpdateInternalSystemCommandHandler(
        IApplicationDbContext context) : IRequestHandler<UpdateInternalSystemCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(UpdateInternalSystemCommand request, CancellationToken cancellationToken)
        {
            var entity = await context.InternalSystems
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (entity == null)
            {
                return Result<bool>.BuildFailure($"Internal system with ID {request.Id} not found.");
            }

            // Check for duplicate name (excluding current record)
            var duplicateExists = await context.InternalSystems
                .AnyAsync(s => s.Id != request.Id && s.Name.ToLower() == request.Name.ToLower(), cancellationToken);

            if (duplicateExists)
            {
                return Result<bool>.BuildFailure($"An internal system with name '{request.Name}' already exists.");
            }

            entity.Name = request.Name;
            entity.IsActive = request.IsActive;

            await context.SaveChangesAsync(cancellationToken);

            return Result<bool>.BuildSuccess(true);
        }
    }
}

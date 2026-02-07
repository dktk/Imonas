using SG.Common;

namespace Application.Features.Psps.Commands
{
    public class DeleteInternalSystemCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }

    public class DeleteInternalSystemCommandHandler(
        IApplicationDbContext context) : IRequestHandler<DeleteInternalSystemCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteInternalSystemCommand request, CancellationToken cancellationToken)
        {
            var entity = await context.InternalSystems
                .Include(s => s.Psps)
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (entity == null)
            {
                return Result<bool>.BuildFailure($"Internal system with ID {request.Id} not found.");
            }

            // Check if there are associated PSPs
            if (entity.Psps != null && entity.Psps.Any())
            {
                return Result<bool>.BuildFailure($"Cannot delete internal system '{entity.Name}' because it has {entity.Psps.Count} associated PSP(s). Please reassign or remove the PSPs first.");
            }

            context.InternalSystems.Remove(entity);
            await context.SaveChangesAsync(cancellationToken);

            return Result<bool>.BuildSuccess(true);
        }
    }
}

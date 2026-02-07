using SG.Common;

namespace Application.Features.Psps.Commands
{
    public class DeletePspCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }

    public class DeletePspCommandHandler(
        IApplicationDbContext context) : IRequestHandler<DeletePspCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeletePspCommand request, CancellationToken cancellationToken)
        {
            var entity = await context.Psps
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (entity == null)
            {
                return Result<bool>.BuildFailure($"PSP with ID {request.Id} not found.");
            }

            // Check if there are related records that would prevent deletion
            var hasExternalPayments = await context.ExternalPayments
                .AnyAsync(ep => ep.PspId == request.Id, cancellationToken);

            if (hasExternalPayments)
            {
                return Result<bool>.BuildFailure($"Cannot delete PSP '{entity.Name}' because it has associated transactions. Consider deactivating it instead.");
            }

            var hasFieldMappings = await context.FieldMappings
                .AnyAsync(fm => fm.PspId == request.Id, cancellationToken);

            if (hasFieldMappings)
            {
                return Result<bool>.BuildFailure($"Cannot delete PSP '{entity.Name}' because it has associated field mappings. Please remove them first.");
            }

            context.Psps.Remove(entity);
            await context.SaveChangesAsync(cancellationToken);

            return Result<bool>.BuildSuccess(true);
        }
    }
}

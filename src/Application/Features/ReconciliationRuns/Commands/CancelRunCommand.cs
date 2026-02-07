using SG.Common;

namespace Application.Features.ReconciliationRuns.Commands
{
    public class CancelRunCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }

    public class CancelRunCommandHandler(
        IApplicationDbContext context,
        IDateTime dateTime,
        ICurrentUserService currentUserService) :
        IRequestHandler<CancelRunCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(CancelRunCommand request, CancellationToken cancellationToken)
        {
            var run = await context.ReconciliationRuns
                .FindAsync(new object[] { request.Id }, cancellationToken);

            if (run == null)
            {
                return Result<bool>.BuildFailure($"Reconciliation run {request.Id} not found.");
            }

            if (run.Status != RunStatus.Pending && run.Status != RunStatus.Running)
            {
                return Result<bool>.BuildFailure($"Cannot cancel a run that is in '{run.Status}' status. Only Pending or Running runs can be cancelled.");
            }

            run.Status = RunStatus.Cancelled;
            run.CompletedAt = dateTime.Now;
            run.ErrorMessage = $"Cancelled by user {currentUserService.UserId ?? "Unknown"} at {dateTime.Now:yyyy-MM-dd HH:mm:ss} UTC";

            await context.SaveChangesAsync(cancellationToken);

            return Result<bool>.BuildSuccess(true, $"Reconciliation run '{run.RunName}' has been cancelled.");
        }
    }
}

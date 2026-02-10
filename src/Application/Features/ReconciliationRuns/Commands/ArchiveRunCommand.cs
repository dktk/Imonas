using SG.Common;

namespace Application.Features.ReconciliationRuns.Commands
{
    public class ArchiveRunCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public class ArchiveRunCommandHandler(
        IApplicationDbContext context,
        IDateTime dateTime,
        ICurrentUserService currentUserService) :
        IRequestHandler<ArchiveRunCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(ArchiveRunCommand request, CancellationToken cancellationToken)
        {
            var run = await context.ReconciliationRuns
                .FindAsync(new object[] { request.Id }, cancellationToken);

            if (run == null)
            {
                return Result<bool>.BuildFailure($"Reconciliation run {request.Id} not found.");
            }

            if (run.IsArchived)
            {
                return Result<bool>.BuildFailure($"Reconciliation run '{run.RunName}' is already archived.");
            }

            if (run.Status == RunStatus.Running)
            {
                return Result<bool>.BuildFailure($"Cannot archive a run that is currently running. Cancel it first.");
            }

            run.IsArchived = true;
            run.ArchivedAt = dateTime.Now;
            run.ArchivedBy = currentUserService.UserId ?? "Unknown";
            run.ArchiveComment = request.Comment;

            await context.SaveChangesAsync(cancellationToken);

            return Result<bool>.BuildSuccess(true, $"Reconciliation run '{run.RunName}' has been archived.");
        }
    }
}

using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using SG.Common;

namespace Application.Features.Schedules.Commands
{
    public class DeleteScheduleCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }

    public class DeleteScheduleCommandHandler(
        IApplicationDbContext context) :
        IRequestHandler<DeleteScheduleCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(
            DeleteScheduleCommand request,
            CancellationToken cancellationToken)
        {
            var schedule = await context.ReconciliationSchedules
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (schedule == null)
            {
                return Result<bool>.BuildFailure("Schedule not found.");
            }

            var scheduleName = schedule.Name;
            context.ReconciliationSchedules.Remove(schedule);
            await context.SaveChangesAsync(cancellationToken);

            return Result<bool>.BuildSuccess(true, $"Schedule '{scheduleName}' deleted successfully.");
        }
    }
}

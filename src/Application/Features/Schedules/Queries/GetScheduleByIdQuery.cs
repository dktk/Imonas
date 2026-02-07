using Application.Common.Interfaces.Identity;
using Application.Features.Schedules.DTOs;

namespace Application.Features.Schedules.Queries
{
    public class GetScheduleByIdQuery : IRequest<ScheduleDetailsDto?>
    {
        public int Id { get; set; }
    }

    public class GetScheduleByIdQueryHandler(
        IApplicationDbContext context,
        IUserService userService) :
        IRequestHandler<GetScheduleByIdQuery, ScheduleDetailsDto?>
    {
        public async Task<ScheduleDetailsDto?> Handle(
            GetScheduleByIdQuery request,
            CancellationToken cancellationToken)
        {
            var schedule = await context.ReconciliationSchedules
                .Include(s => s.Psp)
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (schedule == null)
            {
                return null;
            }

            string? assignedToName = null;
            if (!string.IsNullOrEmpty(schedule.AssignedTo))
            {
                var user = await userService.GetUserByIdAsync(schedule.AssignedTo);
                assignedToName = user?.DisplayName ?? user?.Email;
            }

            // Get run statistics for this PSP
            var runs = await context.ReconciliationRuns
                .Where(r => r.UserId == schedule.AssignedTo ||
                           r.RunName.Contains(schedule.Psp.Name))
                .ToListAsync(cancellationToken);

            var totalRuns = runs.Count;
            var successfulRuns = runs.Count(r => r.Status == RunStatus.Completed);
            var failedRuns = runs.Count(r => r.Status == RunStatus.Failed);

            return new ScheduleDetailsDto
            {
                Id = schedule.Id,
                Name = schedule.Name,
                PspId = schedule.PspId,
                PspName = schedule.Psp.Name,
                PspCode = schedule.Psp.Code,
                RecurrenceType = schedule.RecurrenceType,
                RecurrenceInterval = schedule.RecurrenceInterval,
                DayOfWeek = schedule.DayOfWeek,
                DayOfMonth = schedule.DayOfMonth,
                ScheduledTime = schedule.ScheduledTime,
                StartDate = schedule.StartDate,
                EndDate = schedule.EndDate,
                NextRunDate = schedule.NextRunDate,
                LastRunDate = schedule.LastRunDate,
                IsActive = schedule.IsActive,
                AssignedTo = schedule.AssignedTo,
                AssignedToName = assignedToName,
                Description = schedule.Description,
                Created = schedule.Created,
                CreatedBy = schedule.UserId,
                TotalRunsExecuted = totalRuns,
                SuccessfulRuns = successfulRuns,
                FailedRuns = failedRuns
            };
        }
    }
}

using SG.Common;

namespace Application.Features.Schedules.Commands
{
    public class ToggleScheduleStatusCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }

    public class ToggleScheduleStatusCommandHandler(
        IApplicationDbContext context,
        IDateTime dateTime) :
        IRequestHandler<ToggleScheduleStatusCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(
            ToggleScheduleStatusCommand request,
            CancellationToken cancellationToken)
        {
            var schedule = await context.ReconciliationSchedules
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (schedule == null)
            {
                return Result<bool>.BuildFailure("Schedule not found.");
            }

            schedule.IsActive = !schedule.IsActive;

            // Recalculate next run date
            if (schedule.IsActive)
            {
                schedule.NextRunDate = CalculateNextRunDate(schedule, dateTime.Now);
            }
            else
            {
                schedule.NextRunDate = null;
            }

            await context.SaveChangesAsync(cancellationToken);

            var status = schedule.IsActive ? "activated" : "deactivated";
            return Result<bool>.BuildSuccess(true, $"Schedule '{schedule.Name}' has been {status}.");
        }

        private static DateTime? CalculateNextRunDate(Domain.Entities.Configuration.ReconciliationSchedule schedule, DateTime now)
        {
            var startDate = schedule.StartDate ?? now.Date;
            if (startDate < now.Date)
            {
                startDate = now.Date;
            }

            var candidate = startDate.Add(schedule.ScheduledTime);

            if (candidate <= now)
            {
                candidate = candidate.AddDays(1);
            }

            switch (schedule.RecurrenceType)
            {
                case RecurrenceType.Daily:
                    return candidate;

                case RecurrenceType.Weekly:
                case RecurrenceType.BiWeekly:
                    if (schedule.DayOfWeek.HasValue)
                    {
                        while (candidate.DayOfWeek != schedule.DayOfWeek.Value)
                        {
                            candidate = candidate.AddDays(1);
                        }
                    }
                    return candidate;

                case RecurrenceType.Monthly:
                    if (schedule.DayOfMonth.HasValue)
                    {
                        var targetDay = Math.Min(schedule.DayOfMonth.Value, DateTime.DaysInMonth(candidate.Year, candidate.Month));
                        candidate = new DateTime(candidate.Year, candidate.Month, targetDay).Add(schedule.ScheduledTime);
                        if (candidate <= now)
                        {
                            candidate = candidate.AddMonths(1);
                            targetDay = Math.Min(schedule.DayOfMonth.Value, DateTime.DaysInMonth(candidate.Year, candidate.Month));
                            candidate = new DateTime(candidate.Year, candidate.Month, targetDay).Add(schedule.ScheduledTime);
                        }
                    }
                    return candidate;

                case RecurrenceType.Quarterly:
                    if (schedule.DayOfMonth.HasValue)
                    {
                        var quarterMonths = new[] { 1, 4, 7, 10 };
                        var nextQuarterMonth = quarterMonths.FirstOrDefault(m => m >= candidate.Month);
                        if (nextQuarterMonth == 0)
                        {
                            nextQuarterMonth = 1;
                            candidate = candidate.AddYears(1);
                        }
                        var targetDay = Math.Min(schedule.DayOfMonth.Value, DateTime.DaysInMonth(candidate.Year, nextQuarterMonth));
                        candidate = new DateTime(candidate.Year, nextQuarterMonth, targetDay).Add(schedule.ScheduledTime);
                    }
                    return candidate;

                default:
                    return candidate;
            }
        }
    }
}

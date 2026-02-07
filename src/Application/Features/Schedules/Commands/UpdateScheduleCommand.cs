using SG.Common;

namespace Application.Features.Schedules.Commands
{
    public class UpdateScheduleCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public RecurrenceType RecurrenceType { get; set; }
        public int RecurrenceInterval { get; set; } = 1;
        public DayOfWeek? DayOfWeek { get; set; }
        public int? DayOfMonth { get; set; }
        public TimeSpan ScheduledTime { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public string? AssignedTo { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateScheduleCommandHandler(
        IApplicationDbContext context,
        IDateTime dateTime) :
        IRequestHandler<UpdateScheduleCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(
            UpdateScheduleCommand request,
            CancellationToken cancellationToken)
        {
            var schedule = await context.ReconciliationSchedules
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (schedule == null)
            {
                return Result<bool>.BuildFailure("Schedule not found.");
            }

            // Validate schedule name is unique for this PSP (excluding current)
            var existingSchedule = await context.ReconciliationSchedules
                .FirstOrDefaultAsync(s => s.PspId == schedule.PspId &&
                                         s.Name == request.Name &&
                                         s.Id != request.Id, cancellationToken);

            if (existingSchedule != null)
            {
                return Result<bool>.BuildFailure($"A schedule with name '{request.Name}' already exists for this PSP.");
            }

            schedule.Name = request.Name;
            schedule.RecurrenceType = request.RecurrenceType;
            schedule.RecurrenceInterval = request.RecurrenceInterval;
            schedule.DayOfWeek = request.DayOfWeek;
            schedule.DayOfMonth = request.DayOfMonth;
            schedule.ScheduledTime = request.ScheduledTime;
            schedule.StartDate = request.StartDate;
            schedule.EndDate = request.EndDate;
            schedule.IsActive = request.IsActive;
            schedule.AssignedTo = request.AssignedTo;
            schedule.Description = request.Description;

            // Recalculate next run date
            schedule.NextRunDate = CalculateNextRunDate(schedule, dateTime.Now);

            await context.SaveChangesAsync(cancellationToken);

            return Result<bool>.BuildSuccess(true, $"Schedule '{request.Name}' updated successfully.");
        }

        private static DateTime? CalculateNextRunDate(Domain.Entities.Configuration.ReconciliationSchedule schedule, DateTime now)
        {
            if (!schedule.IsActive)
            {
                return null;
            }

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
                    if (schedule.DayOfWeek.HasValue)
                    {
                        while (candidate.DayOfWeek != schedule.DayOfWeek.Value)
                        {
                            candidate = candidate.AddDays(1);
                        }
                    }
                    return candidate;

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
                        if (candidate <= now)
                        {
                            var currentIndex = Array.IndexOf(quarterMonths, nextQuarterMonth);
                            if (currentIndex < quarterMonths.Length - 1)
                            {
                                nextQuarterMonth = quarterMonths[currentIndex + 1];
                            }
                            else
                            {
                                nextQuarterMonth = 1;
                                candidate = candidate.AddYears(1);
                            }
                            targetDay = Math.Min(schedule.DayOfMonth.Value, DateTime.DaysInMonth(candidate.Year, nextQuarterMonth));
                            candidate = new DateTime(candidate.Year, nextQuarterMonth, targetDay).Add(schedule.ScheduledTime);
                        }
                    }
                    return candidate;

                default:
                    return candidate;
            }
        }
    }
}

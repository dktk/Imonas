using Application.Features.Schedules.DTOs;

using Domain.Entities.Configuration;

using SG.Common;

namespace Application.Features.Schedules.Commands
{
    public class CreateScheduleCommand : IRequest<Result<ScheduleDto>>
    {
        public string Name { get; set; } = string.Empty;
        public int PspId { get; set; }
        public RecurrenceType RecurrenceType { get; set; }
        public int RecurrenceInterval { get; set; } = 1;
        public DayOfWeek? DayOfWeek { get; set; }
        public int? DayOfMonth { get; set; }
        public TimeSpan ScheduledTime { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string? AssignedTo { get; set; }
        public string? Description { get; set; }
    }

    public class CreateScheduleCommandHandler(
        IApplicationDbContext context,
        IDateTime dateTime) :
        IRequestHandler<CreateScheduleCommand, Result<ScheduleDto>>
    {
        public async Task<Result<ScheduleDto>> Handle(
            CreateScheduleCommand request,
            CancellationToken cancellationToken)
        {
            // Validate PSP exists
            var psp = await context.Psps
                .FirstOrDefaultAsync(p => p.Id == request.PspId, cancellationToken);

            if (psp == null)
            {
                return Result<ScheduleDto>.BuildFailure("PSP not found.");
            }

            // Validate schedule name is unique for this PSP
            var existingSchedule = await context.ReconciliationSchedules
                .FirstOrDefaultAsync(s => s.PspId == request.PspId &&
                                         s.Name == request.Name, cancellationToken);

            if (existingSchedule != null)
            {
                return Result<ScheduleDto>.BuildFailure($"A schedule with name '{request.Name}' already exists for this PSP.");
            }

            // Calculate next run date
            var nextRunDate = CalculateNextRunDate(request, dateTime.Now);

            var schedule = new ReconciliationSchedule
            {
                Name = request.Name,
                PspId = request.PspId,
                RecurrenceType = request.RecurrenceType,
                RecurrenceInterval = request.RecurrenceInterval,
                DayOfWeek = request.DayOfWeek,
                DayOfMonth = request.DayOfMonth,
                ScheduledTime = request.ScheduledTime,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                NextRunDate = nextRunDate,
                IsActive = request.IsActive,
                AssignedTo = request.AssignedTo,
                Description = request.Description
            };

            context.ReconciliationSchedules.Add(schedule);
            await context.SaveChangesAsync(cancellationToken);

            var dto = new ScheduleDto
            {
                Id = schedule.Id,
                Name = schedule.Name,
                PspId = schedule.PspId,
                PspName = psp.Name,
                RecurrenceType = schedule.RecurrenceType,
                RecurrenceInterval = schedule.RecurrenceInterval,
                DayOfWeek = schedule.DayOfWeek,
                DayOfMonth = schedule.DayOfMonth,
                ScheduledTime = schedule.ScheduledTime,
                StartDate = schedule.StartDate,
                EndDate = schedule.EndDate,
                NextRunDate = schedule.NextRunDate,
                IsActive = schedule.IsActive,
                AssignedTo = schedule.AssignedTo,
                Description = schedule.Description,
                Created = schedule.Created
            };

            return Result<ScheduleDto>.BuildSuccess(dto, $"Schedule '{request.Name}' created successfully.");
        }

        private static DateTime? CalculateNextRunDate(CreateScheduleCommand request, DateTime now)
        {
            if (!request.IsActive)
            {
                return null;
            }

            var startDate = request.StartDate ?? now.Date;
            if (startDate < now.Date)
            {
                startDate = now.Date;
            }

            var candidate = startDate.Add(request.ScheduledTime);

            // If today's scheduled time has passed, start from tomorrow
            if (candidate <= now)
            {
                candidate = candidate.AddDays(1);
            }

            switch (request.RecurrenceType)
            {
                case RecurrenceType.Daily:
                    return candidate;

                case RecurrenceType.Weekly:
                    if (request.DayOfWeek.HasValue)
                    {
                        while (candidate.DayOfWeek != request.DayOfWeek.Value)
                        {
                            candidate = candidate.AddDays(1);
                        }
                    }
                    return candidate;

                case RecurrenceType.BiWeekly:
                    if (request.DayOfWeek.HasValue)
                    {
                        while (candidate.DayOfWeek != request.DayOfWeek.Value)
                        {
                            candidate = candidate.AddDays(1);
                        }
                    }
                    return candidate;

                case RecurrenceType.Monthly:
                    if (request.DayOfMonth.HasValue)
                    {
                        var targetDay = Math.Min(request.DayOfMonth.Value, DateTime.DaysInMonth(candidate.Year, candidate.Month));
                        candidate = new DateTime(candidate.Year, candidate.Month, targetDay).Add(request.ScheduledTime);
                        if (candidate <= now)
                        {
                            candidate = candidate.AddMonths(1);
                            targetDay = Math.Min(request.DayOfMonth.Value, DateTime.DaysInMonth(candidate.Year, candidate.Month));
                            candidate = new DateTime(candidate.Year, candidate.Month, targetDay).Add(request.ScheduledTime);
                        }
                    }
                    return candidate;

                case RecurrenceType.Quarterly:
                    if (request.DayOfMonth.HasValue)
                    {
                        var quarterMonths = new[] { 1, 4, 7, 10 };
                        var nextQuarterMonth = quarterMonths.FirstOrDefault(m => m >= candidate.Month);
                        if (nextQuarterMonth == 0)
                        {
                            nextQuarterMonth = 1;
                            candidate = candidate.AddYears(1);
                        }
                        var targetDay = Math.Min(request.DayOfMonth.Value, DateTime.DaysInMonth(candidate.Year, nextQuarterMonth));
                        candidate = new DateTime(candidate.Year, nextQuarterMonth, targetDay).Add(request.ScheduledTime);
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
                            targetDay = Math.Min(request.DayOfMonth.Value, DateTime.DaysInMonth(candidate.Year, nextQuarterMonth));
                            candidate = new DateTime(candidate.Year, nextQuarterMonth, targetDay).Add(request.ScheduledTime);
                        }
                    }
                    return candidate;

                default:
                    return candidate;
            }
        }
    }
}

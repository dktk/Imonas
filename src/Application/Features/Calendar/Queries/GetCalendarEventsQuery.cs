using Application.Common.Interfaces.Identity;
using Application.Features.Calendar.DTOs;

using Domain.Constants;

namespace Application.Features.Calendar.Queries
{
    public class GetCalendarEventsQuery : IRequest<IEnumerable<CalendarEventDto>>
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IncludeCases { get; set; } = true;
        public bool IncludeSchedules { get; set; } = true;
        public bool IncludeRuns { get; set; } = true;
        public string? AssignedTo { get; set; }
        public int? PspId { get; set; }
    }

    public class GetCalendarEventsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUserService userService) :
        IRequestHandler<GetCalendarEventsQuery, IEnumerable<CalendarEventDto>>
    {
        public async Task<IEnumerable<CalendarEventDto>> Handle(
            GetCalendarEventsQuery request,
            CancellationToken cancellationToken)
        {
            var events = new List<CalendarEventDto>();
            var currentUserId = currentUserService.UserId;

            // Check if user is admin
            var isAdmin = !string.IsNullOrEmpty(currentUserId) &&
                (await userService.IsInRoleAsync(currentUserId, Roles.Admin) ||
                 await userService.IsInRoleAsync(currentUserId, Roles.SystemAdmin));

            // Get case due dates
            if (request.IncludeCases)
            {
                var casesQuery = context.ExceptionCases
                    .Where(c => c.DueDate.HasValue &&
                               c.DueDate.Value >= request.StartDate &&
                               c.DueDate.Value <= request.EndDate &&
                               c.Status != CaseStatus.Closed);

                // Filter by user if not admin
                if (!isAdmin && !string.IsNullOrEmpty(currentUserId))
                {
                    casesQuery = casesQuery.Where(c => c.AssignedToId == currentUserId);
                }
                else if (!string.IsNullOrEmpty(request.AssignedTo))
                {
                    casesQuery = casesQuery.Where(c => c.AssignedToId == request.AssignedTo);
                }

                var cases = await casesQuery.ToListAsync(cancellationToken);

                foreach (var caseItem in cases)
                {
                    var color = caseItem.Severity switch
                    {
                        CaseSeverity.Critical => "#dc3545", // Red
                        CaseSeverity.High => "#fd7e14", // Orange
                        CaseSeverity.Medium => "#ffc107", // Yellow
                        CaseSeverity.Low => "#28a745", // Green
                        _ => "#6c757d" // Gray
                    };

                    events.Add(new CalendarEventDto
                    {
                        Id = $"case-{caseItem.Id}",
                        Title = $"[Case] {caseItem.CaseNumber}: {caseItem.Title}",
                        Start = caseItem.DueDate!.Value,
                        Description = caseItem.Description,
                        EventType = CalendarEventType.CaseDueDate,
                        Color = color,
                        Url = $"/Cases/Details/{caseItem.Id}",
                        AllDay = true,
                        AssignedTo = caseItem.AssignedToId,
                        CaseId = caseItem.Id,
                        CaseNumber = caseItem.CaseNumber,
                        CaseStatus = caseItem.Status,
                        CaseSeverity = caseItem.Severity
                    });
                }
            }

            // Get reconciliation schedules
            if (request.IncludeSchedules)
            {
                var schedulesQuery = context.ReconciliationSchedules
                    .Include(s => s.Psp)
                    .Where(s => s.IsActive);

                if (request.PspId.HasValue)
                {
                    schedulesQuery = schedulesQuery.Where(s => s.PspId == request.PspId.Value);
                }

                // Filter by user if not admin
                if (!isAdmin && !string.IsNullOrEmpty(currentUserId))
                {
                    schedulesQuery = schedulesQuery.Where(s => s.AssignedTo == currentUserId);
                }
                else if (!string.IsNullOrEmpty(request.AssignedTo))
                {
                    schedulesQuery = schedulesQuery.Where(s => s.AssignedTo == request.AssignedTo);
                }

                var schedules = await schedulesQuery.ToListAsync(cancellationToken);

                foreach (var schedule in schedules)
                {
                    // Generate occurrences within the date range
                    var occurrences = GenerateScheduleOccurrences(schedule, request.StartDate, request.EndDate);

                    foreach (var occurrence in occurrences)
                    {
                        events.Add(new CalendarEventDto
                        {
                            Id = $"schedule-{schedule.Id}-{occurrence:yyyyMMdd}",
                            Title = $"[Recon] {schedule.Psp.Name}: {schedule.Name}",
                            Start = occurrence,
                            Description = schedule.Description ?? $"Scheduled reconciliation for {schedule.Psp.Name}",
                            EventType = CalendarEventType.ReconciliationSchedule,
                            Color = "#007bff", // Blue
                            AllDay = false,
                            AssignedTo = schedule.AssignedTo,
                            ScheduleId = schedule.Id,
                            PspId = schedule.PspId,
                            PspName = schedule.Psp.Name,
                            RecurrenceType = schedule.RecurrenceType
                        });
                    }
                }
            }

            // Get past reconciliation runs
            if (request.IncludeRuns)
            {
                var runsQuery = context.ReconciliationRuns
                    .Where(r => r.StartedAt >= request.StartDate &&
                               r.StartedAt <= request.EndDate);

                var runs = await runsQuery.ToListAsync(cancellationToken);

                foreach (var run in runs)
                {
                    var color = run.Status switch
                    {
                        RunStatus.Completed => "#28a745", // Green
                        RunStatus.Failed => "#dc3545", // Red
                        RunStatus.Running => "#17a2b8", // Cyan
                        RunStatus.Cancelled => "#6c757d", // Gray
                        _ => "#ffc107" // Yellow for Pending/PartialSuccess
                    };

                    events.Add(new CalendarEventDto
                    {
                        Id = $"run-{run.Id}",
                        Title = $"[Run] {run.RunName}",
                        Start = run.StartedAt,
                        End = run.CompletedAt,
                        Description = $"Status: {run.Status}, Matched: {run.ExternalMatchedRecordsCount}, Unmatched: {run.ExternalUnmatchedRecordsCount}",
                        EventType = CalendarEventType.ReconciliationRun,
                        Color = color,
                        Url = $"/Runs/Details/{run.Id}",
                        AllDay = false
                    });
                }
            }

            return events.OrderBy(e => e.Start);
        }

        private static IEnumerable<DateTime> GenerateScheduleOccurrences(
            Domain.Entities.Configuration.ReconciliationSchedule schedule,
            DateTime rangeStart,
            DateTime rangeEnd)
        {
            var occurrences = new List<DateTime>();
            var startDate = schedule.StartDate ?? schedule.Created;
            var endDate = schedule.EndDate ?? rangeEnd;

            // Ensure we don't go past the schedule end date
            if (endDate > rangeEnd)
                endDate = rangeEnd;

            // Start from the later of schedule start or range start
            var current = startDate > rangeStart ? startDate : rangeStart;

            // Adjust to the scheduled time
            current = current.Date.Add(schedule.ScheduledTime);

            while (current <= endDate)
            {
                bool shouldAdd = false;

                switch (schedule.RecurrenceType)
                {
                    case RecurrenceType.Daily:
                        shouldAdd = true;
                        break;

                    case RecurrenceType.Weekly:
                        if (schedule.DayOfWeek.HasValue)
                        {
                            shouldAdd = current.DayOfWeek == schedule.DayOfWeek.Value;
                        }
                        break;

                    case RecurrenceType.BiWeekly:
                        if (schedule.DayOfWeek.HasValue)
                        {
                            var weeksSinceStart = (current - startDate).Days / 7;
                            shouldAdd = current.DayOfWeek == schedule.DayOfWeek.Value &&
                                       weeksSinceStart % 2 == 0;
                        }
                        break;

                    case RecurrenceType.Monthly:
                        if (schedule.DayOfMonth.HasValue)
                        {
                            var targetDay = Math.Min(schedule.DayOfMonth.Value, DateTime.DaysInMonth(current.Year, current.Month));
                            shouldAdd = current.Day == targetDay;
                        }
                        break;

                    case RecurrenceType.Quarterly:
                        if (schedule.DayOfMonth.HasValue)
                        {
                            var quarterMonths = new[] { 1, 4, 7, 10 };
                            var targetDay = Math.Min(schedule.DayOfMonth.Value, DateTime.DaysInMonth(current.Year, current.Month));
                            shouldAdd = quarterMonths.Contains(current.Month) && current.Day == targetDay;
                        }
                        break;
                }

                if (shouldAdd && current >= rangeStart)
                {
                    occurrences.Add(current);
                }

                // Move to next day
                current = current.AddDays(schedule.RecurrenceInterval);
            }

            return occurrences;
        }
    }
}

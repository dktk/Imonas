using Application.Features.Schedules.DTOs;

namespace Application.Features.Schedules.Queries
{
    public class GetScheduleStatsQuery : IRequest<ScheduleStatsDto>
    {
    }

    public class GetScheduleStatsQueryHandler(
        IApplicationDbContext context) :
        IRequestHandler<GetScheduleStatsQuery, ScheduleStatsDto>
    {
        public async Task<ScheduleStatsDto> Handle(
            GetScheduleStatsQuery request,
            CancellationToken cancellationToken)
        {
            var schedules = await context.ReconciliationSchedules
                .ToListAsync(cancellationToken);

            var today = DateTime.UtcNow.Date;
            var endOfWeek = today.AddDays(7);

            return new ScheduleStatsDto
            {
                TotalSchedules = schedules.Count,
                ActiveSchedules = schedules.Count(s => s.IsActive),
                InactiveSchedules = schedules.Count(s => !s.IsActive),
                SchedulesDueToday = schedules.Count(s => s.IsActive &&
                    s.NextRunDate.HasValue &&
                    s.NextRunDate.Value.Date == today),
                SchedulesDueThisWeek = schedules.Count(s => s.IsActive &&
                    s.NextRunDate.HasValue &&
                    s.NextRunDate.Value.Date >= today &&
                    s.NextRunDate.Value.Date <= endOfWeek)
            };
        }
    }
}

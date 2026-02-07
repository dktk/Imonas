using Application.Common.Interfaces.Identity;
using Application.Features.Schedules.DTOs;

namespace Application.Features.Schedules.Queries
{
    public class GetSchedulesQuery : IRequest<IEnumerable<ScheduleDto>>
    {
        public int? PspId { get; set; }
        public bool? IsActive { get; set; }
        public string? AssignedTo { get; set; }
    }

    public class GetSchedulesQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUserService userService) :
        IRequestHandler<GetSchedulesQuery, IEnumerable<ScheduleDto>>
    {
        public async Task<IEnumerable<ScheduleDto>> Handle(
            GetSchedulesQuery request,
            CancellationToken cancellationToken)
        {
            var currentUserId = currentUserService.UserId;

            var query = context.ReconciliationSchedules
                .Include(s => s.Psp)
                .AsQueryable();

            if (request.PspId.HasValue)
            {
                query = query.Where(s => s.PspId == request.PspId.Value);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(s => s.IsActive == request.IsActive.Value);
            }

            if (!string.IsNullOrEmpty(request.AssignedTo))
            {
                query = query.Where(s => s.AssignedTo == request.AssignedTo);
            }

            var schedules = await query
                .OrderBy(s => s.Psp.Name)
                .ThenBy(s => s.Name)
                .ToListAsync(cancellationToken);

            var result = new List<ScheduleDto>();

            foreach (var schedule in schedules)
            {
                string? assignedToName = null;
                if (!string.IsNullOrEmpty(schedule.AssignedTo))
                {
                    var user = await userService.GetUserByIdAsync(schedule.AssignedTo);
                    assignedToName = user?.DisplayName ?? user?.Email;
                }

                result.Add(new ScheduleDto
                {
                    Id = schedule.Id,
                    Name = schedule.Name,
                    PspId = schedule.PspId,
                    PspName = schedule.Psp.Name,
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
                    CreatedBy = schedule.UserId
                });
            }

            return result;
        }
    }
}

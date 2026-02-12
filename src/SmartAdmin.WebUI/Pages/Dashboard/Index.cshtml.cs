using Application.Features.Cases.Queries;
using Application.Features.ReconciliationRuns.DTOs;
using Application.Features.ReconciliationRuns.Queries;
using Application.Features.Schedules.DTOs;
using Application.Features.Schedules.Queries;
using Application.Features.Users.DTOs;
using Application.Features.Users.Queries;

namespace SmartAdmin.WebUI.Pages.Dashboard
{
    [Authorize]
    public class IndexModel(
        IStringLocalizer<IndexModel> localizer,
        ISender mediator) : PageModel
    {
        public RunStatsDto RunStats { get; set; } = new();
        public CaseStatsDto CaseStats { get; set; } = new();
        public ScheduleStatsDto ScheduleStats { get; set; } = new();
        public UserStatsDto UserStats { get; set; } = new();
        public IEnumerable<ReconciliationRunDto> RecentRuns { get; set; } = new List<ReconciliationRunDto>();

        public async Task OnGetAsync()
        {
            // todo: these queries can go in parallel

            RunStats = await mediator.Send(new GetRunStatsQuery());
            CaseStats = await mediator.Send(new GetCaseStatsQuery());
            ScheduleStats = await mediator.Send(new GetScheduleStatsQuery());
            UserStats = await mediator.Send(new GetUserStatsQuery());
            RecentRuns = await mediator.Send(new GetRecentRunsQuery { Count = 5 });
        }
    }
}

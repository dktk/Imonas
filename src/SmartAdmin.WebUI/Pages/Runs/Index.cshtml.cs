using Application.Features.ReconciliationRuns.DTOs;
using Application.Features.ReconciliationRuns.Queries;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace SmartAdmin.WebUI.Pages.Runs
{
    public class IndexModel(
        IStringLocalizer<IndexModel> localizer,
        ISender mediator) : PageModel
    {
        public IEnumerable<ReconciliationRunDto> Runs { get; set; } = new List<ReconciliationRunDto>();
        public RunStatsDto Stats { get; set; } = new();

        public async Task OnGetAsync()
        {
            Runs = await mediator.Send(new GetRecentRunsQuery { Count = 50 });
            Stats = await mediator.Send(new GetRunStatsQuery());
        }
    }
}

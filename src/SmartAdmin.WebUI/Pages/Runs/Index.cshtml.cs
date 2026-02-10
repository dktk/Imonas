using Application.Features.ReconciliationRuns.Commands;
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

        public async Task<IActionResult> OnPostArchiveAsync(int id, string comment)
        {
            if (id <= 0)
            {
                return new JsonResult(new { succeeded = false, message = localizer["Invalid run ID."].Value });
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                return new JsonResult(new { succeeded = false, message = localizer["Archive comment is required."].Value });
            }

            var command = new ArchiveRunCommand
            {
                Id = id,
                Comment = comment.Trim()
            };

            var result = await mediator.Send(command);

            if (result.Success)
            {
                return new JsonResult(new
                {
                    succeeded = true,
                    message = localizer["Run archived successfully."].Value
                });
            }

            return new JsonResult(new { succeeded = false, message = result.Message ?? "Failed to archive run." });
        }
    }
}

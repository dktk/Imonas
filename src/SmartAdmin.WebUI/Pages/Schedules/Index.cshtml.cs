using Application.Features.Schedules.Commands;
using Application.Features.Schedules.DTOs;
using Application.Features.Schedules.Queries;

namespace SmartAdmin.WebUI.Pages.Schedules
{
    [Authorize]
    public class IndexModel(
        IStringLocalizer<IndexModel> localizer,
        ISender mediator) : PageModel
    {
        public IEnumerable<ScheduleDto> Schedules { get; set; } = new List<ScheduleDto>();
        public ScheduleStatsDto Stats { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            Schedules = await mediator.Send(new GetSchedulesQuery());
            Stats = await mediator.Send(new GetScheduleStatsQuery());

            if (TempData["SuccessMessage"] != null)
            {
                SuccessMessage = TempData["SuccessMessage"]?.ToString();
            }
        }

        public async Task<IActionResult> OnPostToggleAsync(int id)
        {
            var result = await mediator.Send(new ToggleScheduleStatusCommand { Id = id });

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message ?? "Failed to toggle schedule status.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var result = await mediator.Send(new DeleteScheduleCommand { Id = id });

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message ?? "Failed to delete schedule.";
            }

            return RedirectToPage();
        }
    }
}

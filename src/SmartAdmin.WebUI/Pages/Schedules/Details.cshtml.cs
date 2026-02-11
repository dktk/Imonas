using Application.Features.Schedules.Commands;
using Application.Features.Schedules.DTOs;
using Application.Features.Schedules.Queries;

namespace SmartAdmin.WebUI.Pages.Schedules
{
    [Authorize]
    public class DetailsModel(
        IStringLocalizer<DetailsModel> localizer,
        ISender mediator) : PageModel
    {
        public ScheduleDetailsDto Schedule { get; set; } = null!;
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var schedule = await mediator.Send(new GetScheduleByIdQuery { Id = id });

            if (schedule == null)
            {
                return NotFound();
            }

            Schedule = schedule;
            return Page();
        }

        public async Task<IActionResult> OnPostToggleAsync(int id)
        {
            var result = await mediator.Send(new ToggleScheduleStatusCommand { Id = id });

            if (!result.Success)
            {
                ErrorMessage = result.Message ?? "Failed to toggle schedule status.";
            }

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var result = await mediator.Send(new DeleteScheduleCommand { Id = id });

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToPage("Index");
            }

            ErrorMessage = result.Message ?? "Failed to delete schedule.";
            return RedirectToPage(new { id });
        }
    }
}

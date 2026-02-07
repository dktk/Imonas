using Application.Features.ReconciliationRuns.Commands;
using Application.Features.ReconciliationRuns.DTOs;
using Application.Features.ReconciliationRuns.Queries;

namespace SmartAdmin.WebUI.Pages.Runs
{
    public class DetailsModel(
        IStringLocalizer<DetailsModel> localizer,
        ISender mediator) : PageModel
    {
        public ReconciliationRunDetailsDto? Run { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Run = await mediator.Send(new GetRunByIdQuery { Id = id });

            if (Run == null)
            {
                return Page();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var result = await mediator.Send(new CancelRunCommand { Id = id });

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToPage(new { id });
        }
    }
}

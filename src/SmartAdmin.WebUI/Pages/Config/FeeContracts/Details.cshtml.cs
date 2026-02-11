using Application.Features.Configuration.FeeContracts.Commands;
using Application.Features.Configuration.FeeContracts.DTOs;
using Application.Features.Configuration.FeeContracts.Queries;
using Domain.Enums;

namespace SmartAdmin.WebUI.Pages.Config.FeeContracts
{
    [Authorize]
    public class DetailsModel(
        IStringLocalizer<DetailsModel> localizer,
        ISender mediator) : PageModel
    {
        public FeeContractDto? Contract { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Contract = await mediator.Send(new GetFeeContractByIdQuery { Id = id });
            if (Contract == null) return NotFound();
            return Page();
        }

        public async Task<IActionResult> OnPostSubmitAsync(int id)
        {
            var result = await mediator.Send(new SubmitFeeContractCommand { Id = id });
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var result = await mediator.Send(new ApproveFeeContractCommand { Id = id });
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostRejectAsync(int id, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["ErrorMessage"] = "Rejection reason is required.";
                return RedirectToPage(new { id });
            }

            var result = await mediator.Send(new RejectFeeContractCommand { Id = id, Reason = reason });
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToPage(new { id });
        }
    }
}

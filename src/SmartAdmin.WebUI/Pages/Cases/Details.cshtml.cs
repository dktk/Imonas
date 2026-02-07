using Application.Features.Cases.Commands;
using Application.Features.Cases.DTOs;
using Application.Features.Cases.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace SmartAdmin.WebUI.Pages.Cases
{
    public class DetailsModel(
        IStringLocalizer<DetailsModel> localizer,
        ISender mediator) : PageModel
    {
        public ExceptionCaseDetailsDto? Case { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Case = await mediator.Send(new GetCaseByIdQuery { Id = id });
            return Page();
        }

        public async Task<IActionResult> OnPostAddCommentAsync(int id, string comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                return RedirectToPage(new { id });
            }

            await mediator.Send(new AddCommentCommand { CaseId = id, Comment = comment });
            TempData["SuccessMessage"] = "Comment added successfully.";
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostAssignAsync(int id)
        {
            await mediator.Send(new AssignCaseCommand { CaseId = id });
            TempData["SuccessMessage"] = "Case assigned successfully.";
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostCloseAsync(int id)
        {
            await mediator.Send(new CloseCaseCommand { CaseId = id });
            TempData["SuccessMessage"] = "Case closed successfully.";
            return RedirectToPage(new { id });
        }
    }
}

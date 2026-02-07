using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Imonas.V9.Application.Services;
using Imonas.V9.Domain.Entities.Cases;

namespace Imonas.V9.Web.Pages.Cases;

public class DetailsModel : PageModel
{
    private readonly ICaseService _caseService;

    public DetailsModel(ICaseService caseService)
    {
        _caseService = caseService;
    }

    public Case? Case { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Case = await _caseService.GetCaseAsync(id);

        if (Case == null)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAddCommentAsync(Guid id, string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
        {
            return RedirectToPage(new { id });
        }

        await _caseService.AddCommentAsync(id, comment, "CurrentUser");

        TempData["SuccessMessage"] = "Comment added successfully.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostAssignAsync(Guid id)
    {
        await _caseService.AssignCaseAsync(id, "CurrentUser");

        TempData["SuccessMessage"] = "Case assigned successfully.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostCloseAsync(Guid id)
    {
        await _caseService.CloseCaseAsync(id, "CurrentUser", "Case resolved and closed.");

        TempData["SuccessMessage"] = "Case closed successfully.";
        return RedirectToPage(new { id });
    }
}

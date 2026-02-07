using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Imonas.V9.Application.Services;
using Imonas.V9.Domain.Entities.Reconciliation;

namespace Imonas.V9.Web.Pages.Runs;

public class DetailsModel : PageModel
{
    private readonly IReconciliationService _reconciliationService;

    public DetailsModel(IReconciliationService reconciliationService)
    {
        _reconciliationService = reconciliationService;
    }

    public ReconciliationRun? Run { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Run = await _reconciliationService.GetRunAsync(id);

        if (Run == null)
        {
            return NotFound();
        }

        return Page();
    }
}

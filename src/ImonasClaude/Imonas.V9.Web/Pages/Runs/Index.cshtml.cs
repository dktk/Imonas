using Microsoft.AspNetCore.Mvc.RazorPages;
using Imonas.V9.Application.Services;
using Imonas.V9.Domain.Entities.Reconciliation;

namespace Imonas.V9.Web.Pages.Runs;

public class IndexModel : PageModel
{
    private readonly IReconciliationService _reconciliationService;

    public IndexModel(IReconciliationService reconciliationService)
    {
        _reconciliationService = reconciliationService;
    }

    public List<ReconciliationRun> Runs { get; set; } = new();

    public async Task OnGetAsync()
    {
        Runs = await _reconciliationService.GetRecentRunsAsync(50);
    }
}

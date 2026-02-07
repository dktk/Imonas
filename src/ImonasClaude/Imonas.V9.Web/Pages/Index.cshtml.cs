using Microsoft.AspNetCore.Mvc.RazorPages;
using Imonas.V9.Application.Services;
using Imonas.V9.Domain.Entities.Reconciliation;
using Imonas.V9.Domain.Entities.Cases;

namespace Imonas.V9.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IReconciliationService _reconciliationService;
    private readonly ICaseService _caseService;

    public IndexModel(IReconciliationService reconciliationService, ICaseService caseService)
    {
        _reconciliationService = reconciliationService;
        _caseService = caseService;
    }

    public int TotalRuns { get; set; }
    public decimal MatchPercentage { get; set; }
    public int OpenCases { get; set; }
    public int ProcessedToday { get; set; }
    public List<ReconciliationRun> RecentRuns { get; set; } = new();
    public List<Case> RecentCases { get; set; } = new();

    public async Task OnGetAsync()
    {
        RecentRuns = await _reconciliationService.GetRecentRunsAsync(5);
        RecentCases = await _caseService.GetCasesAsync(pageSize: 5);

        TotalRuns = RecentRuns.Count;
        MatchPercentage = RecentRuns.Any() ? RecentRuns.Average(r => r.MatchPercentage) : 0;
        OpenCases = RecentCases.Count(c => c.Status != Domain.Enums.CaseStatus.Closed);
        ProcessedToday = RecentRuns.Count(r => r.StartedAt.Date == DateTime.UtcNow.Date);
    }
}

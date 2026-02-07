using Microsoft.AspNetCore.Mvc.RazorPages;
using Imonas.V9.Application.Services;
using Imonas.V9.Domain.Entities.Cases;

namespace Imonas.V9.Web.Pages.Cases;

public class IndexModel : PageModel
{
    private readonly ICaseService _caseService;

    public IndexModel(ICaseService caseService)
    {
        _caseService = caseService;
    }

    public List<Case> Cases { get; set; } = new();

    public async Task OnGetAsync()
    {
        Cases = await _caseService.GetCasesAsync(pageSize: 100);
    }
}

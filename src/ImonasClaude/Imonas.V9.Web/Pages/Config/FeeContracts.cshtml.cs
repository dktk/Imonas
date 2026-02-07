using Microsoft.AspNetCore.Mvc.RazorPages;
using Imonas.V9.Infrastructure.Data;
using Imonas.V9.Domain.Entities.Finance;
using Microsoft.EntityFrameworkCore;

namespace Imonas.V9.Web.Pages.Config;

public class FeeContractsModel : PageModel
{
    private readonly ImonasDbContext _context;

    public FeeContractsModel(ImonasDbContext context)
    {
        _context = context;
    }

    public List<FeeContract> Contracts { get; set; } = new();

    public async Task OnGetAsync()
    {
        Contracts = await _context.FeeContracts
            .OrderByDescending(c => c.EffectiveFrom)
            .ToListAsync();
    }
}

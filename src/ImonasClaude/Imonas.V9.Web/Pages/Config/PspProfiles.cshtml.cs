using Microsoft.AspNetCore.Mvc.RazorPages;
using Imonas.V9.Infrastructure.Data;
using Imonas.V9.Domain.Entities.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Imonas.V9.Web.Pages.Config;

public class PspProfilesModel : PageModel
{
    private readonly ImonasDbContext _context;

    public PspProfilesModel(ImonasDbContext context)
    {
        _context = context;
    }

    public List<PspProfile> Profiles { get; set; } = new();

    public async Task OnGetAsync()
    {
        Profiles = await _context.PspProfiles
            .OrderBy(p => p.PspName)
            .ToListAsync();
    }
}

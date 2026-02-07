using Microsoft.AspNetCore.Mvc.RazorPages;
using Imonas.V9.Infrastructure.Data;
using Imonas.V9.Domain.Entities.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Imonas.V9.Web.Pages.Config;

public class StatusMappingsModel : PageModel
{
    private readonly ImonasDbContext _context;

    public StatusMappingsModel(ImonasDbContext context)
    {
        _context = context;
    }

    public List<StatusMapping> StatusMappings { get; set; } = new();
    public Dictionary<Guid, string> PspProfileNames { get; set; } = new();

    public async Task OnGetAsync()
    {
        StatusMappings = await _context.StatusMappings
            .OrderBy(m => m.CanonicalStatus)
            .ThenBy(m => m.PspStatus)
            .ToListAsync();

        // Load PSP profile names for display
        var pspProfiles = await _context.PspProfiles.ToListAsync();
        PspProfileNames = pspProfiles.ToDictionary(p => p.Id, p => p.PspName);
    }
}

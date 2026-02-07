using Microsoft.AspNetCore.Mvc.RazorPages;
using Imonas.V9.Infrastructure.Data;
using Imonas.V9.Domain.Entities.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Imonas.V9.Web.Pages.Config;

public class MappingsModel : PageModel
{
    private readonly ImonasDbContext _context;

    public MappingsModel(ImonasDbContext context)
    {
        _context = context;
    }

    public List<FieldMapping> Mappings { get; set; } = new();
    public Dictionary<Guid, string> PspProfileNames { get; set; } = new();

    public async Task OnGetAsync()
    {
        Mappings = await _context.FieldMappings
            .OrderBy(m => m.SourceField)
            .ToListAsync();

        // Load PSP profile names for display
        var pspProfiles = await _context.PspProfiles.ToListAsync();
        PspProfileNames = pspProfiles.ToDictionary(p => p.Id, p => p.PspName);
    }
}

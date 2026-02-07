using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Imonas.RuleEngine.Data;
using Imonas.RuleEngine.Models;

namespace Imonas.RuleEngine.Pages.RuleGroups;

public class IndexModel(AppDbContext db) : PageModel
{
    public List<RuleGroup> Roots { get; set; } = new();

    public async Task OnGet()
    {
        Roots = await db.RuleGroups
            .Where(g => g.ParentGroupId == null)
            .Include(g => g.Rules)
            .Include(g => g.Children).ThenInclude(c => c.Rules)
            .OrderBy(g => g.Order)
            .ToListAsync();
    }
}

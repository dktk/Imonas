using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Imonas.RuleEngine.Data;
using Imonas.RuleEngine.Models;
using Imonas.RuleEngine.Services;

namespace Imonas.RuleEngine.Pages.Evaluate;

public class IndexModel(AppDbContext db) : PageModel
{
    public List<RuleGroup> Groups { get; set; } = new();
    public List<Transaction> Results { get; set; } = new();
    [BindProperty] public int? SelectedGroupId { get; set; }

    public async Task OnGet()
    {
        Groups = await db.RuleGroups
            .Where(g => g.ParentGroupId == null)
            .Include(g => g.Rules)
            .Include(g => g.Children).ThenInclude(c => c.Rules)
            .OrderBy(g => g.Order).ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Groups = await db.RuleGroups
            .Where(g => g.ParentGroupId == null)
            .Include(g => g.Rules)
            .Include(g => g.Children).ThenInclude(c => c.Rules)
            .OrderBy(g => g.Order).ToListAsync();

        if (SelectedGroupId is null)
        {
            ModelState.AddModelError(string.Empty, "Please choose a rule group.");
            return Page();
        }

        var group = await db.RuleGroups
            .Include(g => g.Rules)
            .Include(g => g.Children).ThenInclude(c => c.Rules)
            .FirstOrDefaultAsync(g => g.Id == SelectedGroupId);

        if (group is null)
        {
            ModelState.AddModelError(string.Empty, "Rule group not found.");
            return Page();
        }

        var predicate = Engine.BuildPredicate(group);
        Results = await db.Transactions.Where(predicate).OrderBy(t => t.Id).ToListAsync();
        return Page();
    }
}

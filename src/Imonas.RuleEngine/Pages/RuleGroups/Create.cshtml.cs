using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Imonas.RuleEngine.Data;
using Imonas.RuleEngine.Models;
using Imonas.RuleEngine.Services;
using Microsoft.EntityFrameworkCore;

namespace Imonas.RuleEngine.Pages.RuleGroups;

public class CreateModel(AppDbContext db) : PageModel
{
    [BindProperty] public GroupVm Root { get; set; } = new() { Name = "New Group", GroupOperator = LogicalOperator.And };
    public List<string> ValidationErrors { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (!ModelState.IsValid) return Page();

        var group = MapToEntity(Root, null, 0);
        var errors = Engine.ValidateRulesRecursive(group).ToList();
        if (errors.Any())
        {
            ValidationErrors = errors;
            return Page();
        }

        db.RuleGroups.Add(group);
        await db.SaveChangesAsync();
        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostPreviewAsync()
    {
        if (!ModelState.IsValid) return Page();

        var group = MapToEntity(Root, null, 0);
        var errors = Engine.ValidateRulesRecursive(group).ToList();
        if (errors.Any())
        {
            ValidationErrors = errors;
            return Page();
        }

        var predicate = Engine.BuildPredicate(group);
        var count = await db.Transactions.Where(predicate).CountAsync();
        ViewData["PreviewCount"] = count;
        return Page();
    }

    private static RuleGroup MapToEntity(GroupVm vm, RuleGroup? parent, int order)
    {
        var g = new RuleGroup
        {
            Name = vm.Name,
            GroupOperator = vm.GroupOperator,
            ParentGroup = parent,
            Order = order
        };

        for (int i = 0; i < vm.Rules.Count; i++)
        {
            var rvm = vm.Rules[i];
            g.Rules.Add(new Rule
            {
                Field = rvm.Field,
                Operator = rvm.Operator,
                Value = rvm.Field == RuleField.Status ? rvm.ValueStatus : rvm.ValueText,
                Order = i
            });
        }

        for (int i = 0; i < vm.Children.Count; i++)
        {
            var cvm = vm.Children[i];
            var child = MapToEntity(cvm, g, i);
            g.Children.Add(child);
        }

        return g;
    }
}

public class GroupVm
{
    public string Name { get; set; } = "Group";
    public LogicalOperator GroupOperator { get; set; } = LogicalOperator.And;
    public List<RuleVm> Rules { get; set; } = new();
    public List<GroupVm> Children { get; set; } = new();
}

public class RuleVm
{
    public RuleField Field { get; set; }
    public RuleOperator Operator { get; set; }
    public string ValueText { get; set; } = string.Empty;
    public string ValueStatus { get; set; } = nameof(Imonas.RuleEngine.Models.Status.Pending);
}

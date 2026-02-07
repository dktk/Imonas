using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Imonas.V9.Infrastructure.Data;
using Imonas.V9.Domain.Entities.Rules;
using Imonas.V9.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Imonas.V9.Web.Pages.Rules;

public class CreateModel : PageModel
{
    private readonly ImonasDbContext _context;

    public CreateModel(ImonasDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [StringLength(255)]
        public string RuleName { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public RuleType RuleType { get; set; } = RuleType.Equality;

        [Required]
        public string RuleDefinition { get; set; } = @"{""field"": ""TransactionId"", ""operator"": ""equals""}";

        public int Priority { get; set; } = 10;

        public bool IsActive { get; set; } = true;

        public bool StopAtFirstMatch { get; set; } = false;

        public decimal? ToleranceAmount { get; set; }

        public int? ToleranceWindowDays { get; set; }

        public decimal? MinimumScore { get; set; }

        [Required]
        public string Version { get; set; } = "1.0.0";

        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var rule = new MatchingRule
            {
                Id = Guid.NewGuid(),
                RuleName = Input.RuleName,
                Description = Input.Description,
                RuleType = Input.RuleType,
                RuleDefinition = Input.RuleDefinition,
                Priority = Input.Priority,
                IsActive = Input.IsActive,
                StopAtFirstMatch = Input.StopAtFirstMatch,
                ToleranceAmount = Input.ToleranceAmount,
                ToleranceWindowDays = Input.ToleranceWindowDays,
                MinimumScore = Input.MinimumScore,
                Version = Input.Version,
                EffectiveFrom = Input.EffectiveFrom,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "CurrentUser"
            };

            _context.MatchingRules.Add(rule);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Matching rule '{Input.RuleName}' created successfully.";
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to create rule: {ex.Message}";
            return Page();
        }
    }
}

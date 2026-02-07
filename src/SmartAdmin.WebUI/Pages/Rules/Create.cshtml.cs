using Application.Features.Rules.Commands;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;

namespace SmartAdmin.WebUI.Pages.Rules
{
    public class CreateModel(
        IStringLocalizer<CreateModel> localizer,
        ISender mediator) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(200)]
            public string RuleName { get; set; } = string.Empty;

            [Required]
            public string Description { get; set; } = string.Empty;

            [Required]
            public RuleType RuleType { get; set; } = RuleType.Equality;

            [Required]
            public string RuleDefinition { get; set; } = "{}";

            [Required]
            [Range(1, 1000)]
            public int Priority { get; set; } = 100;

            public bool IsActive { get; set; } = true;
            public bool StopAtFirstMatch { get; set; } = false;

            public decimal? ToleranceAmount { get; set; }
            public int? ToleranceWindowDays { get; set; }
            public decimal? MinimumScore { get; set; }

            [Required]
            public string Version { get; set; } = "1.0.0";

            [Required]
            public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow.Date;

            public DateTime? EffectiveTo { get; set; }
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
                var command = new CreateMatchingRuleCommand
                {
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
                    EffectiveTo = Input.EffectiveTo
                };

                var result = await mediator.Send(command);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToPage("Index");
                }

                ErrorMessage = result.Message ?? "Failed to create matching rule.";
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to create rule: {ex.Message}";
                return Page();
            }
        }
    }
}

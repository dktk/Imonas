using System.ComponentModel.DataAnnotations;

using Application.Features.Rules.Commands;
using Application.Features.Rules.Queries;

namespace SmartAdmin.WebUI.Pages.Rules
{
    [Authorize]
    public class EditModel(
        IStringLocalizer<EditModel> localizer,
        ISender mediator) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            public int Id { get; set; }

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

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var rule = await mediator.Send(new GetMatchingRuleByIdQuery { Id = id });

            if (rule == null)
            {
                return RedirectToPage("Index");
            }

            Input = new InputModel
            {
                Id = rule.Id,
                RuleName = rule.RuleName,
                Description = rule.Description,
                RuleType = rule.RuleType,
                RuleDefinition = rule.RuleDefinition,
                Priority = rule.Priority,
                IsActive = rule.IsActive,
                StopAtFirstMatch = rule.StopAtFirstMatch,
                ToleranceAmount = rule.ToleranceAmount,
                ToleranceWindowDays = rule.ToleranceWindowDays,
                MinimumScore = rule.MinimumScore,
                Version = rule.Version,
                EffectiveFrom = rule.EffectiveFrom,
                EffectiveTo = rule.EffectiveTo
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var command = new UpdateMatchingRuleCommand
                {
                    Id = Input.Id,
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

                ErrorMessage = result.Message ?? "Failed to update matching rule.";
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to update rule: {ex.Message}";
                return Page();
            }
        }
    }
}

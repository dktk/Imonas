using Application.Features.ReconciliationRuns.Commands;
using Application.Features.Rules.Queries;
using Application.Features.Settlement.Commands;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;

namespace SmartAdmin.WebUI.Pages.Runs
{
    public class CreateModel(
        IStringLocalizer<CreateModel> localizer,
        ISender mediator) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public IEnumerable<MatchingRuleDto> AllRules { get; set; } = new List<MatchingRuleDto>();

        public class InputModel
        {
            [Required]
            [StringLength(255)]
            public string RunName { get; set; } = $"Daily Recon - {DateTime.UtcNow:yyyy-MM-dd}";

            [Required]
            public string RulePackVersion { get; set; } = "1.0.0";

            public string? Description { get; set; }

            public bool IncludeInternalTransactions { get; set; } = true;
            public bool IncludePspTransactions { get; set; } = true;
            public bool IncludeBankTransactions { get; set; } = true;

            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }

            public bool StopAtFirstMatch { get; set; } = false;
            public bool GenerateEvidencePack { get; set; } = true;
            public bool CreateCasesForExceptions { get; set; } = true;
        }

        public async Task OnGetAsync()
        {
            Input.EndDate = DateTime.UtcNow.Date;
            Input.StartDate = DateTime.UtcNow.Date.AddDays(-7);
            AllRules = await mediator.Send(new GetMatchingRulesQuery());
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var command = new StartRunCommand
                {
                    RunName = Input.RunName,
                    RulePackVersion = Input.RulePackVersion,
                    Description = Input.Description,
                    IncludeInternalTransactions = Input.IncludeInternalTransactions,
                    IncludePspTransactions = Input.IncludePspTransactions,
                    IncludeBankTransactions = Input.IncludeBankTransactions,
                    StartDate = Input.StartDate,
                    EndDate = Input.EndDate,
                    StopAtFirstMatch = Input.StopAtFirstMatch,
                    GenerateEvidencePack = Input.GenerateEvidencePack,
                    CreateCasesForExceptions = Input.CreateCasesForExceptions
                };

                var result = await mediator.Send(command);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = $"Reconciliation run '{Input.RunName}' started successfully.";
                    return RedirectToPage("Details", new { id = result.Value?.Id });
                }

                ErrorMessage = result.Message ?? "Failed to start reconciliation run.";
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to start reconciliation run: {ex.Message}";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostSimulateAsync([FromBody] SimulateRequest request)
        {
            try
            {
                if (request.CandidateRuleIds == null || !request.CandidateRuleIds.Any())
                {
                    return new JsonResult(new
                    {
                        succeeded = false,
                        message = localizer["Please select at least one candidate rule."].Value
                    });
                }

                var command = new RunSimulationCommand
                {
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    PspId = request.PspId,
                    CurrencyCode = request.CurrencyCode,
                    CandidateRuleIds = request.CandidateRuleIds,
                    FalsePositiveThreshold = request.FalsePositiveThreshold ?? 0.7m,
                    StopAtFirstMatch = request.StopAtFirstMatch
                };

                var result = await mediator.Send(command);

                if (result.Success)
                {
                    return new JsonResult(new
                    {
                        succeeded = true,
                        message = result.Message,
                        data = result.Value
                    });
                }

                return new JsonResult(new
                {
                    succeeded = false,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    succeeded = false,
                    message = $"Simulation failed: {ex.Message}"
                });
            }
        }

        public class SimulateRequest
        {
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public int? PspId { get; set; }
            public string? CurrencyCode { get; set; }
            public List<int> CandidateRuleIds { get; set; } = new();
            public decimal? FalsePositiveThreshold { get; set; }
            public bool StopAtFirstMatch { get; set; }
        }
    }
}

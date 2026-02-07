using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Imonas.V9.Application.Services;
using System.ComponentModel.DataAnnotations;

namespace Imonas.V9.Web.Pages.Runs;

public class CreateModel : PageModel
{
    private readonly IReconciliationService _reconciliationService;

    public CreateModel(IReconciliationService reconciliationService)
    {
        _reconciliationService = reconciliationService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

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

    public void OnGet()
    {
        // Set default date range to last 7 days
        Input.EndDate = DateTime.UtcNow.Date;
        Input.StartDate = DateTime.UtcNow.Date.AddDays(-7);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var run = await _reconciliationService.StartRunAsync(Input.RunName, Input.RulePackVersion);

            TempData["SuccessMessage"] = $"Reconciliation run '{Input.RunName}' started successfully.";
            return RedirectToPage("Details", new { id = run.Id });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to start reconciliation run: {ex.Message}";
            return Page();
        }
    }
}

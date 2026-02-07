using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Imonas.V9.Application.Services;
using Imonas.V9.Domain.Entities.Cases;
using Imonas.V9.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Imonas.V9.Web.Pages.Cases;

public class CreateModel : PageModel
{
    private readonly ICaseService _caseService;

    public CreateModel(ICaseService caseService)
    {
        _caseService = caseService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [StringLength(500)]
        public string Title { get; set; } = string.Empty;

        public string CaseNumber { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public CaseSeverity Severity { get; set; } = CaseSeverity.Medium;

        [Required]
        public VarianceType VarianceType { get; set; } = VarianceType.Amount;

        public decimal? VarianceAmount { get; set; }

        public string? AssignedTo { get; set; }

        public DateTime? DueDate { get; set; }

        public string? LinkedTransactionId { get; set; }

        public string? RootCauseCode { get; set; }

        public string? InitialNotes { get; set; }
    }

    public void OnGet()
    {
        // Generate case number
        Input.CaseNumber = $"CASE-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var caseEntity = new Case
            {
                CaseNumber = Input.CaseNumber,
                Title = Input.Title,
                Description = Input.Description,
                Severity = Input.Severity,
                VarianceType = Input.VarianceType,
                VarianceAmount = Input.VarianceAmount,
                AssignedTo = Input.AssignedTo,
                DueDate = Input.DueDate,
                LinkedTransactionId = string.IsNullOrWhiteSpace(Input.LinkedTransactionId)
                    ? null
                    : Guid.Parse(Input.LinkedTransactionId),
                RootCauseCode = Input.RootCauseCode,
                CreatedBy = "CurrentUser" // TODO: Get from auth context
            };

            var createdCase = await _caseService.CreateCaseAsync(caseEntity);

            // If initial notes provided, add as first comment
            if (!string.IsNullOrWhiteSpace(Input.InitialNotes))
            {
                await _caseService.AddCommentAsync(
                    createdCase.Id,
                    Input.InitialNotes,
                    "CurrentUser"
                );
            }

            TempData["SuccessMessage"] = $"Case {Input.CaseNumber} created successfully.";
            return RedirectToPage("Details", new { id = createdCase.Id });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to create case: {ex.Message}";
            return Page();
        }
    }
}

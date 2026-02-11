using Application.Features.Cases.Commands;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;

namespace SmartAdmin.WebUI.Pages.Cases
{
    [Authorize]
    public class CreateModel(
        IStringLocalizer<CreateModel> localizer,
        ISender mediator) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string GeneratedCaseNumber { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(500)]
            public string Title { get; set; } = string.Empty;

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
            GeneratedCaseNumber = $"CASE-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                GeneratedCaseNumber = $"CASE-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
                return Page();
            }

            try
            {
                var command = new CreateCaseCommand
                {
                    Title = Input.Title,
                    Description = Input.Description,
                    Severity = Input.Severity,
                    VarianceType = Input.VarianceType,
                    VarianceAmount = Input.VarianceAmount,
                    AssignedTo = Input.AssignedTo,
                    DueDate = Input.DueDate,
                    LinkedTransactionId = Input.LinkedTransactionId,
                    RootCauseCode = Input.RootCauseCode,
                    InitialNotes = Input.InitialNotes
                };

                var result = await mediator.Send(command);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToPage("Details", new { id = result.Value?.Id });
                }

                ErrorMessage = result.Message ?? "Failed to create case.";
                GeneratedCaseNumber = $"CASE-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to create case: {ex.Message}";
                GeneratedCaseNumber = $"CASE-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
                return Page();
            }
        }
    }
}

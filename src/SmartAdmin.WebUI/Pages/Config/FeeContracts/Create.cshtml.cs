using System.ComponentModel.DataAnnotations;
using Application.Features.Configuration.FeeContracts.Commands;
using Application.Features.Psps.DTOs;
using Application.Features.Psps.Queries;
using Domain.Enums;

namespace SmartAdmin.WebUI.Pages.Config.FeeContracts
{
    [Authorize]
    public class CreateModel(
        IStringLocalizer<CreateModel> localizer,
        ISender mediator) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public IEnumerable<PspDto> PspProfiles { get; set; } = new List<PspDto>();
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(255)]
            [Display(Name = "Contract Name")]
            public string ContractName { get; set; } = string.Empty;

            [Required]
            [Display(Name = "PSP Profile")]
            public int PspId { get; set; }

            [Required]
            [Display(Name = "Payment Method")]
            public string PaymentMethod { get; set; } = "Card";

            [Required]
            [Display(Name = "Brand")]
            public string Brand { get; set; } = "Visa";

            [Required]
            [Display(Name = "Currency")]
            public string Currency { get; set; } = "USD";

            [Required]
            [Display(Name = "Fee Structure")]
            public FeeStructureType FeeStructure { get; set; } = FeeStructureType.Hybrid;

            [Display(Name = "Fixed Fee")]
            public decimal? FixedFee { get; set; }

            [Display(Name = "Percentage Fee")]
            public decimal? PercentageFee { get; set; }

            [Display(Name = "Minimum Fee")]
            public decimal? MinimumFee { get; set; }

            [Display(Name = "Maximum Fee")]
            public decimal? MaximumFee { get; set; }

            [Display(Name = "Tiered Structure (JSON)")]
            public string? TieredStructure { get; set; }

            [Required]
            [Display(Name = "Effective From")]
            public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;

            [Display(Name = "Effective To")]
            public DateTime? EffectiveTo { get; set; }

            [Required]
            [Display(Name = "Version")]
            public string Version { get; set; } = "1.0";
        }

        public async Task OnGetAsync()
        {
            PspProfiles = await mediator.Send(new GetAllPspsQuery());
        }

        public async Task<IActionResult> OnPostAsync(string action)
        {
            if (!ModelState.IsValid)
            {
                PspProfiles = await mediator.Send(new GetAllPspsQuery());
                return Page();
            }

            try
            {
                var command = new CreateFeeContractCommand
                {
                    PspId = Input.PspId,
                    ContractName = Input.ContractName,
                    PaymentMethod = Input.PaymentMethod,
                    Brand = Input.Brand,
                    Currency = Input.Currency,
                    FeeStructure = Input.FeeStructure,
                    FixedFee = Input.FixedFee,
                    PercentageFee = Input.PercentageFee,
                    MinimumFee = Input.MinimumFee,
                    MaximumFee = Input.MaximumFee,
                    TieredStructure = Input.TieredStructure,
                    EffectiveFrom = Input.EffectiveFrom,
                    EffectiveTo = Input.EffectiveTo,
                    Version = Input.Version,
                    SubmitForApproval = action == "submit"
                };

                var result = await mediator.Send(command);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToPage("Details", new { id = result.Value?.Id });
                }

                ErrorMessage = result.Message ?? "Failed to create fee contract.";
                PspProfiles = await mediator.Send(new GetAllPspsQuery());
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to create fee contract: {ex.Message}";
                PspProfiles = await mediator.Send(new GetAllPspsQuery());
                return Page();
            }
        }
    }
}

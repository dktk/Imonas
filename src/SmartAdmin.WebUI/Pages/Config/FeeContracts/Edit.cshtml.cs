using System.ComponentModel.DataAnnotations;

using Application.Features.Configuration.FeeContracts.Commands;
using Application.Features.Configuration.FeeContracts.DTOs;
using Application.Features.Configuration.FeeContracts.Queries;

namespace SmartAdmin.WebUI.Pages.Config.FeeContracts
{
    [Authorize]
    public class EditModel(
        IStringLocalizer<EditModel> localizer,
        ISender mediator) : PageModel
    {
        public FeeContractDto? Contract { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            public int Id { get; set; }

            [Required]
            [StringLength(255)]
            [Display(Name = "Contract Name")]
            public string ContractName { get; set; } = string.Empty;

            [Required]
            [Display(Name = "Payment Method")]
            public string PaymentMethod { get; set; } = string.Empty;

            [Display(Name = "Brand")]
            public string Brand { get; set; } = string.Empty;

            [Required]
            [Display(Name = "Currency")]
            public string Currency { get; set; } = string.Empty;

            [Required]
            [Display(Name = "Fee Structure")]
            public FeeStructureType FeeStructure { get; set; }

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
            public DateTime EffectiveFrom { get; set; }

            [Display(Name = "Effective To")]
            public DateTime? EffectiveTo { get; set; }

            [Required]
            [Display(Name = "Version")]
            public string Version { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Contract = await mediator.Send(new GetFeeContractByIdQuery { Id = id });
            if (Contract == null) return NotFound();

            if (Contract.Status != ContractStatus.Draft)
                return Page();

            Input = new InputModel
            {
                Id = Contract.Id,
                ContractName = Contract.ContractName,
                PaymentMethod = Contract.PaymentMethod,
                Brand = Contract.Brand,
                Currency = Contract.Currency,
                FeeStructure = Contract.FeeStructure,
                FixedFee = Contract.FixedFee,
                PercentageFee = Contract.PercentageFee,
                MinimumFee = Contract.MinimumFee,
                MaximumFee = Contract.MaximumFee,
                TieredStructure = Contract.TieredStructure,
                EffectiveFrom = Contract.EffectiveFrom,
                EffectiveTo = Contract.EffectiveTo,
                Version = Contract.Version
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string action)
        {
            if (!ModelState.IsValid)
            {
                Contract = await mediator.Send(new GetFeeContractByIdQuery { Id = Input.Id });
                return Page();
            }

            try
            {
                var command = new UpdateFeeContractCommand
                {
                    Id = Input.Id,
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
                    return RedirectToPage("Details", new { id = Input.Id });
                }

                ErrorMessage = result.Message ?? "Failed to update fee contract.";
                Contract = await mediator.Send(new GetFeeContractByIdQuery { Id = Input.Id });
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to update fee contract: {ex.Message}";
                Contract = await mediator.Send(new GetFeeContractByIdQuery { Id = Input.Id });
                return Page();
            }
        }
    }
}

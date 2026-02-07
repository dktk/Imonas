using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Imonas.V9.Application.Services;
using Imonas.V9.Domain.Entities.Finance;
using System.ComponentModel.DataAnnotations;

namespace Imonas.V9.Web.Pages.Config.FeeContracts;

public class EditModel : PageModel
{
    private readonly IFeeContractService _feeContractService;

    public EditModel(IFeeContractService feeContractService)
    {
        _feeContractService = feeContractService;
    }

    public FeeContract? Contract { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public Guid Id { get; set; }

        [Required]
        [Display(Name = "Contract Name")]
        public string ContractName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = string.Empty;

        [Display(Name = "Brand")]
        public string? Brand { get; set; }

        [Required]
        [Display(Name = "Currency")]
        public string Currency { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Fee Structure")]
        public string FeeStructure { get; set; } = string.Empty;

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

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Contract = await _feeContractService.GetContractAsync(id);

        if (Contract == null)
        {
            return NotFound();
        }

        if (Contract.Status != "Draft")
        {
            return Page();
        }

        // Populate input model from contract
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
            Contract = await _feeContractService.GetContractAsync(Input.Id);
            return Page();
        }

        // Get existing contract
        var existingContract = await _feeContractService.GetContractAsync(Input.Id);
        if (existingContract == null)
        {
            TempData["ErrorMessage"] = "Contract not found.";
            return RedirectToPage("/Config/FeeContracts");
        }

        if (existingContract.Status != "Draft")
        {
            TempData["ErrorMessage"] = "Only draft contracts can be edited.";
            return RedirectToPage("Details", new { id = Input.Id });
        }

        // Update contract properties
        existingContract.ContractName = Input.ContractName;
        existingContract.PaymentMethod = Input.PaymentMethod;
        existingContract.Brand = Input.Brand;
        existingContract.Currency = Input.Currency;
        existingContract.FeeStructure = Input.FeeStructure;
        existingContract.FixedFee = Input.FixedFee;
        existingContract.PercentageFee = Input.PercentageFee;
        existingContract.MinimumFee = Input.MinimumFee;
        existingContract.MaximumFee = Input.MaximumFee;
        existingContract.TieredStructure = Input.TieredStructure;
        existingContract.EffectiveFrom = Input.EffectiveFrom;
        existingContract.EffectiveTo = Input.EffectiveTo;
        existingContract.Version = Input.Version;

        var updated = await _feeContractService.UpdateContractAsync(existingContract);

        if (updated == null)
        {
            TempData["ErrorMessage"] = "Failed to update contract.";
            Contract = existingContract;
            return Page();
        }

        if (action == "submit")
        {
            // Submit for approval
            var submitted = await _feeContractService.SubmitForApprovalAsync(updated.Id, "current-user"); // TODO: Get from auth context
            if (submitted)
            {
                TempData["SuccessMessage"] = "Contract updated and submitted for approval.";
            }
            else
            {
                TempData["SuccessMessage"] = "Contract updated but failed to submit for approval.";
            }
        }
        else
        {
            TempData["SuccessMessage"] = "Contract updated successfully.";
        }

        return RedirectToPage("Details", new { id = updated.Id });
    }
}

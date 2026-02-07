using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Imonas.V9.Application.Services;
using Imonas.V9.Domain.Entities.Finance;
using Imonas.V9.Domain.Entities.Configuration;
using Imonas.V9.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Imonas.V9.Web.Pages.Config.FeeContracts;

public class CreateModel : PageModel
{
    private readonly IFeeContractService _feeContractService;
    private readonly ImonasDbContext _context;

    public CreateModel(IFeeContractService feeContractService, ImonasDbContext context)
    {
        _feeContractService = feeContractService;
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<PspProfile> PspProfiles { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [StringLength(255)]
        public string ContractName { get; set; } = string.Empty;

        [Required]
        public Guid PspProfileId { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = "card";

        [Required]
        public string Brand { get; set; } = "Visa";

        [Required]
        public string Currency { get; set; } = "USD";

        [Required]
        public string FeeStructure { get; set; } = "hybrid";

        public decimal? FixedFee { get; set; }

        public decimal? PercentageFee { get; set; }

        public decimal? MinimumFee { get; set; }

        public decimal? MaximumFee { get; set; }

        public string? TieredStructure { get; set; }

        [Required]
        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;

        public DateTime? EffectiveTo { get; set; }

        [Required]
        public string Version { get; set; } = "1.0";
    }

    public async Task OnGetAsync()
    {
        PspProfiles = await _context.PspProfiles
            .Where(p => p.IsActive)
            .OrderBy(p => p.PspName)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync(string action)
    {
        PspProfiles = await _context.PspProfiles
            .Where(p => p.IsActive)
            .OrderBy(p => p.PspName)
            .ToListAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var contract = new FeeContract
            {
                ContractName = Input.ContractName,
                PspProfileId = Input.PspProfileId,
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
                CreatedBy = "CurrentUser" // TODO: Get from auth context
            };

            var created = await _feeContractService.CreateContractAsync(contract);

            // If action is "submit", also submit for approval
            if (action == "submit")
            {
                await _feeContractService.SubmitForApprovalAsync(created.Id, "CurrentUser");
                TempData["SuccessMessage"] = $"Fee contract '{Input.ContractName}' created and submitted for approval.";
            }
            else
            {
                TempData["SuccessMessage"] = $"Fee contract '{Input.ContractName}' saved as draft.";
            }

            return RedirectToPage("Details", new { id = created.Id });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to create fee contract: {ex.Message}";
            return Page();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Imonas.V9.Application.Services;
using Imonas.V9.Domain.Entities.Finance;

namespace Imonas.V9.Web.Pages.Config.FeeContracts;

public class DetailsModel : PageModel
{
    private readonly IFeeContractService _feeContractService;

    public DetailsModel(IFeeContractService feeContractService)
    {
        _feeContractService = feeContractService;
    }

    public FeeContract? Contract { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Contract = await _feeContractService.GetContractAsync(id);

        if (Contract == null)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostSubmitAsync(Guid id)
    {
        var success = await _feeContractService.SubmitForApprovalAsync(id, "current-user"); // TODO: Get from auth context

        if (success)
        {
            TempData["SuccessMessage"] = "Contract submitted for approval successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to submit contract for approval.";
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostApproveAsync(Guid id)
    {
        var success = await _feeContractService.ApproveContractAsync(id, "current-user"); // TODO: Get from auth context

        if (success)
        {
            TempData["SuccessMessage"] = "Contract approved successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to approve contract.";
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostRejectAsync(Guid id, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["ErrorMessage"] = "Rejection reason is required.";
            return RedirectToPage(new { id });
        }

        var success = await _feeContractService.RejectContractAsync(id, "current-user", reason); // TODO: Get from auth context

        if (success)
        {
            TempData["SuccessMessage"] = $"Contract rejected. Reason: {reason}";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to reject contract.";
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnGetCalculateFeeAsync(Guid id, decimal amount)
    {
        try
        {
            var fee = await _feeContractService.CalculateFeeAsync(id, amount);
            return new JsonResult(new { fee });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { error = ex.Message }) { StatusCode = 400 };
        }
    }
}

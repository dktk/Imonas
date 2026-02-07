using Imonas.V9.Domain.Entities.Finance;
using Imonas.V9.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Imonas.V9.Application.Services;

public class FeeContractService : IFeeContractService
{
    private readonly ImonasDbContext _context;

    public FeeContractService(ImonasDbContext context)
    {
        _context = context;
    }

    public async Task<FeeContract> CreateContractAsync(FeeContract contract)
    {
        contract.Id = Guid.NewGuid();
        contract.Status = "Draft";
        contract.CreatedAt = DateTime.UtcNow;

        _context.FeeContracts.Add(contract);
        await _context.SaveChangesAsync();

        return contract;
    }

    public async Task<FeeContract?> GetContractAsync(Guid id)
    {
        return await _context.FeeContracts
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<FeeContract>> GetContractsAsync(Guid? pspProfileId = null)
    {
        var query = _context.FeeContracts.AsQueryable();

        if (pspProfileId.HasValue)
        {
            query = query.Where(c => c.PspProfileId == pspProfileId.Value);
        }

        return await query
            .OrderByDescending(c => c.EffectiveFrom)
            .ToListAsync();
    }

    public async Task<FeeContract?> UpdateContractAsync(FeeContract contract)
    {
        var existing = await _context.FeeContracts.FindAsync(contract.Id);
        if (existing == null) return null;

        // Only allow updates if in Draft status
        if (existing.Status != "Draft")
        {
            throw new InvalidOperationException("Only draft contracts can be modified.");
        }

        existing.ContractName = contract.ContractName;
        existing.PaymentMethod = contract.PaymentMethod;
        existing.Brand = contract.Brand;
        existing.Currency = contract.Currency;
        existing.FeeStructure = contract.FeeStructure;
        existing.FixedFee = contract.FixedFee;
        existing.PercentageFee = contract.PercentageFee;
        existing.MinimumFee = contract.MinimumFee;
        existing.MaximumFee = contract.MaximumFee;
        existing.TieredStructure = contract.TieredStructure;
        existing.EffectiveFrom = contract.EffectiveFrom;
        existing.EffectiveTo = contract.EffectiveTo;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> SubmitForApprovalAsync(Guid contractId, string submittedBy)
    {
        var contract = await _context.FeeContracts.FindAsync(contractId);
        if (contract == null) return false;

        if (contract.Status != "Draft")
        {
            throw new InvalidOperationException("Only draft contracts can be submitted for approval.");
        }

        contract.Status = "PendingApproval";
        contract.UpdatedAt = DateTime.UtcNow;
        contract.UpdatedBy = submittedBy;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ApproveContractAsync(Guid contractId, string approvedBy)
    {
        var contract = await _context.FeeContracts.FindAsync(contractId);
        if (contract == null) return false;

        if (contract.Status != "PendingApproval")
        {
            throw new InvalidOperationException("Only contracts pending approval can be approved.");
        }

        contract.Status = "Active";
        contract.ApprovedBy = approvedBy;
        contract.ApprovedAt = DateTime.UtcNow;
        contract.UpdatedAt = DateTime.UtcNow;
        contract.UpdatedBy = approvedBy;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejectContractAsync(Guid contractId, string rejectedBy, string reason)
    {
        var contract = await _context.FeeContracts.FindAsync(contractId);
        if (contract == null) return false;

        if (contract.Status != "PendingApproval")
        {
            throw new InvalidOperationException("Only contracts pending approval can be rejected.");
        }

        contract.Status = "Rejected";
        contract.UpdatedAt = DateTime.UtcNow;
        contract.UpdatedBy = rejectedBy;

        // Store rejection reason in tiered_structure field as JSON (or add a new field)
        contract.TieredStructure = $"{{\"rejection_reason\": \"{reason}\"}}";

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<decimal> CalculateFeeAsync(Guid contractId, decimal transactionAmount)
    {
        var contract = await _context.FeeContracts.FindAsync(contractId);
        if (contract == null || contract.Status != "Active")
        {
            throw new InvalidOperationException("Contract not found or not active.");
        }

        decimal fee = 0m;

        switch (contract.FeeStructure.ToLower())
        {
            case "fixed":
                fee = contract.FixedFee ?? 0m;
                break;

            case "percentage":
                fee = transactionAmount * (contract.PercentageFee ?? 0m);
                break;

            case "hybrid":
                fee = (contract.FixedFee ?? 0m) + (transactionAmount * (contract.PercentageFee ?? 0m));
                break;

            case "tiered":
                // TODO: Implement tiered fee calculation based on TieredStructure JSON
                fee = 0m;
                break;
        }

        // Apply minimum and maximum constraints
        if (contract.MinimumFee.HasValue && fee < contract.MinimumFee.Value)
        {
            fee = contract.MinimumFee.Value;
        }

        if (contract.MaximumFee.HasValue && fee > contract.MaximumFee.Value)
        {
            fee = contract.MaximumFee.Value;
        }

        return Math.Round(fee, 2);
    }

    public async Task<List<FeeContract>> GetActiveContractsAsync(string pspCode, string paymentMethod, string currency)
    {
        var now = DateTime.UtcNow;

        return await _context.FeeContracts
            .Where(c => c.Status == "Active")
            .Where(c => c.PaymentMethod == paymentMethod)
            .Where(c => c.Currency == currency)
            .Where(c => c.EffectiveFrom <= now)
            .Where(c => c.EffectiveTo == null || c.EffectiveTo > now)
            .OrderByDescending(c => c.EffectiveFrom)
            .ToListAsync();
    }
}

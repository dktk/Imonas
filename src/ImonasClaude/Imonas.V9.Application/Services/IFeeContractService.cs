using Imonas.V9.Domain.Entities.Finance;

namespace Imonas.V9.Application.Services;

public interface IFeeContractService
{
    Task<FeeContract> CreateContractAsync(FeeContract contract);
    Task<FeeContract?> GetContractAsync(Guid id);
    Task<List<FeeContract>> GetContractsAsync(Guid? pspProfileId = null);
    Task<FeeContract?> UpdateContractAsync(FeeContract contract);
    Task<bool> SubmitForApprovalAsync(Guid contractId, string submittedBy);
    Task<bool> ApproveContractAsync(Guid contractId, string approvedBy);
    Task<bool> RejectContractAsync(Guid contractId, string rejectedBy, string reason);
    Task<decimal> CalculateFeeAsync(Guid contractId, decimal transactionAmount);
    Task<List<FeeContract>> GetActiveContractsAsync(string pspCode, string paymentMethod, string currency);
}

using SG.Common;

namespace Application.Features.Configuration.FeeContracts.Commands
{
    public class RejectFeeContractCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class RejectFeeContractCommandHandler(
        IApplicationDbContext context) :
        IRequestHandler<RejectFeeContractCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(RejectFeeContractCommand request, CancellationToken cancellationToken)
        {
            var contract = await context.FeeContracts.FindAsync(new object[] { request.Id }, cancellationToken);
            if (contract == null)
                return Result<bool>.CreateFailure("Fee contract not found.");

            if (contract.Status != ContractStatus.PendingApproval)
                return Result<bool>.CreateFailure("Only contracts pending approval can be rejected.");

            contract.Status = ContractStatus.Rejected;
            contract.RejectionReason = request.Reason;
            await context.SaveChangesAsync(cancellationToken);

            return Result<bool>.CreateSuccess(true, "Contract rejected.");
        }
    }
}

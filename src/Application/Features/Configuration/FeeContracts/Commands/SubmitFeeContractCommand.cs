using Domain.Enums;
using SG.Common;

namespace Application.Features.Configuration.FeeContracts.Commands
{
    public class SubmitFeeContractCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }

    public class SubmitFeeContractCommandHandler(
        IApplicationDbContext context) :
        IRequestHandler<SubmitFeeContractCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(SubmitFeeContractCommand request, CancellationToken cancellationToken)
        {
            var contract = await context.FeeContracts.FindAsync(new object[] { request.Id }, cancellationToken);
            if (contract == null)
                return Result<bool>.CreateFailure("Fee contract not found.");

            if (contract.Status != ContractStatus.Draft)
                return Result<bool>.CreateFailure("Only draft contracts can be submitted for approval.");

            contract.Status = ContractStatus.PendingApproval;
            await context.SaveChangesAsync(cancellationToken);

            return Result<bool>.CreateSuccess(true, "Contract submitted for approval.");
        }
    }
}

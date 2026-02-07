using SG.Common;

namespace Application.Features.Configuration.FeeContracts.Commands
{
    public class ApproveFeeContractCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }

    public class ApproveFeeContractCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IDateTime dateTime) :
        IRequestHandler<ApproveFeeContractCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(ApproveFeeContractCommand request, CancellationToken cancellationToken)
        {
            var contract = await context.FeeContracts.FindAsync(new object[] { request.Id }, cancellationToken);
            if (contract == null)
                return Result<bool>.CreateFailure("Fee contract not found.");

            if (contract.Status != ContractStatus.PendingApproval)
                return Result<bool>.CreateFailure("Only contracts pending approval can be approved.");

            contract.Status = ContractStatus.Active;
            contract.ApprovedBy = currentUserService.UserId;
            contract.ApprovedAt = dateTime.Now;
            await context.SaveChangesAsync(cancellationToken);

            return Result<bool>.CreateSuccess(true, "Contract approved successfully.");
        }
    }
}

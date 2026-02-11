using Domain.Entities.MedalionData.Gold;
using Domain.Enums;
using SG.Common;

namespace Application.Features.Settlement.Commands
{
    public class CreateManualSettlementCommand : IRequest<Result<int>>
    {
        public int CaseId { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class CreateManualSettlementCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IDateTime dateTime) :
        IRequestHandler<CreateManualSettlementCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(CreateManualSettlementCommand request, CancellationToken cancellationToken)
        {
            var caseEntity = await context.ExceptionCases
                .FindAsync(new object[] { request.CaseId }, cancellationToken);

            if (caseEntity == null)
                return Result<int>.CreateFailure("Case not found.");

            if (caseEntity.Status == CaseStatus.Closed)
                return Result<int>.CreateFailure("Case is already closed.");

            if (!caseEntity.InternalTransactionId.HasValue || !caseEntity.ExternalTransactionId.HasValue)
                return Result<int>.CreateFailure("Case does not have both internal and external transactions linked.");

            var internalPayment = await context.InternalPayments
                .FindAsync(new object[] { caseEntity.InternalTransactionId.Value }, cancellationToken);
            var externalPayment = await context.ExternalPayments
                .FindAsync(new object[] { caseEntity.ExternalTransactionId.Value }, cancellationToken);

            if (internalPayment == null || externalPayment == null)
                return Result<int>.CreateFailure("One or both linked transactions could not be found.");

            // Check no existing Successful settlement already links these two payments
            var existingSettlement = await context.PspSettlements
                .AnyAsync(s => s.InternalPaymentId == internalPayment.Id
                    && s.ExternalPaymentId == externalPayment.Id
                    && s.ReconciliationStatus == ReconciliationStatus.Successful, cancellationToken);

            if (existingSettlement)
                return Result<int>.CreateFailure("These transactions are already successfully settled.");

            var displayName = currentUserService.DisplayName ?? currentUserService.UserId ?? "System";

            var settlement = new PspSettlement
            {
                InternalPaymentId = internalPayment.Id,
                ExternalPaymentId = externalPayment.Id,
                ReconciliationRunId = caseEntity.ReconciliationRunId ?? 0,
                PspId = externalPayment.PspId,
                CurrencyCode = externalPayment.CurrencyCode,
                Amount = (int)externalPayment.Amount,
                TxDate = externalPayment.TxDate,
                TotalFees = 0,
                NetSettlement = externalPayment.Amount,
                ReconciliationStatus = ReconciliationStatus.Successful,
                ReconciliationComments = new List<ReconciliationComment>
                {
                    new()
                    {
                        Text = $"Manual settlement by {displayName}: {request.Description}",
                        Created = dateTime.Now,
                        UserId = currentUserService.UserId ?? "System"
                    }
                },
                Created = dateTime.Now,
                UserId = currentUserService.UserId ?? "System"
            };

            context.PspSettlements.Add(settlement);

            // Close the case
            caseEntity.Status = CaseStatus.Closed;
            caseEntity.ResolvedAt = dateTime.Now;
            caseEntity.ResolvedBy = displayName;
            caseEntity.ResolutionNotes = $"Manual settlement: {request.Description}";
            caseEntity.LastModified = dateTime.Now;
            caseEntity.LastModifiedBy = currentUserService.UserId;

            await context.SaveChangesAsync(cancellationToken);

            return Result<int>.CreateSuccess(settlement.Id, "Manual settlement created and case closed successfully.");
        }
    }
}

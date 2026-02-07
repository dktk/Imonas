using Domain.Enums;
using SG.Common;

namespace Application.Features.Cases.Commands
{
    public class CloseCaseCommand : IRequest<Result<bool>>
    {
        public int CaseId { get; set; }
        public string? ResolutionNotes { get; set; }
    }

    public class CloseCaseCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IDateTime dateTime) :
        IRequestHandler<CloseCaseCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(CloseCaseCommand request, CancellationToken cancellationToken)
        {
            var caseEntity = await context.ExceptionCases.FindAsync(new object[] { request.CaseId }, cancellationToken);

            if (caseEntity == null)
                return Result<bool>.CreateFailure(new[] { "Case not found." });

            caseEntity.Status = CaseStatus.Closed;
            caseEntity.ResolvedAt = dateTime.Now;
            caseEntity.ResolvedBy = currentUserService.UserId;
            caseEntity.ResolutionNotes = request.ResolutionNotes;
            caseEntity.LastModified = dateTime.Now;
            caseEntity.LastModifiedBy = currentUserService.UserId;

            await context.SaveChangesAsync(cancellationToken);

            return Result<bool>.CreateSuccess(true, $"Case {caseEntity.CaseNumber} closed successfully.");
        }
    }
}

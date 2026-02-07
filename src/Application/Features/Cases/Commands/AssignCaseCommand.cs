using Domain.Enums;
using SG.Common;

namespace Application.Features.Cases.Commands
{
    public class AssignCaseCommand : IRequest<Result<bool>>
    {
        public int CaseId { get; set; }
        public string? AssignedTo { get; set; }
    }

    public class AssignCaseCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IDateTime dateTime) :
        IRequestHandler<AssignCaseCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(AssignCaseCommand request, CancellationToken cancellationToken)
        {
            var caseEntity = await context.ExceptionCases.FindAsync(new object[] { request.CaseId }, cancellationToken);

            if (caseEntity == null)
                return Result<bool>.CreateFailure(new[] { "Case not found." });

            caseEntity.AssignedTo = request.AssignedTo ?? currentUserService.UserId;
            caseEntity.Status = CaseStatus.InProgress;
            caseEntity.LastModified = dateTime.Now;
            caseEntity.LastModifiedBy = currentUserService.UserId;

            await context.SaveChangesAsync(cancellationToken);

            return Result<bool>.CreateSuccess(true, $"Case assigned to {caseEntity.AssignedTo}.");
        }
    }
}

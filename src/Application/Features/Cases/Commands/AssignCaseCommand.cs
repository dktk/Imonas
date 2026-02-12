using SG.Common;

namespace Application.Features.Cases.Commands
{
    public class AssignCaseCommand : IRequest<Result<string>>
    {
        public int CaseId { get; set; }
        public string? AssignedTo { get; set; }
    }

    public class AssignCaseCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IDateTime dateTime) :
        IRequestHandler<AssignCaseCommand, Result<string>>
    {
        public async Task<Result<string>> Handle(AssignCaseCommand request, CancellationToken cancellationToken)
        {
            var caseEntity = await context.ExceptionCases.FindAsync(new object[] { request.CaseId }, cancellationToken);

            if (caseEntity == null)
            {
                return Result<string>.CreateFailure(new[] { "Case not found." });
            }

            var assignedToId = await context.AspNetUsers
                                        .Where(x => x.DisplayName == request.AssignedTo)
                                        .Select(x => x.Id)
                                        .FirstOrDefaultAsync(cancellationToken);

            if (assignedToId.IsNullOrWhiteSpace())
            {
                return Result<string>.CreateFailure(new[] { $"User '{request.AssignedTo}' can not be found." });
            }

            caseEntity.AssignedToId = assignedToId;

            caseEntity.Status = CaseStatus.InProgress;
            caseEntity.LastModified = dateTime.Now;
            caseEntity.LastModifiedBy = currentUserService.UserId;

            context.UserNotifications.Add(new UserNotification
            {
                RecipientUserId = assignedToId,               
                SenderUserId = currentUserService.UserId,
                SenderDisplayName = currentUserService.DisplayName,
                LinkUrl = "/cases/details/" + request.CaseId,
                Message = $"You have been assigned a new case"
            });

            await context.SaveChangesAsync(cancellationToken);

            return Result<string>.CreateSuccess(assignedToId, $"Case assigned to {request.AssignedTo}.");
        }
    }
}

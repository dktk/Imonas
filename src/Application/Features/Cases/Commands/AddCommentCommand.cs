using Domain.Entities.Cases;
using SG.Common;

namespace Application.Features.Cases.Commands
{
    public class AddCommentCommand : IRequest<Result<bool>>
    {
        public int CaseId { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public class AddCommentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IDateTime dateTime) :
        IRequestHandler<AddCommentCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(AddCommentCommand request, CancellationToken cancellationToken)
        {
            var caseEntity = await context.ExceptionCases.FindAsync(new object[] { request.CaseId }, cancellationToken);

            if (caseEntity == null)
                return Result<bool>.CreateFailure(new[] { "Case not found." });

            var comment = new CaseComment
            {
                CaseId = request.CaseId,
                Comment = request.Comment,
                CommentedBy = currentUserService.UserId ?? "System",
                Created = dateTime.Now,
                UserId = currentUserService.UserId ?? "System"
            };

            context.CaseComments.Add(comment);
            await context.SaveChangesAsync(cancellationToken);

            return Result<bool>.CreateSuccess(true, "Comment added successfully.");
        }
    }
}

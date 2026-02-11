using SG.Common;

namespace Application.Features.Cases.Commands
{
    public class DeleteAttachmentCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }

    public class DeleteAttachmentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService) :
        IRequestHandler<DeleteAttachmentCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteAttachmentCommand request, CancellationToken cancellationToken)
        {
            var attachment = await context.CaseAttachments.FindAsync(new object[] { request.Id }, cancellationToken);

            if (attachment == null)
                return Result<bool>.CreateFailure(new[] { "Attachment not found." });

            context.CaseAttachments.Remove(attachment);
            await context.SaveChangesAsync(cancellationToken);

            return Result<bool>.CreateSuccess(true, "Attachment deleted successfully.");
        }
    }
}

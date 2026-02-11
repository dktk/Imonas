using Domain.Entities.Cases;
using SG.Common;

namespace Application.Features.Cases.Commands
{
    public class AddAttachmentCommand : IRequest<Result<int>>
    {
        public int CaseId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
    }

    public class AddAttachmentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IDateTime dateTime) :
        IRequestHandler<AddAttachmentCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(AddAttachmentCommand request, CancellationToken cancellationToken)
        {
            var caseEntity = await context.ExceptionCases.FindAsync(new object[] { request.CaseId }, cancellationToken);

            if (caseEntity == null)
                return Result<int>.CreateFailure(new[] { "Case not found." });

            var attachment = new CaseAttachment
            {
                CaseId = request.CaseId,
                FileName = request.FileName,
                FileSizeBytes = request.FileSizeBytes,
                ContentType = request.ContentType,
                FileContent = request.FileContent,
                UploadedBy = currentUserService.DisplayName ?? currentUserService.UserId ?? "System",
                Created = dateTime.Now,
                UserId = currentUserService.UserId ?? "System"
            };

            context.CaseAttachments.Add(attachment);
            await context.SaveChangesAsync(cancellationToken);

            return Result<int>.CreateSuccess(attachment.Id, "File uploaded successfully.");
        }
    }
}

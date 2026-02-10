using Application.Hubs;
using Application.Hubs.Constants;
using Domain.Entities.Cases;
using Microsoft.AspNetCore.SignalR;
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
        IDateTime dateTime,
        IHubContext<SignalRHub> hubContext) :
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
                CommentedBy = currentUserService.DisplayName ?? currentUserService.UserId ?? "System",
                Created = dateTime.Now,
                UserId = currentUserService.UserId ?? "System"
            };

            context.CaseComments.Add(comment);
            await context.SaveChangesAsync(cancellationToken);

            // todo: check this
            // Parse @mentions and create notifications
            if (request.Comment.Contains('@'))
            {
                var users = await context.AspNetUsers
                    .Where(u => u.IsActive && u.DisplayName != null)
                    .Select(u => new { u.Id, u.DisplayName })
                    .ToListAsync(cancellationToken);

                var senderName = currentUserService.DisplayName ?? "Someone";
                var mentionedUsers = users
                    .Where(u => request.Comment.Contains($"@{u.DisplayName}", StringComparison.OrdinalIgnoreCase))
                    .Where(u => u.Id != currentUserService.UserId)
                    .ToList();

                if (mentionedUsers.Any())
                {
                    foreach (var user in mentionedUsers)
                    {
                        var notification = new UserNotification
                        {
                            RecipientUserId = user.Id,
                            SenderUserId = currentUserService.UserId ?? "System",
                            SenderDisplayName = senderName,
                            Message = $"mentioned you in a comment on case {caseEntity.CaseNumber}",
                            LinkUrl = $"/Cases/Details/{request.CaseId}#panel-comments",
                            IsRead = false,
                            Created = dateTime.Now,
                            UserId = currentUserService.UserId ?? "System"
                        };
                        context.UserNotifications.Add(notification);
                    }

                    await context.SaveChangesAsync(cancellationToken);

                    // Push real-time SignalR notifications
                    foreach (var user in mentionedUsers)
                    {
                        await hubContext.Clients.User(user.Id).SendAsync(
                            SignalR.ReceiveMentionNotification,
                            new
                            {
                                senderName,
                                message = $"mentioned you in a comment on case {caseEntity.CaseNumber}",
                                linkUrl = $"/Cases/Details/{request.CaseId}#panel-comments",
                                caseNumber = caseEntity.CaseNumber
                            },
                            cancellationToken);
                    }
                }
            }

            return Result<bool>.CreateSuccess(true, "Comment added successfully.");
        }
    }
}

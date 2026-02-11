using SG.Common;

namespace Application.Features.UserNotifications.Commands
{
    public class MarkNotificationReadCommand : IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }

    public class MarkNotificationReadCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IDateTime dateTime) :
        IRequestHandler<MarkNotificationReadCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(
            MarkNotificationReadCommand request, CancellationToken cancellationToken)
        {
            var notification = await context.UserNotifications
                .FirstOrDefaultAsync(n => n.Id == request.Id
                    && n.RecipientUserId == currentUserService.UserId, cancellationToken);

            if (notification == null)
                return Result<bool>.CreateFailure(new[] { "Notification not found." });

            notification.IsRead = true;
            notification.ReadAt = dateTime.Now;
            await context.SaveChangesAsync(cancellationToken);

            return Result<bool>.CreateSuccess(true);
        }
    }

    public class MarkAllNotificationsReadCommand : IRequest<Result<bool>> { }

    public class MarkAllNotificationsReadCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IDateTime dateTime) :
        IRequestHandler<MarkAllNotificationsReadCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(
            MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
        {
            var unread = await context.UserNotifications
                .Where(n => n.RecipientUserId == currentUserService.UserId && !n.IsRead)
                .ToListAsync(cancellationToken);

            foreach (var n in unread)
            {
                n.IsRead = true;
                n.ReadAt = dateTime.Now;
            }

            await context.SaveChangesAsync(cancellationToken);
            return Result<bool>.CreateSuccess(true);
        }
    }
}

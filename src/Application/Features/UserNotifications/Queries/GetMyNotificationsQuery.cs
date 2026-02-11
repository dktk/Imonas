using Application.Features.UserNotifications.DTOs;

namespace Application.Features.UserNotifications.Queries
{
    public class GetMyNotificationsQuery : IRequest<List<UserNotificationDto>>
    {
        // todo: configuration setting
        public int Take { get; set; } = 20;
    }

    public class GetMyNotificationsQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ICurrentUserService currentUserService) :
        IRequestHandler<GetMyNotificationsQuery, List<UserNotificationDto>>
    {
        public async Task<List<UserNotificationDto>> Handle(
            GetMyNotificationsQuery request, CancellationToken cancellationToken)
        {
            return await context.UserNotifications
                .Where(n => n.RecipientUserId == currentUserService.UserId)
                .OrderByDescending(n => n.Created)
                .Take(request.Take)
                .ProjectTo<UserNotificationDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }

    public class GetUnreadNotificationCountQuery : IRequest<int> { }

    public class GetUnreadNotificationCountQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService) :
        IRequestHandler<GetUnreadNotificationCountQuery, int>
    {
        public async Task<int> Handle(
            GetUnreadNotificationCountQuery request, CancellationToken cancellationToken)
        {
            return await context.UserNotifications
                .CountAsync(n => n.RecipientUserId == currentUserService.UserId
                              && !n.IsRead, cancellationToken);
        }
    }
}

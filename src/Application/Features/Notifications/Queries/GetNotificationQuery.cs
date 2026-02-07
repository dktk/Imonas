// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Application.Features.Notifications.DTOs;

namespace Application.Features.Notifications.Queries
{
    public class GetNotificationQuery : IRequest<NotificationDto>
    {
        public int PageSize { get; set; } = 100;
    }

    public class GetNotificationQueryHandler(
        IApplicationDbContext context,
        IMapper mapper) :
        IRequestHandler<GetNotificationQuery, NotificationDto>
    {
        public async Task<NotificationDto> Handle(GetNotificationQuery request, CancellationToken cancellationToken)
        {
            return await context.Notifications
                .OrderByDescending(c => c.Created)
                .Take(request.PageSize)
                .ProjectTo<NotificationDto>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}

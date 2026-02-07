// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Application.Features.Notifications.DTOs;

using SG.Common;

namespace Application.Features.Notifications.Commands
{
    public class AddNotificationCommand : NotificationDto, IRequest<Result<int>>
    {

    }

    public class AddNotificationCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ResultSafeWrapper secureApplicationStore) :
        IRequestHandler<AddNotificationCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(AddNotificationCommand request, CancellationToken cancellationToken)
        {
            var result = await secureApplicationStore.ExecuteAsync(async () =>
            {
                if (request.Id > 0)
                {
                    var item = await context.Notifications.FindAsync(new object[] { request.Id }, cancellationToken);
                    item = mapper.Map(request, item);

                    await context.SaveChangesAsync(cancellationToken);

                    return Result<int>.CreateSuccess(item.Id);
                }
                else
                {
                    var item = mapper.Map<Notification>(request);
                    context.Notifications.Add(item);
                    await context.SaveChangesAsync(cancellationToken);

                    return Result<int>.CreateSuccess(item.Id);
                }
            }, request, "An error occured while saving a notification.");

            return result;
        }
    }
}

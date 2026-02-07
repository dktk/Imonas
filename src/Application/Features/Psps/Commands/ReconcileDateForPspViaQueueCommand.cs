using System.Text;

using Imonas.Exchange.Contracts;
using Application.Features.Psps.DTOs;

using Microsoft.Extensions.Options;

using SG.Common.Settings;
using SG.Common;

namespace Application.Features.Psps.Commands
{
    public class ReconcileDateForPspViaQueueCommand : ReconciliationDataDto, IRequest<Result<Guid>>
    {
        // todo: use this instead of ReconcileDateForPspCommand
        public class ReconcileDateForPspViaQueueCommandHandler(ILogger<ReconcileDateForPspViaQueueCommandHandler> logger,
            HttpClient client,
            IApplicationDbContext context,
            IOptions<PspDataGatheringSettings> settings,
            ISerilogsService serilogsService,
            ICurrentUserService currentUserService) : IRequestHandler<ReconcileDateForPspViaQueueCommand, Result<Guid>>
        {
            public async Task<Result<Guid>> Handle(ReconcileDateForPspViaQueueCommand request, CancellationToken cancellationToken)
            {
                var psp = await context.Psps
                                        .Where(x => x.Id == request.PspId)
                                        .Select(x => x.Name)
                                        .FirstOrDefaultAsync(cancellationToken);

                var messageId = Guid.NewGuid();
                var queueData = new QueuePspData(request.StartDate, request.EndDate, request.PspId, request.ExternalSystem, messageId);
                var jsonData = queueData.ToJson();
                HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                try
                {
                    var response = await client.PostAsync(settings.Value.BaseUrl, content, cancellationToken);

                    await serilogsService.AddInfo($"Getting data for {psp} for the {request.StartDate.ToShortDateString()} - {messageId}", currentUserService.DisplayName);

                    return Result<Guid>.CreateSuccess(messageId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, jsonData);

                    await serilogsService.AddError($"Failed to get {psp} data for the {request.StartDate.ToShortDateString()} - {messageId}", currentUserService.DisplayName, ex);
                }

                return Result<Guid>.CreateFailure([$"An error occured while getting the {psp} for {request.StartDate.ToShortDateString()}."], messageId.ToString());
            }
        }
    }
}

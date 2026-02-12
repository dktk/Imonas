using Domain.Entities.Configuration;
using SG.Common;

namespace Application.Features.Psps.Commands
{
    public class SavePspConfigurationCommand : IRequest<Result<int>>
    {
        public int PspId { get; set; }
        public string ConfigJson { get; set; } = "{}";
    }

    public class SavePspConfigurationCommandHandler(
        IApplicationDbContext context) :
        IRequestHandler<SavePspConfigurationCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(
            SavePspConfigurationCommand request, CancellationToken cancellationToken)
        {
            var psp = await context.Psps
                .FirstOrDefaultAsync(p => p.Id == request.PspId, cancellationToken);

            if (psp == null)
                return Result<int>.BuildFailure("PSP not found.");

            var config = await context.PspConfigurations
                .FirstOrDefaultAsync(c => c.PspId == request.PspId, cancellationToken);

            if (config == null)
            {
                config = new PspConfiguration
                {
                    PspId = request.PspId,
                    ConfigJson = request.ConfigJson
                };
                context.PspConfigurations.Add(config);
            }
            else
            {
                config.ConfigJson = request.ConfigJson;
            }

            await context.SaveChangesAsync(cancellationToken);
            return Result<int>.BuildSuccess(config.Id);
        }
    }
}

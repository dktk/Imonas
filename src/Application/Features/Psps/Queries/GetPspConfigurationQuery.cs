namespace Application.Features.Psps.Queries
{
    public class GetPspConfigurationQuery : IRequest<PspConfigurationDto?>
    {
        public int PspId { get; set; }
    }

    public class PspConfigurationDto
    {
        public int Id { get; set; }
        public int PspId { get; set; }
        public string PspName { get; set; } = string.Empty;
        public string PspCode { get; set; } = string.Empty;
        public bool IsCsvBased { get; set; }
        public string ConfigJson { get; set; } = "{}";
    }

    public class GetPspConfigurationQueryHandler(
        IApplicationDbContext context) :
        IRequestHandler<GetPspConfigurationQuery, PspConfigurationDto?>
    {
        public async Task<PspConfigurationDto?> Handle(
            GetPspConfigurationQuery request, CancellationToken cancellationToken)
        {
            var psp = await context.Psps
                .FirstOrDefaultAsync(p => p.Id == request.PspId, cancellationToken);

            if (psp == null) return null;

            var config = await context.PspConfigurations
                .FirstOrDefaultAsync(c => c.PspId == request.PspId, cancellationToken);

            return new PspConfigurationDto
            {
                Id = config?.Id ?? 0,
                PspId = psp.Id,
                PspName = psp.Name,
                PspCode = psp.Code,
                IsCsvBased = psp.IsCsvBased,
                ConfigJson = config?.ConfigJson ?? "{}"
            };
        }
    }
}

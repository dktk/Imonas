using Application.Features.Configuration.FeeContracts.DTOs;

using Domain.Entities.Configuration;

using SG.Common;

namespace Application.Features.Configuration.FeeContracts.Commands
{
    public class CreateFeeContractCommand : IRequest<Result<FeeContractDto>>
    {
        public int PspId { get; set; }
        public string ContractName { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public FeeStructureType FeeStructure { get; set; }
        public decimal? FixedFee { get; set; }
        public decimal? PercentageFee { get; set; }
        public decimal? MinimumFee { get; set; }
        public decimal? MaximumFee { get; set; }
        public string? TieredStructure { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string Version { get; set; } = string.Empty;
        public bool SubmitForApproval { get; set; }
    }

    public class CreateFeeContractCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IDateTime dateTime) :
        IRequestHandler<CreateFeeContractCommand, Result<FeeContractDto>>
    {
        public async Task<Result<FeeContractDto>> Handle(CreateFeeContractCommand request, CancellationToken cancellationToken)
        {
            var contract = new FeeContract
            {
                PspId = request.PspId,
                ContractName = request.ContractName,
                PaymentMethod = request.PaymentMethod,
                Brand = request.Brand,
                Currency = request.Currency,
                FeeStructure = request.FeeStructure,
                FixedFee = request.FixedFee,
                PercentageFee = request.PercentageFee,
                MinimumFee = request.MinimumFee,
                MaximumFee = request.MaximumFee,
                TieredStructure = request.TieredStructure,
                EffectiveFrom = request.EffectiveFrom,
                EffectiveTo = request.EffectiveTo,
                Version = request.Version,
                Status = request.SubmitForApproval ? ContractStatus.PendingApproval : ContractStatus.Draft
            };

            context.FeeContracts.Add(contract);
            await context.SaveChangesAsync(cancellationToken);

            var dto = mapper.Map<FeeContractDto>(contract);
            dto.PspName = await context.Psps
                .Where(p => p.Id == contract.PspId)
                .Select(p => p.Name)
                .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

            var action = request.SubmitForApproval ? "created and submitted for approval" : "saved as draft";
            return Result<FeeContractDto>.CreateSuccess(dto, $"Fee contract '{contract.ContractName}' {action}.");
        }
    }
}

using Application.Features.Configuration.FeeContracts.DTOs;

using SG.Common;

namespace Application.Features.Configuration.FeeContracts.Commands
{
    public class UpdateFeeContractCommand : IRequest<Result<FeeContractDto>>
    {
        public int Id { get; set; }
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

    public class UpdateFeeContractCommandHandler(
        IApplicationDbContext context,
        IMapper mapper) :
        IRequestHandler<UpdateFeeContractCommand, Result<FeeContractDto>>
    {
        public async Task<Result<FeeContractDto>> Handle(UpdateFeeContractCommand request, CancellationToken cancellationToken)
        {
            var contract = await context.FeeContracts
                .Include(f => f.Psp)
                .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

            if (contract == null)
                return Result<FeeContractDto>.CreateFailure("Fee contract not found.");

            if (contract.Status != ContractStatus.Draft)
                return Result<FeeContractDto>.CreateFailure("Only draft contracts can be modified.");

            contract.ContractName = request.ContractName;
            contract.PaymentMethod = request.PaymentMethod;
            contract.Brand = request.Brand;
            contract.Currency = request.Currency;
            contract.FeeStructure = request.FeeStructure;
            contract.FixedFee = request.FixedFee;
            contract.PercentageFee = request.PercentageFee;
            contract.MinimumFee = request.MinimumFee;
            contract.MaximumFee = request.MaximumFee;
            contract.TieredStructure = request.TieredStructure;
            contract.EffectiveFrom = request.EffectiveFrom;
            contract.EffectiveTo = request.EffectiveTo;
            contract.Version = request.Version;

            if (request.SubmitForApproval)
                contract.Status = ContractStatus.PendingApproval;

            await context.SaveChangesAsync(cancellationToken);

            var dto = mapper.Map<FeeContractDto>(contract);
            dto.PspName = contract.Psp?.Name ?? string.Empty;

            var action = request.SubmitForApproval ? "updated and submitted for approval" : "updated";
            return Result<FeeContractDto>.CreateSuccess(dto, $"Fee contract '{contract.ContractName}' {action}.");
        }
    }
}

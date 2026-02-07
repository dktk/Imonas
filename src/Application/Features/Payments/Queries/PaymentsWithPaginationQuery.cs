using Domain.Entities.MedalionData.Silver;

namespace Application.Features.Payments.Queries
{
    public class PaymentsWithPaginationQuery : PaginationRequest, IRequest<PaginatedData<PaymentDto>>
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int SelectedPspId { get; set; }
        public PaginationRequest PaginationInfo { get; set; }
    }

    public class PaymentDto : IMapFrom<ExternalPayment>
    {
        public int Id { get; set; }
        public string PspName { get; set; }
        public string TxId { get; set; }
        public string SourceSystem { get; set; }
        public string PlayerId { get; set; }
        public string BrandId { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public string Status { get; set; }
        public string Action { get; set; }
        public DateTime TxDate { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<ExternalPayment, PaymentDto>()
                .ForMember(d => d.Action, opt => opt.MapFrom(s => s.Action.ToString()));

            profile.CreateMap<ExternalPayment, PaymentDto>()
                .ForMember(d => d.PspName, opt => opt.MapFrom(s => s.Psp.Name));

            profile.CreateMap<ExternalPayment, PaymentDto>()
                .ForMember(d => d.SourceSystem, opt => opt.MapFrom(s => s.ExternalSystem));
        }
    }

    public class PaymentsWithPaginationQueryHandler(
            IApplicationDbContext context,
            IMapper mapper) : IRequestHandler<PaymentsWithPaginationQuery, PaginatedData<PaymentDto>>
    {
        public async Task<PaginatedData<PaymentDto>> Handle(PaymentsWithPaginationQuery request, CancellationToken cancellationToken)
        {
            var filters = PredicateBuilder.FromFilter<ExternalPayment>(request.FilterRules);

            var rowsToSkip = (request.Page - 1) * request.Rows;
            if (rowsToSkip < 0)
            {
                rowsToSkip = 0;
            }

            var data = await context.ExternalPayments
                .Include(ep => ep.Psp)
                .Where(x => x.PspId == request.SelectedPspId && x.TxDate >= request.StartDate && x.TxDate <= request.EndDate)
                .Where(filters)
                .OrderBy($"{request.Sort} {request.Order}")
                .ProjectTo<PaymentDto>(mapper.ConfigurationProvider)
                .PaginatedDataAsync(request.Page, request.Rows);

            return data;
        }
    }
}

using Application.Features.Cases.DTOs;

namespace Application.Features.Cases.Queries
{
    public class GetCaseByIdQuery : IRequest<ExceptionCaseDetailsDto?>
    {
        public int Id { get; set; }
    }

    public class GetCaseByIdQueryHandler(
        IApplicationDbContext context,
        IMapper mapper) :
        IRequestHandler<GetCaseByIdQuery, ExceptionCaseDetailsDto?>
    {
        public async Task<ExceptionCaseDetailsDto?> Handle(GetCaseByIdQuery request, CancellationToken cancellationToken)
        {
            var caseEntity = await context.ExceptionCases
                .Include(c => c.ReconciliationRun)
                .Include(c => c.AssignedTo)
                .Include(c => c.Comments)
                .Include(c => c.Attachments)
                .Include(c => c.Labels)
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (caseEntity == null)
                return null;

            var result = mapper.Map<ExceptionCaseDetailsDto>(caseEntity);
            var internalPayment = await context.InternalPayments.FirstOrDefaultAsync(x => x.Id == result.InternalTransactionId);
            var externalPayment = await context.ExternalPayments.Include(x => x.Psp).FirstOrDefaultAsync(x => x.Id == result.ExternalTransactionId);
            
            result.ExternalTransaction = new ExceptionCaseDetailsDto.TransactionDto
            {
                Amount = externalPayment.Amount,
                CurrencyCode = externalPayment.CurrencyCode,
                ExternalPaymentId = externalPayment.ExternalPaymentId,
                Id = result.ExternalTransactionId,
                Source = externalPayment.Psp.Name,
                Status = externalPayment.Status,
                TransactionDate = externalPayment.TxDate
            };

            result.InternalTransaction = new ExceptionCaseDetailsDto.TransactionDto
            {
                Amount = internalPayment.Amount,
                CurrencyCode = internalPayment.CurrencyCode,
                Id = result.InternalTransactionId,
                Source = internalPayment.System,
                Status = internalPayment.Status,
                TransactionDate = internalPayment.TxDate.Date,
                ExternalPaymentId = internalPayment.ProviderTxId
            };

            return result;
        }
    }
}

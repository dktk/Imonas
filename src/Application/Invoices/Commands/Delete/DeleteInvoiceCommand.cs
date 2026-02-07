using SG.Common;

namespace Application.Invoices.Commands.Delete
{
    public class DeleteInvoiceCommand: IRequest<Result<bool>>
    {
      public int Id {  get; set; }
    }
    public class DeleteCheckedInvoicesCommand : IRequest<Result<bool>>
    {
      public int[] Id {  get; set; }
    }

    public class DeleteInvoiceCommandHandler : 
                 IRequestHandler<DeleteInvoiceCommand,Result<bool>>,
                 IRequestHandler<DeleteCheckedInvoicesCommand,Result<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<DeleteInvoiceCommandHandler> _localizer;
        public DeleteInvoiceCommandHandler(
            IApplicationDbContext context,
            IStringLocalizer<DeleteInvoiceCommandHandler> localizer,
             IMapper mapper
            )
        {
            _context = context;
            _localizer = localizer;
            _mapper = mapper;
        }
        public async Task<Result<bool>> Handle(DeleteInvoiceCommand request, CancellationToken cancellationToken)
        {
           var item = await _context.Invoices.FindAsync(new object[] { request.Id }, cancellationToken);
            _context.Invoices.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.CreateSuccess(true);
        }

        public async Task<Result<bool>> Handle(DeleteCheckedInvoicesCommand request, CancellationToken cancellationToken)
        {
           var items = await _context.Invoices.Where(x => request.Id.Contains(x.Id)).ToListAsync(cancellationToken);
            foreach (var item in items)
            {
                _context.Invoices.Remove(item);
            }
            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.CreateSuccess(true);
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Application.Customers.Caching;

using SG.Common;

namespace Application.Customers.Commands.Delete
{
    public class DeleteCustomerCommand: IRequest<Result<bool>>, ICacheInvalidator
    {
        public int Id { get; set; }
        public string CacheKey => CustomerCacheKey.GetAllCacheKey;

        public CancellationTokenSource SharedExpiryTokenSource => CustomerCacheTokenSource.ResetCacheToken;
    }
    public class DeleteCheckedCustomersCommand : IRequest<Result<bool>>, ICacheInvalidator
    {
        public int[] Id { get; set; }
        public string CacheKey => CustomerCacheKey.GetAllCacheKey;

        public CancellationTokenSource ResetCacheToken => CustomerCacheTokenSource.ResetCacheToken;
    }

    public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, Result<bool>>,
        IRequestHandler<DeleteCheckedCustomersCommand, Result<bool>>
    {
        private readonly IApplicationDbContext _context;

        public DeleteCustomerCommandHandler(
            IApplicationDbContext context
            )
        {
            _context = context;
        }
        public async Task<Result<bool>> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
        {
            var item =await _context.Customers.FindAsync(new object[] { request.Id }, cancellationToken);
            _context.Customers.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.CreateSuccess(true);
        }

        public async Task<Result<bool>> Handle(DeleteCheckedCustomersCommand request, CancellationToken cancellationToken)
        {
            var items = await _context.Customers.Where(x => request.Id.Contains(x.Id)).ToListAsync(cancellationToken);
            foreach(var item in items)
            {
                _context.Customers.Remove(item);
            }
            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.CreateSuccess(true);
        }
    }
}

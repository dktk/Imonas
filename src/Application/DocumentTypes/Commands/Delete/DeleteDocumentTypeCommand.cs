// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Application.DocumentTypes.Caching;

using SG.Common;

namespace Application.DocumentTypes.Commands.Delete
{
    public class DeleteDocumentTypeCommand: IRequest<Result<bool>>, ICacheInvalidator
    {
        public int Id { get; set; }

        public string CacheKey => DocumentTypeCacheKey.GetAllCacheKey;

        public CancellationTokenSource SharedExpiryTokenSource => DocumentTypeCacheTokenSource.ResetCacheToken;
    }
    public class DeleteCheckedDocumentTypesCommand : IRequest<Result<bool>>, ICacheInvalidator
    {
        public int[] Id { get; set; }

        public string CacheKey => DocumentTypeCacheKey.GetAllCacheKey;

        public CancellationTokenSource SharedExpiryTokenSource => DocumentTypeCacheTokenSource.ResetCacheToken;
    }

    public class DeleteDocumentTypeCommandHandler : IRequestHandler<DeleteDocumentTypeCommand,Result<bool>>,
        IRequestHandler<DeleteCheckedDocumentTypesCommand,Result<bool>>
    {
        private readonly IApplicationDbContext _context;

        public DeleteDocumentTypeCommandHandler(
            IApplicationDbContext context
            )
        {
            _context = context;
        }
        public async Task<Result<bool>> Handle(DeleteDocumentTypeCommand request, CancellationToken cancellationToken)
        {
            var item =await _context.DocumentTypes.FindAsync(new object[] { request.Id }, cancellationToken);
            _context.DocumentTypes.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.CreateSuccess(true);
        }

        public async Task<Result<bool>> Handle(DeleteCheckedDocumentTypesCommand request, CancellationToken cancellationToken)
        {
            var items = await _context.DocumentTypes.Where(x => request.Id.Contains(x.Id)).ToListAsync(cancellationToken);
            foreach(var item in items)
            {
                _context.DocumentTypes.Remove(item);
            }
            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.CreateSuccess(true);
        }
    }
}

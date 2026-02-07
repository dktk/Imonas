// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using SG.Common;

namespace Application.Documents.Commands.Delete
{
    public class DeleteDocumentCommand: IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }
    public class DeleteCheckedDocumentsCommand : IRequest<Result<bool>>
    {
        public int[] Id { get; set; }
    }

    public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand,Result<bool>>,
        IRequestHandler<DeleteCheckedDocumentsCommand,Result<bool>>
    {
        private readonly IApplicationDbContext _context;

        public DeleteDocumentCommandHandler(
            IApplicationDbContext context
            )
        {
            _context = context;
        }
        public async Task<Result<bool>> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
        {
            var item =await _context.Documents.FindAsync(new object[] { request.Id },cancellationToken);
            _context.Documents.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.CreateSuccess(true);
        }

        public async Task<Result<bool>> Handle(DeleteCheckedDocumentsCommand request, CancellationToken cancellationToken)
        {
            var items = await _context.Documents.Where(x => request.Id.Contains(x.Id)).ToListAsync(cancellationToken);
            foreach(var item in items)
            {
                _context.Documents.Remove(item);
            }
            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.CreateSuccess(true);
        }
    }
}

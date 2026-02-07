// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using SG.Common;

namespace Application.KeyValues.Commands.Delete
{
    public class DeleteKeyValueCommand: IRequest<Result<bool>>
    {
        public int Id { get; set; }
    }
    public class DeleteCheckedKeyValuesCommand : IRequest<Result<bool>>
    {
        public int[] Id { get; set; }
    }

    public class DeleteKeyValueCommandHandler : IRequestHandler<DeleteKeyValueCommand,Result<bool>>,
        IRequestHandler<DeleteCheckedKeyValuesCommand,Result<bool>>
    {
        private readonly IApplicationDbContext _context;

        public DeleteKeyValueCommandHandler(
            IApplicationDbContext context
            )
        {
            _context = context;
        }
        public async Task<Result<bool>> Handle(DeleteKeyValueCommand request, CancellationToken cancellationToken)
        {
            var item =await _context.KeyValues.FindAsync(new object[] { request.Id }, cancellationToken);
            _context.KeyValues.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.CreateSuccess(true);
        }

        public async Task<Result<bool>> Handle(DeleteCheckedKeyValuesCommand request, CancellationToken cancellationToken)
        {
            var items = await _context.KeyValues.Where(x => request.Id.Contains(x.Id)).ToListAsync(cancellationToken);
            foreach(var item in items)
            {
                _context.KeyValues.Remove(item);
            }
            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.CreateSuccess(true);
        }
    }
}

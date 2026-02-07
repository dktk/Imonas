// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Application.Features.Psps.DTOs;

namespace Application.Features.Psps.Queries
{
    public class GetCsvBasedPspsQuery : IRequest<IEnumerable<PspDto>>
    {
    }

    public class GetCsvBasedPspsQueryHandler(
            IApplicationDbContext context,
            IMapper mapper) :
        IRequestHandler<GetCsvBasedPspsQuery, IEnumerable<PspDto>>
    {
        public async Task<IEnumerable<PspDto>> Handle(GetCsvBasedPspsQuery request, CancellationToken cancellationToken)
        {
            return await context.Psps
                            .Where(x => x.IsCsvBased && x.IsActive)
                            .OrderBy(x => x.Name)
                            .ProjectTo<PspDto>(mapper.ConfigurationProvider).ToListAsync(cancellationToken);
        }
    }
}

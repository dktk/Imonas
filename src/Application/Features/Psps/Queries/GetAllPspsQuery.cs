// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Application.Features.Psps.DTOs;

namespace Application.Features.Psps.Queries
{
    public class GetAllPspsQuery : IRequest<IEnumerable<PspDto>>
    {
    }

    public class GetAllPspsQueryHandler(
            IApplicationDbContext context,
            IMapper mapper) :
        IRequestHandler<GetAllPspsQuery, IEnumerable<PspDto>>
    {
        public async Task<IEnumerable<PspDto>> Handle(GetAllPspsQuery request, CancellationToken cancellationToken)
        {
            return await context.Psps
                            .OrderBy(x => x.Name)
                            .ProjectTo<PspDto>(mapper.ConfigurationProvider)
                            .ToListAsync(cancellationToken);
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Application.Features.Psps.DTOs;

namespace Application.Features.Psps.Queries
{
    public class GetInternalSystemsQuery : IRequest<List<InternalSystemDto>>
    {

    }

    public class GetInternalSystemsQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetInternalSystemsQuery, List<InternalSystemDto>>
    {
        public async Task<List<InternalSystemDto>> Handle(GetInternalSystemsQuery request, CancellationToken cancellationToken)
        {
            return await context.InternalSystems
                            .ProjectTo<InternalSystemDto>(mapper.ConfigurationProvider)
                            .ToListAsync(cancellationToken);
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Application.Features.Psps.DTOs;

namespace Application.Features.Psps.Queries
{
    public class PspsWithPaginationQuery : PaginationRequest, IRequest<PaginatedData<PspDto>>
    {

    }

    public class PspsWithPaginationQueryHandler(IApplicationDbContext context,
            IMapper mapper,
            IStringLocalizer<PspsWithPaginationQueryHandler> localizer) : IRequestHandler<PspsWithPaginationQuery, PaginatedData<PspDto>>
    {
        public async Task<PaginatedData<PspDto>> Handle(PspsWithPaginationQuery request, CancellationToken cancellationToken)
        {
            var filters = PredicateBuilder.FromFilter<Psp>(request.FilterRules);

            var data = await context.Psps.Where(filters)
                 .OrderBy($"{request.Sort} {request.Order}")
                 .ProjectTo<PspDto>(mapper.ConfigurationProvider)
                 .PaginatedDataAsync(request.Page, request.Rows);

            return data;
        }
    }
}

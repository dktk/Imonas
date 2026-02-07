// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Application.Customers.DTOs;
using Application.Common.Specification;
using Application.Customers.Caching;

namespace Application.Customers.Queries.PaginationQuery
{
    public class GetCustomersQuery : PaginationRequest,IRequest<PaginatedData<CustomerDto>>, ICacheable
    {
        public string UserId { get; set; }


        public string CacheKey => $"CustomersByMeQueryQuery,userid:{UserId},{this.ToString()}";

        public MemoryCacheEntryOptions Options => new MemoryCacheEntryOptions().AddExpirationToken(new CancellationChangeToken(CustomerCacheTokenSource.ResetCacheToken.Token));
    }
    public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, PaginatedData<CustomerDto>>
    {

        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetCustomersQueryHandler(
        
            IApplicationDbContext context,
            IMapper mapper
            )
        {
    
            _context = context;
            _mapper = mapper;
        }
        public async Task<PaginatedData<CustomerDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
        {
            var filters = PredicateBuilder.FromFilter<Customer>(request.FilterRules);
            var data = await _context.Customers.Specify(new GetCustomersQuerySpec(request.UserId))
                .Where(filters)
                .OrderBy($"{request.Sort} {request.Order}")
                .ProjectTo<CustomerDto>(_mapper.ConfigurationProvider)
                .PaginatedDataAsync(request.Page, request.Rows);

            return data;
        }

        public class GetCustomersQuerySpec : Specification<Customer>
        {
            public GetCustomersQuerySpec(string userId)
            {
                Criteria = p => p.UserId == userId;
            }
        }
    }
}

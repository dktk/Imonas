using Application.Common.Interfaces.Identity;
using Application.Features.Users.DTOs;

namespace Application.Features.Users.Queries
{
    public class GetUserStatsQuery : IRequest<UserStatsDto>
    {
    }

    public class GetUserStatsQueryHandler(IUserService userService) :
        IRequestHandler<GetUserStatsQuery, UserStatsDto>
    {
        public async Task<UserStatsDto> Handle(GetUserStatsQuery request, CancellationToken cancellationToken)
        {
            return await userService.GetUserStatsAsync();
        }
    }
}

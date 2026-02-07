using Application.Common.Interfaces.Identity;
using Application.Features.Users.DTOs;

namespace Application.Features.Users.Queries
{
    public class GetRolesQuery : IRequest<IEnumerable<RoleDto>>
    {
    }

    public class GetRolesQueryHandler(IUserService userService) :
        IRequestHandler<GetRolesQuery, IEnumerable<RoleDto>>
    {
        public async Task<IEnumerable<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
        {
            return await userService.GetRolesAsync();
        }
    }
}

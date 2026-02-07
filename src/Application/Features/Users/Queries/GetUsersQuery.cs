using Application.Common.Interfaces.Identity;
using Application.Features.Users.DTOs;

namespace Application.Features.Users.Queries
{
    public class GetUsersQuery : IRequest<IEnumerable<UserDto>>
    {
        public string? RoleFilter { get; set; }
        public string? StatusFilter { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class GetUsersQueryHandler(IUserService userService) :
        IRequestHandler<GetUsersQuery, IEnumerable<UserDto>>
    {
        public async Task<IEnumerable<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            return await userService.GetUsersAsync(request.SearchTerm, request.RoleFilter, request.StatusFilter);
        }
    }
}

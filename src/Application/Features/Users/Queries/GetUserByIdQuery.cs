using Application.Common.Interfaces.Identity;
using Application.Features.Users.DTOs;

namespace Application.Features.Users.Queries
{
    public class GetUserByIdQuery : IRequest<UserDetailsDto?>
    {
        public string UserId { get; set; } = string.Empty;
    }

    public class GetUserByIdQueryHandler(IUserService userService) :
        IRequestHandler<GetUserByIdQuery, UserDetailsDto?>
    {
        public async Task<UserDetailsDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            return await userService.GetUserByIdAsync(request.UserId);
        }
    }
}

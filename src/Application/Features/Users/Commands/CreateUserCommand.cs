using Application.Common.Interfaces.Identity;
using Application.Features.Users.DTOs;
using SG.Common;

namespace Application.Features.Users.Commands
{
    public class CreateUserCommand : IRequest<Result<UserDto>>
    {
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public bool IsActive { get; set; } = true;
        public bool EmailConfirmed { get; set; } = false;
    }

    public class CreateUserCommandHandler(IUserService userService) :
        IRequestHandler<CreateUserCommand, Result<UserDto>>
    {
        public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            return await userService.CreateUserAsync(
                request.Email,
                request.DisplayName,
                request.Password,
                request.Roles,
                request.IsActive,
                request.EmailConfirmed);
        }
    }
}

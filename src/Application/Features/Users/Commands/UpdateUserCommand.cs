using Application.Common.Interfaces.Identity;
using SG.Common;

namespace Application.Features.Users.Commands
{
    public class UpdateUserCommand : IRequest<Result<bool>>
    {
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class UpdateUserCommandHandler(IUserService userService) :
        IRequestHandler<UpdateUserCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            return await userService.UpdateUserAsync(
                request.UserId,
                request.DisplayName,
                request.Roles,
                request.IsActive);
        }
    }
}

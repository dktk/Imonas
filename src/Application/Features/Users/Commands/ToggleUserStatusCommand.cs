using Application.Common.Interfaces.Identity;
using SG.Common;

namespace Application.Features.Users.Commands
{
    public class ToggleUserStatusCommand : IRequest<Result<bool>>
    {
        public string UserId { get; set; } = string.Empty;
    }

    public class ToggleUserStatusCommandHandler(IUserService userService) :
        IRequestHandler<ToggleUserStatusCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(ToggleUserStatusCommand request, CancellationToken cancellationToken)
        {
            return await userService.ToggleUserStatusAsync(request.UserId);
        }
    }
}

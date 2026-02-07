using Application.Common.Interfaces.Identity;
using SG.Common;

namespace Application.Features.Users.Commands
{
    public class ResetPasswordCommand : IRequest<Result<string>>
    {
        public string UserId { get; set; } = string.Empty;
    }

    public class ResetPasswordCommandHandler(IUserService userService) :
        IRequestHandler<ResetPasswordCommand, Result<string>>
    {
        public async Task<Result<string>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            return await userService.ResetPasswordAsync(request.UserId);
        }
    }
}

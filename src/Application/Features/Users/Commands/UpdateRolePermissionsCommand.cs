using Application.Common.Interfaces.Identity;
using SG.Common;

namespace Application.Features.Users.Commands
{
    public class UpdateRolePermissionsCommand : IRequest<Result<bool>>
    {
        public string RoleName { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
    }

    public class UpdateRolePermissionsCommandHandler(IUserService userService) :
        IRequestHandler<UpdateRolePermissionsCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
        {
            return await userService.UpdateRolePermissionsAsync(
                request.RoleName,
                request.Permissions);
        }
    }
}

using Application.Common.Interfaces.Identity;
using SG.Common;

namespace Application.Features.Users.Commands
{
    public class CreateRoleCommand : IRequest<Result<bool>>
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<string> Permissions { get; set; } = new();
    }

    public class CreateRoleCommandHandler(IUserService userService) :
        IRequestHandler<CreateRoleCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            return await userService.CreateRoleAsync(
                request.Name,
                request.Description,
                request.Permissions);
        }
    }
}

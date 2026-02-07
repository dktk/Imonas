using Application.Common.Interfaces.Identity;
using SG.Common;

namespace Application.Features.Users.Commands
{
    public class SeedRolesCommand : IRequest<Result<int>>
    {
    }

    public class SeedRolesCommandHandler(IUserService userService) :
        IRequestHandler<SeedRolesCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(SeedRolesCommand request, CancellationToken cancellationToken)
        {
            return await userService.SeedRolesAsync();
        }
    }
}

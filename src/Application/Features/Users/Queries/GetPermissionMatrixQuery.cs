using Application.Common.Interfaces.Identity;
using Application.Features.Users.DTOs;

namespace Application.Features.Users.Queries
{
    public class GetPermissionMatrixQuery : IRequest<PermissionMatrixDto>
    {
    }

    public class GetPermissionMatrixQueryHandler(IUserService userService) :
        IRequestHandler<GetPermissionMatrixQuery, PermissionMatrixDto>
    {
        public async Task<PermissionMatrixDto> Handle(GetPermissionMatrixQuery request, CancellationToken cancellationToken)
        {
            return await userService.GetPermissionMatrixAsync();
        }
    }
}

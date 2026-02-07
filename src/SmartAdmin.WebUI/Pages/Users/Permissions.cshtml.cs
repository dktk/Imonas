using Application.Features.Users.Commands;
using Application.Features.Users.DTOs;
using Application.Features.Users.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace SmartAdmin.WebUI.Pages.Users
{
    [Authorize(Roles = "System Admin,Admin")]
    public class PermissionsModel(
        IStringLocalizer<PermissionsModel> localizer,
        ISender mediator) : PageModel
    {
        public PermissionMatrixDto Matrix { get; set; } = new();

        public async Task OnGetAsync()
        {
            Matrix = await mediator.Send(new GetPermissionMatrixQuery());
        }

        public async Task<IActionResult> OnPostUpdatePermissionsAsync(string roleName, List<string> permissions)
        {
            var command = new UpdateRolePermissionsCommand
            {
                RoleName = roleName,
                Permissions = permissions ?? new List<string>()
            };

            var result = await mediator.Send(command);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToPage();
        }
    }
}

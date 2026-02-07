using Application.Features.Users.Commands;
using Application.Features.Users.DTOs;
using Application.Features.Users.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;

namespace SmartAdmin.WebUI.Pages.Users
{
    [Authorize(Roles = "System Admin,Admin")]
    public class RolesModel(
        IStringLocalizer<RolesModel> localizer,
        ISender mediator) : PageModel
    {
        public IEnumerable<RoleDto> Roles { get; set; } = new List<RoleDto>();

        [BindProperty]
        public CreateRoleInputModel CreateInput { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public class CreateRoleInputModel
        {
            [Required]
            [StringLength(50)]
            [Display(Name = "Role Name")]
            public string Name { get; set; } = string.Empty;

            [StringLength(200)]
            [Display(Name = "Description")]
            public string? Description { get; set; }
        }

        public async Task OnGetAsync()
        {
            Roles = await mediator.Send(new GetRolesQuery());
        }

        public async Task<IActionResult> OnPostCreateRoleAsync()
        {
            if (!ModelState.IsValid)
            {
                Roles = await mediator.Send(new GetRolesQuery());
                return Page();
            }

            var command = new CreateRoleCommand
            {
                Name = CreateInput.Name,
                Description = CreateInput.Description
            };

            var result = await mediator.Send(command);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToPage();
            }

            ErrorMessage = result.Message;
            Roles = await mediator.Send(new GetRolesQuery());
            return Page();
        }

        public async Task<IActionResult> OnPostSeedRolesAsync()
        {
            var result = await mediator.Send(new SeedRolesCommand());

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

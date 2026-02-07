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
    public class EditModel(
        IStringLocalizer<EditModel> localizer,
        ISender mediator) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public UserDetailsDto? UserDetails { get; set; }
        public IEnumerable<RoleDto> AvailableRoles { get; set; } = new List<RoleDto>();

        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            public string Id { get; set; } = string.Empty;

            [Required]
            [Display(Name = "Display Name")]
            [StringLength(100)]
            public string DisplayName { get; set; } = string.Empty;

            [Display(Name = "Roles")]
            public List<string> SelectedRoles { get; set; } = new();

            [Display(Name = "Active")]
            public bool IsActive { get; set; } = true;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            UserDetails = await mediator.Send(new GetUserByIdQuery { UserId = id });
            if (UserDetails == null)
            {
                return NotFound();
            }

            Input = new InputModel
            {
                Id = UserDetails.Id,
                DisplayName = UserDetails.DisplayName,
                SelectedRoles = UserDetails.Roles.ToList(),
                IsActive = UserDetails.IsActive
            };

            AvailableRoles = await mediator.Send(new GetRolesQuery());
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                UserDetails = await mediator.Send(new GetUserByIdQuery { UserId = Input.Id });
                AvailableRoles = await mediator.Send(new GetRolesQuery());
                return Page();
            }

            var command = new UpdateUserCommand
            {
                UserId = Input.Id,
                DisplayName = Input.DisplayName,
                Roles = Input.SelectedRoles,
                IsActive = Input.IsActive
            };

            var result = await mediator.Send(command);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToPage("Index");
            }

            ErrorMessage = result.Message;
            UserDetails = await mediator.Send(new GetUserByIdQuery { UserId = Input.Id });
            AvailableRoles = await mediator.Send(new GetRolesQuery());
            return Page();
        }

        public async Task<IActionResult> OnPostResetPasswordAsync(string userId)
        {
            var result = await mediator.Send(new ResetPasswordCommand { UserId = userId });

            if (result.Success)
            {
                TempData["SuccessMessage"] = $"Password reset. New temporary password: {result.Value}";
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToPage(new { id = userId });
        }
    }
}

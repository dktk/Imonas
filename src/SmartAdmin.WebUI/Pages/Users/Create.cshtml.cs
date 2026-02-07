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
    public class CreateModel(
        IStringLocalizer<CreateModel> localizer,
        ISender mediator) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public IEnumerable<RoleDto> AvailableRoles { get; set; } = new List<RoleDto>();

        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required]
            [Display(Name = "Display Name")]
            [StringLength(100)]
            public string DisplayName { get; set; } = string.Empty;

            [Required]
            [StringLength(100, MinimumLength = 8)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirm Password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;

            [Display(Name = "Roles")]
            public List<string> SelectedRoles { get; set; } = new();

            [Display(Name = "Active")]
            public bool IsActive { get; set; } = true;

            [Display(Name = "Email Confirmed")]
            public bool EmailConfirmed { get; set; } = false;
        }

        public async Task OnGetAsync()
        {
            AvailableRoles = await mediator.Send(new GetRolesQuery());
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                AvailableRoles = await mediator.Send(new GetRolesQuery());
                return Page();
            }

            var command = new CreateUserCommand
            {
                Email = Input.Email,
                DisplayName = Input.DisplayName,
                Password = Input.Password,
                Roles = Input.SelectedRoles,
                IsActive = Input.IsActive,
                EmailConfirmed = Input.EmailConfirmed
            };

            var result = await mediator.Send(command);

            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToPage("Index");
            }

            ErrorMessage = result.Message;
            AvailableRoles = await mediator.Send(new GetRolesQuery());
            return Page();
        }
    }
}

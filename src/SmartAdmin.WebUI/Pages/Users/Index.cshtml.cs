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
    public class IndexModel(
        IStringLocalizer<IndexModel> localizer,
        ISender mediator) : PageModel
    {
        public IEnumerable<UserDto> Users { get; set; } = new List<UserDto>();
        public UserStatsDto Stats { get; set; } = new();
        public IEnumerable<RoleDto> Roles { get; set; } = new List<RoleDto>();

        [BindProperty(SupportsGet = true)]
        public string? RoleFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public async Task OnGetAsync()
        {
            Users = await mediator.Send(new GetUsersQuery
            {
                RoleFilter = RoleFilter,
                StatusFilter = StatusFilter,
                SearchTerm = SearchTerm
            });

            Stats = await mediator.Send(new GetUserStatsQuery());
            Roles = await mediator.Send(new GetRolesQuery());
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(string userId)
        {
            await mediator.Send(new ToggleUserStatusCommand { UserId = userId });
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSeedRolesAsync()
        {
            var result = await mediator.Send(new SeedRolesCommand());
            TempData["SuccessMessage"] = result.Message;
            return RedirectToPage();
        }
    }
}

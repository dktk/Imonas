using Application.Features.Psps.Commands;
using Application.Features.Psps.Queries;

namespace SmartAdmin.WebUI.Pages.Config
{
    public class InternalSystemsModel(
        IStringLocalizer<InternalSystemsModel> localizer,
        IMediator mediator) : PageModel
    {
        public List<InternalSystemListDto> Systems { get; set; } = new();

        [BindProperty]
        public int EditId { get; set; }

        [BindProperty]
        public string Name { get; set; } = string.Empty;

        [BindProperty]
        public bool IsActive { get; set; } = true;

        public async Task OnGetAsync()
        {
            Systems = await mediator.Send(new GetInternalSystemsWithCountQuery());
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return new JsonResult(new { succeeded = false, message = localizer["Name is required."].Value });
            }

            var command = new CreateInternalSystemCommand
            {
                Name = Name.Trim(),
                IsActive = IsActive
            };

            var result = await mediator.Send(command);

            if (result.Success)
            {
                return new JsonResult(new
                {
                    succeeded = true,
                    message = localizer["Internal system created successfully."].Value,
                    id = result.Value
                });
            }

            return new JsonResult(new { succeeded = false, message = result.Message ?? "Failed to create internal system." });
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (EditId <= 0)
            {
                return new JsonResult(new { succeeded = false, message = localizer["Invalid system ID."].Value });
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                return new JsonResult(new { succeeded = false, message = localizer["Name is required."].Value });
            }

            var command = new UpdateInternalSystemCommand
            {
                Id = EditId,
                Name = Name.Trim(),
                IsActive = IsActive
            };

            var result = await mediator.Send(command);

            if (result.Success)
            {
                return new JsonResult(new
                {
                    succeeded = true,
                    message = localizer["Internal system updated successfully."].Value
                });
            }

            return new JsonResult(new { succeeded = false, message = result.Message ?? "Failed to update internal system." });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (id <= 0)
            {
                return new JsonResult(new { succeeded = false, message = localizer["Invalid system ID."].Value });
            }

            var command = new DeleteInternalSystemCommand { Id = id };
            var result = await mediator.Send(command);

            if (result.Success)
            {
                return new JsonResult(new
                {
                    succeeded = true,
                    message = localizer["Internal system deleted successfully."].Value
                });
            }

            return new JsonResult(new { succeeded = false, message = result.Message ?? "Failed to delete internal system." });
        }

        public async Task<IActionResult> OnGetSystemAsync(int id)
        {
            var system = await mediator.Send(new GetInternalSystemByIdQuery { Id = id });

            if (system == null)
            {
                return new JsonResult(new { succeeded = false, message = "System not found." });
            }

            return new JsonResult(new { succeeded = true, data = system });
        }
    }
}

using Application.Features.Psps.Commands;
using Application.Features.Psps.DTOs;
using Application.Features.Psps.Queries;

namespace SmartAdmin.WebUI.Pages.Psps
{
    [Authorize]
    public class AdminModel(
        IStringLocalizer<AdminModel> localizer,
        ISender mediator) : PageModel
    {
        public List<PspDetailDto> Psps { get; set; } = new();
        public List<InternalSystemDto> InternalSystems { get; set; } = new();

        [BindProperty]
        public int EditId { get; set; }

        [BindProperty]
        public string Name { get; set; } = string.Empty;

        [BindProperty]
        public string Code { get; set; } = string.Empty;

        [BindProperty]
        public bool IsActive { get; set; } = true;

        [BindProperty]
        public bool IsCsvBased { get; set; } = true;

        [BindProperty]
        public int InternalSystemId { get; set; }

        public async Task OnGetAsync()
        {
            Psps = await mediator.Send(new GetPspsWithDetailsQuery());
            InternalSystems = (await mediator.Send(new GetInternalSystemsQuery()))
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .ToList();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return new JsonResult(new { succeeded = false, message = localizer["Name is required."].Value });
            }

            if (string.IsNullOrWhiteSpace(Code))
            {
                return new JsonResult(new { succeeded = false, message = localizer["Code is required."].Value });
            }

            if (InternalSystemId <= 0)
            {
                return new JsonResult(new { succeeded = false, message = localizer["Please select an Internal System."].Value });
            }

            var command = new AddEditPspCommand
            {
                Id = 0,
                Name = Name.Trim(),
                Code = Code.Trim().ToUpper(),
                IsActive = IsActive,
                IsCsvBased = IsCsvBased,
                InternalSystemId = InternalSystemId
            };

            var result = await mediator.Send(command);

            if (result.Success)
            {
                return new JsonResult(new
                {
                    succeeded = true,
                    message = localizer["PSP created successfully."].Value,
                    id = result.Value
                });
            }

            return new JsonResult(new { succeeded = false, message = result.Message ?? "Failed to create PSP." });
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (EditId <= 0)
            {
                return new JsonResult(new { succeeded = false, message = localizer["Invalid PSP ID."].Value });
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                return new JsonResult(new { succeeded = false, message = localizer["Name is required."].Value });
            }

            if (string.IsNullOrWhiteSpace(Code))
            {
                return new JsonResult(new { succeeded = false, message = localizer["Code is required."].Value });
            }

            if (InternalSystemId <= 0)
            {
                return new JsonResult(new { succeeded = false, message = localizer["Please select an Internal System."].Value });
            }

            var command = new AddEditPspCommand
            {
                Id = EditId,
                Name = Name.Trim(),
                Code = Code.Trim().ToUpper(),
                IsActive = IsActive,
                IsCsvBased = IsCsvBased,
                InternalSystemId = InternalSystemId
            };

            var result = await mediator.Send(command);

            if (result.Success)
            {
                return new JsonResult(new
                {
                    succeeded = true,
                    message = localizer["PSP updated successfully."].Value
                });
            }

            return new JsonResult(new { succeeded = false, message = result.Message ?? "Failed to update PSP." });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (id <= 0)
            {
                return new JsonResult(new { succeeded = false, message = localizer["Invalid PSP ID."].Value });
            }

            var command = new DeletePspCommand { Id = id };
            var result = await mediator.Send(command);

            if (result.Success)
            {
                return new JsonResult(new
                {
                    succeeded = true,
                    message = localizer["PSP deleted successfully."].Value
                });
            }

            return new JsonResult(new { succeeded = false, message = result.Message ?? "Failed to delete PSP." });
        }

        public async Task<IActionResult> OnPostToggleActiveAsync(int id)
        {
            if (id <= 0)
            {
                return new JsonResult(new { succeeded = false, message = localizer["Invalid PSP ID."].Value });
            }

            var psps = await mediator.Send(new GetPspsWithDetailsQuery());
            var psp = psps.FirstOrDefault(p => p.Id == id);

            if (psp == null)
            {
                return new JsonResult(new { succeeded = false, message = "PSP not found." });
            }

            var command = new AddEditPspCommand
            {
                Id = psp.Id,
                Name = psp.Name,
                Code = psp.Code,
                IsActive = !psp.IsActive,
                IsCsvBased = psp.IsCsvBased,
                InternalSystemId = psp.InternalSystemId
            };

            var result = await mediator.Send(command);

            if (result.Success)
            {
                return new JsonResult(new
                {
                    succeeded = true,
                    message = psp.IsActive
                        ? localizer["PSP deactivated successfully."].Value
                        : localizer["PSP activated successfully."].Value,
                    newStatus = !psp.IsActive
                });
            }

            return new JsonResult(new { succeeded = false, message = result.Message ?? "Failed to toggle PSP status." });
        }

        public async Task<IActionResult> OnGetSeedAsync()
        {
            var result = await mediator.Send(new SeedPspsCommand());
            return new JsonResult(result);
        }
    }
}

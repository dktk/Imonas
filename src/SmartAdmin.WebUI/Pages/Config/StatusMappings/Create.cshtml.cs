using System.ComponentModel.DataAnnotations;

using Application.Features.Configuration.Commands;
using Application.Features.Psps.DTOs;
using Application.Features.Psps.Queries;

namespace SmartAdmin.WebUI.Pages.Config.StatusMappings
{
    [Authorize]
    public class CreateModel(
        IStringLocalizer<CreateModel> localizer,
        ISender mediator) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public IEnumerable<PspDto> PspProfiles { get; set; } = new List<PspDto>();
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "PSP Profile")]
            public int PspId { get; set; }

            [Required]
            [Display(Name = "PSP Status")]
            public string PspStatus { get; set; } = string.Empty;

            [Required]
            [Display(Name = "Canonical Status")]
            public string CanonicalStatus { get; set; } = string.Empty;

            [Display(Name = "Description")]
            public string? Description { get; set; }

            [Required]
            [Display(Name = "Version")]
            public string Version { get; set; } = "1.0.0";

            [Display(Name = "Active")]
            public bool IsActive { get; set; } = true;
        }

        public async Task OnGetAsync()
        {
            PspProfiles = await mediator.Send(new GetAllPspsQuery());
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                PspProfiles = await mediator.Send(new GetAllPspsQuery());
                return Page();
            }

            try
            {
                var command = new CreateStatusMappingCommand
                {
                    PspId = Input.PspId,
                    PspStatus = Input.PspStatus,
                    CanonicalStatus = Input.CanonicalStatus,
                    Description = Input.Description,
                    Version = Input.Version,
                    IsActive = Input.IsActive
                };

                var result = await mediator.Send(command);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToPage("/Config/StatusMappings");
                }

                ErrorMessage = result.Message ?? "Failed to create status mapping.";
                PspProfiles = await mediator.Send(new GetAllPspsQuery());
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to create status mapping: {ex.Message}";
                PspProfiles = await mediator.Send(new GetAllPspsQuery());
                return Page();
            }
        }
    }
}

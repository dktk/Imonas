using System.ComponentModel.DataAnnotations;

using Application.Features.Configuration.Commands;
using Application.Features.Configuration.Queries;

namespace SmartAdmin.WebUI.Pages.Config.FieldMappings
{
    [Authorize]
    public class EditModel(
        IStringLocalizer<EditModel> localizer,
        ISender mediator) : PageModel
    {
        public FieldMappingDto? Mapping { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            public int Id { get; set; }

            [Required]
            [Display(Name = "Source Field")]
            public string SourceField { get; set; } = string.Empty;

            [Required]
            [Display(Name = "Target Field")]
            public string TargetField { get; set; } = string.Empty;

            [Display(Name = "Transform Expression")]
            public string? TransformExpression { get; set; }

            [Required]
            [Display(Name = "Version")]
            public string Version { get; set; } = string.Empty;

            [Required]
            [Display(Name = "Effective From")]
            [DataType(DataType.Date)]
            public DateTime EffectiveFrom { get; set; }

            [Display(Name = "Effective To")]
            [DataType(DataType.Date)]
            public DateTime? EffectiveTo { get; set; }

            [Display(Name = "Active")]
            public bool IsActive { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Mapping = await mediator.Send(new GetFieldMappingByIdQuery { Id = id });
            if (Mapping == null) return NotFound();

            Input = new InputModel
            {
                Id = Mapping.Id,
                SourceField = Mapping.SourceField,
                TargetField = Mapping.TargetField,
                TransformExpression = Mapping.TransformExpression,
                Version = Mapping.Version,
                EffectiveFrom = Mapping.EffectiveFrom,
                EffectiveTo = Mapping.EffectiveTo,
                IsActive = Mapping.IsActive
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Mapping = await mediator.Send(new GetFieldMappingByIdQuery { Id = Input.Id });
                return Page();
            }

            try
            {
                var command = new UpdateFieldMappingCommand
                {
                    Id = Input.Id,
                    SourceField = Input.SourceField,
                    TargetField = Input.TargetField,
                    TransformExpression = Input.TransformExpression,
                    Version = Input.Version,
                    EffectiveFrom = Input.EffectiveFrom,
                    EffectiveTo = Input.EffectiveTo,
                    IsActive = Input.IsActive
                };

                var result = await mediator.Send(command);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToPage("/Config/FieldMappings");
                }

                ErrorMessage = result.Message ?? "Failed to update field mapping.";
                Mapping = await mediator.Send(new GetFieldMappingByIdQuery { Id = Input.Id });
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to update field mapping: {ex.Message}";
                Mapping = await mediator.Send(new GetFieldMappingByIdQuery { Id = Input.Id });
                return Page();
            }
        }
    }
}

using System.ComponentModel.DataAnnotations;

using Application.Features.Configuration.Commands;
using Application.Features.Files.Queries;
using Application.Features.Psps.DTOs;
using Application.Features.Psps.Queries;

namespace SmartAdmin.WebUI.Pages.Config.FieldMappings
{
    [Authorize]
    public class CreateModel(
        IStringLocalizer<CreateModel> localizer,
        ISender mediator) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();

        [BindProperty]
        public BulkInputModel BulkInput { get; set; } = new();

        public IEnumerable<PspDto> PspProfiles { get; set; } = new List<PspDto>();
        public IEnumerable<UploadedFileDto> UploadedFiles { get; set; } = new List<UploadedFileDto>();
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "PSP Profile")]
            public int PspId { get; set; }

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
            public string Version { get; set; } = "1.0.0";

            [Required]
            [Display(Name = "Effective From")]
            [DataType(DataType.Date)]
            public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow.Date;

            [Display(Name = "Effective To")]
            [DataType(DataType.Date)]
            public DateTime? EffectiveTo { get; set; }

            [Display(Name = "Active")]
            public bool IsActive { get; set; } = true;
        }

        public class BulkInputModel
        {
            [Required]
            [Display(Name = "PSP Profile")]
            public int PspId { get; set; }

            [Required]
            [Display(Name = "Version")]
            public string Version { get; set; } = "1.0.0";

            [Required]
            [Display(Name = "Effective From")]
            [DataType(DataType.Date)]
            public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow.Date;

            [Display(Name = "Effective To")]
            [DataType(DataType.Date)]
            public DateTime? EffectiveTo { get; set; }

            [Display(Name = "Active")]
            public bool IsActive { get; set; } = true;

            // JSON array of mappings: [{ sourceField, targetField, transformExpression }]
            public string MappingsJson { get; set; } = "[]";
        }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            try
            {
                var command = new CreateFieldMappingCommand
                {
                    PspId = Input.PspId,
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

                ErrorMessage = result.Message ?? "Failed to create field mapping.";
                await LoadDataAsync();
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to create field mapping: {ex.Message}";
                await LoadDataAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostBulkAsync()
        {
            await LoadDataAsync();

            try
            {
                var mappings = System.Text.Json.JsonSerializer.Deserialize<List<FieldMappingInput>>(
                    BulkInput.MappingsJson,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? new List<FieldMappingInput>();

                if (mappings.Count == 0)
                {
                    ErrorMessage = "No mappings defined. Please drag CSV columns to target fields.";
                    return Page();
                }

                var command = new BulkCreateFieldMappingsCommand
                {
                    PspId = BulkInput.PspId,
                    Mappings = mappings,
                    Version = BulkInput.Version,
                    EffectiveFrom = BulkInput.EffectiveFrom,
                    EffectiveTo = BulkInput.EffectiveTo,
                    IsActive = BulkInput.IsActive
                };

                var result = await mediator.Send(command);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToPage("/Config/FieldMappings");
                }

                ErrorMessage = result.Message ?? "Failed to create field mappings.";
                return Page();
            }
            catch (System.Text.Json.JsonException ex)
            {
                ErrorMessage = $"Invalid mappings data: {ex.Message}";
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to create field mappings: {ex.Message}";
                return Page();
            }
        }

        private async Task LoadDataAsync()
        {
            PspProfiles = await mediator.Send(new GetAllPspsQuery());
            var filesResult = await mediator.Send(new GetFilesQuery { PageSize = 50 });
            if (filesResult.Success)
            {
                UploadedFiles = filesResult.Value ?? Array.Empty<UploadedFileDto>();
            }
        }
    }
}

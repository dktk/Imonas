using Application.Features.Configuration.Queries;
using Application.Features.Psps.DTOs;
using Application.Features.Psps.Queries;

namespace SmartAdmin.WebUI.Pages.Config
{
    [Authorize]
    public class FieldMappingsModel(
        IStringLocalizer<FieldMappingsModel> localizer,
        ISender mediator) : PageModel
    {
        public IEnumerable<FieldMappingDto> Mappings { get; set; } = new List<FieldMappingDto>();
        public IEnumerable<PspDto> AvailablePsps { get; set; } = new List<PspDto>();

        [BindProperty(SupportsGet = true)]
        public int? SelectedPspId { get; set; }

        public async Task OnGetAsync()
        {
            // Load all active PSPs for the filter dropdown
            AvailablePsps = await mediator.Send(new GetCsvBasedPspsQuery());

            // Load mappings
            var allMappings = await mediator.Send(new GetFieldMappingsQuery());

            // Filter mappings if a PSP is selected
            if (SelectedPspId.HasValue)
            {
                Mappings = allMappings.Where(m => m.PspId == SelectedPspId.Value).ToList();
            }
            else
            {
                Mappings = allMappings;
            }
        }
    }
}

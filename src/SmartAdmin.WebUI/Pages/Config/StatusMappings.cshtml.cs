using Application.Features.Configuration.Queries;

namespace SmartAdmin.WebUI.Pages.Config
{
    [Authorize]
    public class StatusMappingsModel(
        IStringLocalizer<StatusMappingsModel> localizer,
        ISender mediator) : PageModel
    {
        public IEnumerable<StatusMappingDto> Mappings { get; set; } = new List<StatusMappingDto>();

        public async Task OnGetAsync()
        {
            Mappings = await mediator.Send(new GetStatusMappingsQuery());
        }
    }
}

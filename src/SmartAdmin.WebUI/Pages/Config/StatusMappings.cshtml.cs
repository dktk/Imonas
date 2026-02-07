using Application.Features.Configuration.Queries;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace SmartAdmin.WebUI.Pages.Config
{
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

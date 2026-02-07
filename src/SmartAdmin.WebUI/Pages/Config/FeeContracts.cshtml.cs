using Application.Features.Configuration.FeeContracts.DTOs;
using Application.Features.Configuration.FeeContracts.Queries;

namespace SmartAdmin.WebUI.Pages.Config
{
    public class FeeContractsModel(
        IStringLocalizer<FeeContractsModel> localizer,
        ISender mediator) : PageModel
    {
        public IEnumerable<FeeContractDto> Contracts { get; set; } = new List<FeeContractDto>();

        public async Task OnGetAsync()
        {
            Contracts = await mediator.Send(new GetFeeContractsQuery());
        }
    }
}

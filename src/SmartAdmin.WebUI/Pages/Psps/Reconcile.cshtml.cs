using Application.Features.Psps.Commands;
using Application.Features.Psps.Queries;

namespace SmartAdmin.WebUI.Pages.Psps
{
    public class ReconcileModel(IStringLocalizer<ReconcileModel> localizer, ISender mediator) : PageModel
    {
        public Dictionary<int, string> PspOptions { get; set; }

        [BindProperty]
        public ReconcileDatesForPspCommand Input { get; set; }

        public async Task OnGetAsync()
        {
            var result = await mediator.Send(new GetAllPspsQuery());

            PspOptions = result.ToDictionary(x => x.Id, x => x.Name);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // todo:
            Input.EndDate = Input.EndDate;
            Input.ExternalSystem = "SquarePay";

            var result = await mediator.Send(Input);

            return new JsonResult(result);
        }
    }
}

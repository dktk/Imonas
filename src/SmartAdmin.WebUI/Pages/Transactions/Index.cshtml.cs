using Application;
using Application.Common.Interfaces;
using Application.Features.Payments.Queries;
using Application.Features.Psps.Commands;
using Application.Features.Psps.Queries;

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using SG.Common;

namespace SmartAdmin.WebUI.Pages.Transactions
{
    public class IndexModel(
        IStringLocalizer<IndexModel> localizer,
        IApplicationDbContext context,
        ResultSafeWrapper resultSafeWrapper,
        IDateTime dateTime, IMediator mediator) : PageModel
    {
        [BindProperty] public DateTime StartDate { get; set; }
        [BindProperty] public DateTime EndDate { get; set; }
        [BindProperty] public int SelectedPspId { get; set; }
        [BindProperty] public int Page { get; set; }
        [BindProperty] public int Rows { get; set; }
        [BindProperty] public string Sort { get; set; }
        [BindProperty] public string Order { get; set; }

        public SelectList Psps { get; set; }

        public async Task OnGetAsync()
        {
            StartDate = dateTime.TwoDaysBack;
            EndDate = dateTime.Yesterday;

            await LoadSelectLists();
        }

        public async Task<IActionResult> OnGetDataAsync([FromQuery] PaymentsWithPaginationQuery query)
        {
            var result = await mediator.Send(query);
            return new CamelCaseJsonResult(result);
        }


        public async Task<IActionResult> OnPostReconcileAsync()
        {
            var result = await resultSafeWrapper.ExecuteAsync(async () => {
                                var internalSystemName = await context.Psps
                                                            .Where(x => x.Id == SelectedPspId)
                                                            .Select(x => x.InternalSystem.Name)
                                                            .FirstOrDefaultAsync();

                                if (string.IsNullOrWhiteSpace(internalSystemName))
                                {
                                    return Result<ReconcileDatesForPspCommandDto>.BuildFailure("Unable to retrieve the Internal System associated to PSP " + SelectedPspId);
                                }

                                var command = new ReconcileDatesForPspCommand
                                {
                                    EndDate = EndDate,
                                    StartDate = StartDate,
                                    PspId = SelectedPspId,
                                    ExternalSystem = internalSystemName,
                                    ReconciliationRunId = 1
                                };

                                var result = await mediator.Send(command);

                                return result;
                            }, this, $"Unable to reconcile {SelectedPspId}");

            return new CamelCaseJsonResult(result);
        }

        public async Task<IActionResult> OnPostDataAsync()
        {
            if (string.IsNullOrWhiteSpace(Sort) && string.IsNullOrWhiteSpace(Order))
            {
                return new CamelCaseJsonResult(new
                {
                    succeeded = true,
                    data = new PaginatedData<PaymentDto>(Enumerable.Empty<PaymentDto>(), 0),
                    message = string.Empty
                });
            }

            // Create a query with the search parameters
            var query = new PaymentsWithPaginationQuery
            {
                StartDate = StartDate,
                EndDate = EndDate,
                SelectedPspId = SelectedPspId,
                Page = Page,
                Rows = Rows,
                Sort = Sort,
                Order = Order
            };

            var result = await mediator.Send(query);

            var message = result.total == 0 ? localizer["No results found."] : string.Format(localizer["Found {0} results."], result.total);
            return new CamelCaseJsonResult(new
            {
                succeeded = true,
                data = result,
                message
            });
        }

        private async Task LoadSelectLists()
        {
            // Load PSPs for dropdown
            var pspsQuery = new GetAllPspsQuery();
            var psps = await mediator.Send(pspsQuery);

            Psps = new SelectList(psps.OrderBy(x => x.Name), "Id", "Name");
        }
    }
}

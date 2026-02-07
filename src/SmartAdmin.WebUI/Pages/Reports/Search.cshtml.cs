using Application.Common.Interfaces;
using Application.Features.Psps.Queries;
using Application.Features.Reports.Queries;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SmartAdmin.WebUI.Pages.Reports
{
    public class SearchModel(
        IStringLocalizer<SearchModel> localizer,
        IMediator mediator) : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string? TransactionId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ExternalPaymentId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? PlayerId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedPspId { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? AmountFrom { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? AmountTo { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? DateFrom { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? DateTo { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? CurrencyCode { get; set; }

        public SelectList Psps { get; set; } = default!;
        public List<SelectListItem> Statuses { get; set; } = new();
        public List<SelectListItem> Currencies { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadSelectLists();
        }

        public async Task<IActionResult> OnGetSearchAsync(
            string? transactionId,
            string? externalPaymentId,
            string? playerId,
            int? pspId,
            decimal? amountFrom,
            decimal? amountTo,
            DateTime? dateFrom,
            DateTime? dateTo,
            string? status,
            string? currencyCode,
            int page = 1,
            int rows = 25,
            string sort = "TxDate",
            string order = "desc")
        {
            var query = new SearchTransactionsQuery
            {
                TransactionId = transactionId,
                ExternalPaymentId = externalPaymentId,
                PlayerId = playerId,
                PspId = pspId,
                AmountFrom = amountFrom,
                AmountTo = amountTo,
                DateFrom = dateFrom,
                DateTo = dateTo,
                Status = status,
                CurrencyCode = currencyCode,
                Page = page <= 0 ? 1 : page,
                Rows = rows <= 0 ? 25 : rows,
                Sort = sort ?? "TxDate",
                Order = order ?? "desc"
            };

            var result = await mediator.Send(query);

            return new JsonResult(new
            {
                succeeded = true,
                data = result,
                message = result.total == 0
                    ? localizer["No transactions found matching your criteria."].Value
                    : string.Format(localizer["Found {0} transaction(s)."].Value, result.total)
            });
        }

        public async Task<IActionResult> OnGetDetailsAsync(int id)
        {
            // Get a single transaction by ID with full details
            var query = new SearchTransactionsQuery
            {
                Page = 1,
                Rows = 1
            };

            // We'll filter by ID in the result
            var allResults = await mediator.Send(query);
            var transaction = allResults.rows.FirstOrDefault(t => t.Id == id);

            if (transaction == null)
            {
                return NotFound(new { error = "Transaction not found" });
            }

            return new JsonResult(new
            {
                succeeded = true,
                data = transaction
            });
        }

        private async Task LoadSelectLists()
        {
            var pspsQuery = new GetAllPspsQuery();
            var psps = await mediator.Send(pspsQuery);
            Psps = new SelectList(psps.OrderBy(x => x.Name), "Id", "Name");

            // Common statuses
            Statuses = new List<SelectListItem>
            {
                new("All Statuses", ""),
                new("completed", "completed"),
                new("pending", "pending"),
                new("failed", "failed"),
                new("refunded", "refunded"),
                new("cancelled", "cancelled")
            };

            // Common currencies
            Currencies = new List<SelectListItem>
            {
                new("All Currencies", ""),
                new("EUR", "EUR"),
                new("USD", "USD"),
                new("GBP", "GBP"),
                new("CAD", "CAD"),
                new("AUD", "AUD")
            };
        }
    }
}

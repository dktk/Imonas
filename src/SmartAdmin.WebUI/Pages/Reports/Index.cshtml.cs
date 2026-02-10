using System.Text.Json;

using Application.Common.Interfaces;
using Application.Features.Psps.Queries;
using Application.Features.Reports.Queries;

using Microsoft.AspNetCore.Mvc.Rendering;

namespace SmartAdmin.WebUI.Pages.Reports
{
    public class IndexModel(
        IStringLocalizer<IndexModel> localizer,
        IDateTime dateTime,
        IMediator mediator) : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public DateTime StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedPspId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? RunId { get; set; }

        [BindProperty(SupportsGet = true)]
        public SettlementFilter SettlementFilter { get; set; } = SettlementFilter.All;

        [BindProperty(SupportsGet = true)]
        public MatchStatusFilter MatchStatus { get; set; } = MatchStatusFilter.All;

        [BindProperty(SupportsGet = true)]
        public int Page { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int Rows { get; set; } = 25;

        [BindProperty(SupportsGet = true)]
        public string Sort { get; set; } = "TxDate";

        [BindProperty(SupportsGet = true)]
        public string Order { get; set; } = "desc";

        public SelectList Psps { get; set; } = default!;
        public string? RunName { get; set; }

        public async Task OnGetAsync([FromQuery]int runId = 0, [FromQuery]string matchedStatus = null)
        {
            if (StartDate == default)
                StartDate = dateTime.TwoDaysBack;
            if (EndDate == default)
                EndDate = dateTime.Yesterday;

            // Load run name if RunId is specified
            if (RunId.HasValue && RunId.Value > 0)
            {
                var run = await mediator.Send(new Application.Features.ReconciliationRuns.Queries.GetRunByIdQuery { Id = RunId.Value });
                RunName = run?.RunName;
            }

            await LoadSelectLists();
        }

        public async Task<IActionResult> OnGetDataAsync(
            DateTime startDate,
            DateTime endDate,
            int? pspId,
            int? runId,
            SettlementFilter settlementFilter,
            MatchStatusFilter matchStatus,
            int page,
            int rows,
            string sort,
            string order)
        {
            var query = new GetTransactionReportQuery
            {
                StartDate = startDate,
                EndDate = endDate,
                PspId = pspId,
                RunId = runId,
                SettlementFilter = settlementFilter,
                MatchStatus = matchStatus,
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
                    ? localizer["No results found."].Value
                    : string.Format(localizer["Found {0} results."].Value, result.total)
            }, SystemExtensions.CamelCaseJsonPolicy);
        }

        public async Task<IActionResult> OnGetExportAsync(
            DateTime startDate,
            DateTime endDate,
            int? pspId,
            int? runId,
            SettlementFilter settlementFilter,
            MatchStatusFilter matchStatus)
        {
            var query = new GetTransactionReportQuery
            {
                StartDate = startDate,
                EndDate = endDate,
                PspId = pspId,
                RunId = runId,
                SettlementFilter = settlementFilter,
                MatchStatus = matchStatus,
                Page = 1,
                Rows = 50000, // Max export
                Sort = "TxDate",
                Order = "desc"
            };

            var result = await mediator.Send(query);

            // Generate CSV with proper escaping
            var csv = new System.Text.StringBuilder();

            // Header row
            csv.AppendLine("Transaction ID,External Payment ID,PSP,User ID,Brand,Amount,Currency,Date,Status,Action,Settlement Status,Fees,Net Settlement,Case Number,Case Status");

            foreach (var item in result.rows)
            {
                csv.AppendLine(string.Join(",",
                    EscapeCsvField(item.TxId),
                    EscapeCsvField(item.ExternalPaymentId),
                    EscapeCsvField(item.PspName),
                    EscapeCsvField(item.ClientId.ToString()),
                    EscapeCsvField(item.BrandId),
                    item.Amount.ToString("F2"),
                    EscapeCsvField(item.CurrencyCode),
                    item.TxDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    EscapeCsvField(item.Status),
                    EscapeCsvField(item.Action),
                    EscapeCsvField(item.SettlementStatusDisplay),
                    item.TotalFees?.ToString("F2") ?? "",
                    item.NetSettlement?.ToString("F2") ?? "",
                    EscapeCsvField(item.CaseNumber ?? ""),
                    EscapeCsvField(item.CaseStatus ?? "")
                ));
            }

            // Add BOM for Excel UTF-8 compatibility
            var preamble = System.Text.Encoding.UTF8.GetPreamble();
            var csvBytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            var bytes = new byte[preamble.Length + csvBytes.Length];
            preamble.CopyTo(bytes, 0);
            csvBytes.CopyTo(bytes, preamble.Length);

            // Generate filename with filter context
            var filenameParts = new List<string> { "transaction_report" };
            if (runId.HasValue && runId.Value > 0)
                filenameParts.Add($"run{runId}");
            if (matchStatus != MatchStatusFilter.All)
                filenameParts.Add(matchStatus.ToString().ToLower());
            filenameParts.Add(DateTime.Now.ToString("yyyyMMdd_HHmmss"));

            var filename = string.Join("_", filenameParts) + ".csv";

            return File(bytes, "text/csv", filename);
        }

        private static string EscapeCsvField(string? field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            // If field contains comma, newline, or quote, wrap in quotes and escape internal quotes
            if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field;
        }

        private async Task LoadSelectLists()
        {
            var pspsQuery = new GetAllPspsQuery();
            var psps = await mediator.Send(pspsQuery);
            Psps = new SelectList(psps.OrderBy(x => x.Name), "Id", "Name");
        }
    }
}

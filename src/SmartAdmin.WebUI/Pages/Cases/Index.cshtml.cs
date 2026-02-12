using Application.Features.Cases.Commands;
using Application.Features.Cases.DTOs;
using Application.Features.Cases.Queries;

namespace SmartAdmin.WebUI.Pages.Cases
{
    [Authorize]
    public class IndexModel(
        IStringLocalizer<IndexModel> localizer,
        ISender mediator) : PageModel
    {
        public IEnumerable<ExceptionCaseDto> Cases { get; set; } = new List<ExceptionCaseDto>();
        public CaseStatsDto Stats { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? RunId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SeverityFilter { get; set; }

        public async Task OnGetAsync()
        {
            var allCases = await mediator.Send(new GetCasesQuery { PageSize = 100 });
            Stats = await mediator.Send(new GetCaseStatsQuery { RunId = RunId });

            // Apply filters
            var filteredCases = allCases.AsEnumerable();

            if (!string.IsNullOrEmpty(StatusFilter) && Enum.TryParse<CaseStatus>(StatusFilter, out var status))
            {
                filteredCases = filteredCases.Where(c => c.Status == status);
            }

            if (!string.IsNullOrEmpty(SeverityFilter) && Enum.TryParse<CaseSeverity>(SeverityFilter, out var severity))
            {
                filteredCases = filteredCases.Where(c => c.Severity == severity);
            }

            Cases = filteredCases.ToList();
        }

        public async Task<IActionResult> OnPostAssignAsync(int id, string assignedTo)
        {
            if (id <= 0 || string.IsNullOrEmpty(assignedTo))
            {
                return new CamelCaseJsonResult(new { succeeded = false, message = localizer["Invalid parameters."].Value });
            }

            var result = await mediator.Send(new AssignCaseCommand { CaseId = id, AssignedTo = assignedTo });

            if (result.Success)
            {
                return new CamelCaseJsonResult(new { succeeded = true, message = result.Message });
            }

            return new CamelCaseJsonResult(new { succeeded = false, message = result.Message ?? localizer["Failed to assign case."].Value });
        }
    }
}

using Application.Features.Rules.Commands;
using Application.Features.Rules.Queries;

namespace SmartAdmin.WebUI.Pages.Rules
{
    [Authorize]
    public class IndexModel(
        IStringLocalizer<IndexModel> localizer,
        ISender mediator) : PageModel
    {
        public IEnumerable<MatchingRuleDto> Rules { get; set; } = new List<MatchingRuleDto>();
        public int ActiveRulesCount { get; set; }
        public string CurrentVersion { get; set; } = "1.0.0";

        public async Task OnGetAsync()
        {
            Rules = await mediator.Send(new GetMatchingRulesQuery());
            ActiveRulesCount = await mediator.Send(new GetActiveRulesCountQuery());
        }

        public async Task<IActionResult> OnPostToggleActiveAsync(int id)
        {
            if (id <= 0)
            {
                return new JsonResult(new { succeeded = false, message = localizer["Invalid rule ID."].Value });
            }

            var result = await mediator.Send(new ToggleMatchingRuleCommand { Id = id });

            if (result.Success)
            {
                return new JsonResult(new
                {
                    succeeded = true,
                    message = result.Message,
                    newStatus = result.Value
                });
            }

            return new JsonResult(new { succeeded = false, message = result.Message ?? localizer["Failed to toggle rule status."] });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (id <= 0)
            {
                return new JsonResult(new { succeeded = false, message = localizer["Invalid rule ID."].Value });
            }

            var result = await mediator.Send(new DeleteMatchingRuleCommand { Id = id });

            if (result.Success)
            {
                return new JsonResult(new
                {
                    succeeded = true,
                    message = result.Message
                });
            }

            return new JsonResult(new { succeeded = false, message = result.Message ?? localizer["Failed to delete rule."] });
        }
    }
}

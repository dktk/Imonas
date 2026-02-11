using Application;
using Application.Common.Interfaces;

namespace SmartAdmin.WebUI.Pages.Config
{
    [Authorize]
    public class IndexModel(IStringLocalizer<IndexModel> localizer,
        IDateTime dateTime,
        IUptimeService uptimeService) : PageModel
    {
        public string Uptime { get; set; } = string.Empty;

        public void OnGet()
        {
            Uptime = uptimeService.GetUptimeString(dateTime);
        }
    }
}

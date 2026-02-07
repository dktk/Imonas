using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Imonas.V9.Web.Pages.Config;

public class IndexModel : PageModel
{
    public string Uptime { get; set; } = "0h 0m";

    public void OnGet()
    {
        var uptime = DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime();
        Uptime = $"{(int)uptime.TotalHours}h {uptime.Minutes}m";
    }
}

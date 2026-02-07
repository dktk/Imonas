using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Imonas.V9.Web.Pages.Rules;

public class IndexModel : PageModel
{
    public int ActiveRules { get; set; }
    public string CurrentVersion { get; set; } = "1.0.0";

    public void OnGet()
    {
        ActiveRules = 0;
    }
}

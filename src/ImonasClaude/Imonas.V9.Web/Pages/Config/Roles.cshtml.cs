using Microsoft.AspNetCore.Mvc.RazorPages;
using Imonas.V9.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Imonas.V9.Web.Pages.Config;

public class RolesModel : PageModel
{
    private readonly ImonasDbContext _context;

    public RolesModel(ImonasDbContext context)
    {
        _context = context;
    }

    public int AdminCount { get; set; }
    public int AnalystCount { get; set; }
    public int ViewerCount { get; set; }

    public async Task OnGetAsync()
    {
        AdminCount = await _context.UserRoles.CountAsync(r => r.RoleName == "Admin");
        AnalystCount = await _context.UserRoles.CountAsync(r => r.RoleName == "Analyst");
        ViewerCount = await _context.UserRoles.CountAsync(r => r.RoleName == "Viewer");
    }
}

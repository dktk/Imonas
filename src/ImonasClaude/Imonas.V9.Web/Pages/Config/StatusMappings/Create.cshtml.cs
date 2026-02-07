using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Imonas.V9.Infrastructure.Data;
using Imonas.V9.Domain.Entities.Configuration;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Imonas.V9.Web.Pages.Config.StatusMappings;

public class CreateModel : PageModel
{
    private readonly ImonasDbContext _context;

    public CreateModel(ImonasDbContext context)
    {
        _context = context;
    }

    public List<PspProfile> PspProfiles { get; set; } = new();

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Display(Name = "PSP Profile")]
        public Guid PspProfileId { get; set; }

        [Required]
        [Display(Name = "PSP Status")]
        public string PspStatus { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Canonical Status")]
        public string CanonicalStatus { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Version")]
        public string Version { get; set; } = "1.0.0";

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }

    public async Task OnGetAsync()
    {
        PspProfiles = await _context.PspProfiles
            .OrderBy(p => p.PspName)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            PspProfiles = await _context.PspProfiles
                .OrderBy(p => p.PspName)
                .ToListAsync();
            return Page();
        }

        // Check if mapping already exists for this PSP Status
        var existingMapping = await _context.StatusMappings
            .Where(m => m.PspProfileId == Input.PspProfileId && m.PspStatus == Input.PspStatus && m.IsActive)
            .FirstOrDefaultAsync();

        if (existingMapping != null)
        {
            ModelState.AddModelError("Input.PspStatus", "An active mapping already exists for this PSP status.");
            PspProfiles = await _context.PspProfiles
                .OrderBy(p => p.PspName)
                .ToListAsync();
            return Page();
        }

        var mapping = new StatusMapping
        {
            Id = Guid.NewGuid(),
            PspProfileId = Input.PspProfileId,
            PspStatus = Input.PspStatus,
            CanonicalStatus = Input.CanonicalStatus,
            Description = Input.Description,
            Version = Input.Version,
            IsActive = Input.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "current-user" // TODO: Get from auth context
        };

        _context.StatusMappings.Add(mapping);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Status mapping created: {Input.PspStatus} → {Input.CanonicalStatus}";
        return RedirectToPage("/Config/StatusMappings");
    }
}

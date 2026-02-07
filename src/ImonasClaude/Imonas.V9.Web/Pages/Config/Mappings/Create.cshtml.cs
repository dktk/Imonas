using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Imonas.V9.Infrastructure.Data;
using Imonas.V9.Domain.Entities.Configuration;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Imonas.V9.Web.Pages.Config.Mappings;

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
        [Display(Name = "Source Field")]
        public string SourceField { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Target Field")]
        public string TargetField { get; set; } = string.Empty;

        [Display(Name = "Transform Expression")]
        public string? TransformExpression { get; set; }

        [Required]
        [Display(Name = "Version")]
        public string Version { get; set; } = "1.0.0";

        [Required]
        [Display(Name = "Effective From")]
        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;

        [Display(Name = "Effective To")]
        public DateTime? EffectiveTo { get; set; }

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

        var mapping = new FieldMapping
        {
            Id = Guid.NewGuid(),
            PspProfileId = Input.PspProfileId,
            SourceField = Input.SourceField,
            TargetField = Input.TargetField,
            TransformExpression = Input.TransformExpression,
            Version = Input.Version,
            EffectiveFrom = Input.EffectiveFrom,
            EffectiveTo = Input.EffectiveTo,
            IsActive = Input.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "current-user" // TODO: Get from auth context
        };

        _context.FieldMappings.Add(mapping);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Field mapping created: {Input.SourceField} → {Input.TargetField}";
        return RedirectToPage("/Config/Mappings");
    }
}

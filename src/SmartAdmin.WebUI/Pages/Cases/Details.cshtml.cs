using Application.Common.Interfaces;
using Application.Features.Cases.Commands;
using Application.Features.Cases.DTOs;
using Application.Features.Cases.Queries;
using Application.Features.Settlement.Commands;
using Microsoft.EntityFrameworkCore;

namespace SmartAdmin.WebUI.Pages.Cases
{
    [Authorize]
    public class DetailsModel(
        IStringLocalizer<DetailsModel> localizer,
        ISender mediator) : PageModel
    {
        public ExceptionCaseDetailsDto? Case { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Case = await mediator.Send(new GetCaseByIdQuery { Id = id });
            return Page();
        }

        public async Task<IActionResult> OnPostAddCommentAsync(int id, string comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                return RedirectToPage(new { id });
            }

            await mediator.Send(new AddCommentCommand { CaseId = id, Comment = comment });
            TempData["SuccessMessage"] = "Comment added successfully.";
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostAssignAsync(int id, string? assignedTo)
        {
            await mediator.Send(new AssignCaseCommand { CaseId = id, AssignedTo = assignedTo });
            TempData["SuccessMessage"] = "Case assigned successfully.";
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostCloseAsync(int id)
        {
            await mediator.Send(new CloseCaseCommand { CaseId = id });
            TempData["SuccessMessage"] = "Case closed successfully.";
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostUploadAttachmentAsync(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a file to upload.";
                return RedirectToPage(new { id });
            }

            const long maxFileSize = 10 * 1024 * 1024; // 10 MB
            if (file.Length > maxFileSize)
            {
                TempData["ErrorMessage"] = "File size exceeds the 10 MB limit.";
                return RedirectToPage(new { id });
            }

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            var result = await mediator.Send(new AddAttachmentCommand
            {
                CaseId = id,
                FileName = file.FileName,
                ContentType = file.ContentType,
                FileSizeBytes = file.Length,
                FileContent = ms.ToArray()
            });

            if (result.Success)
                TempData["SuccessMessage"] = "File uploaded successfully.";
            else
                TempData["ErrorMessage"] = result.Message ?? "Upload failed.";

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostDeleteAttachmentAsync(int id, int attachmentId)
        {
            var result = await mediator.Send(new DeleteAttachmentCommand { Id = attachmentId });
            if (result.Success)
                TempData["SuccessMessage"] = "Attachment deleted successfully.";
            else
                TempData["ErrorMessage"] = result.Message ?? "Delete failed.";

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnGetDownloadAttachmentAsync(int id, int attachmentId,
            [FromServices] IApplicationDbContext dbContext)
        {
            var attachment = await dbContext.CaseAttachments
                .FirstOrDefaultAsync(a => a.Id == attachmentId && a.CaseId == id);

            if (attachment == null)
                return NotFound();

            return File(attachment.FileContent, attachment.ContentType, attachment.FileName);
        }

        public async Task<IActionResult> OnPostManualSettleAsync(int id, string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                TempData["ErrorMessage"] = "A description is required for manual settlement.";
                return RedirectToPage(new { id });
            }

            var result = await mediator.Send(new CreateManualSettlementCommand
            {
                CaseId = id,
                Description = description
            });

            if (result.Success)
                TempData["SuccessMessage"] = "Manual settlement created and case closed successfully.";
            else
                TempData["ErrorMessage"] = result.Message ?? "Manual settlement failed.";

            return RedirectToPage(new { id });
        }
    }
}

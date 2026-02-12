using Application.Features.Psps.Queries;

using SmartAdmin.WebUI.HttpHandlers;

namespace SmartAdmin.WebUI.Pages.Psps
{
    [Authorize]
    public class CsvUploadModel(
        IStringLocalizer<CsvUploadModel> localizer,
        FileUploadHandlers fileUploadHandlers,
        ISender mediator) : PageModel
    {
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB in bytes

        [BindProperty]
        public IFormFile UploadedFile { get; set; }

        [BindProperty]
        public int SelectedPspId { get; set; }

        [BindProperty]
        public string SelectedSchema { get; set; } // "bronze" or "silver"

        public Dictionary<int, string> PspOptions { get; set; }

        public async Task OnGetAsync()
        {
            var result = await mediator.Send(new GetCsvBasedPspsQuery());
            PspOptions = result.ToDictionary(x => x.Id, x => x.Name);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (UploadedFile == null)
            {
                return new CamelCaseJsonResult(new { succeeded = false, message = localizer["Please select a file to upload."] });
            }

            if (UploadedFile.Length > MaxFileSize)
            {
                return new CamelCaseJsonResult(new { succeeded = false, message = localizer["File size exceeds the 5MB limit."] });
            }

            if (!UploadedFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return new CamelCaseJsonResult(new { succeeded = false, message = localizer["Only CSV files are allowed."] });
            }

            if (SelectedPspId <= 0)
            {
                return new CamelCaseJsonResult(new { succeeded = false, message = localizer["Please select a PSP."] });
            }

            using var stream = new MemoryStream();
            await UploadedFile.CopyToAsync(stream);

            var result = await fileUploadHandlers.Upload(mediator, stream, UploadedFile.FileName, SelectedPspId);

            if (result.Failed)
            {
                return new CamelCaseJsonResult(result);
            }

            return new CamelCaseJsonResult(result);
        }
    }
}

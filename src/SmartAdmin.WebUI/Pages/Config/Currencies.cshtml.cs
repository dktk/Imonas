using Application.Features.Configuration.Currencies.Commands;
using Application.Features.Configuration.Currencies.DTOs;
using Application.Features.Configuration.Currencies.Queries;

namespace SmartAdmin.WebUI.Pages.Config
{
    public class CurrenciesModel(
        IStringLocalizer<CurrenciesModel> localizer,
        IMediator mediator) : PageModel
    {
        public List<CurrencyDto> Currencies { get; set; } = new();

        [BindProperty]
        public int EditId { get; set; }

        [BindProperty]
        public string Name { get; set; } = string.Empty;

        [BindProperty]
        public string Code { get; set; } = string.Empty;

        [BindProperty]
        public string HtmlCode { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            var currencies = await mediator.Send(new GetCurrenciesQuery());
            Currencies = currencies.ToList();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return new JsonResult(new { succeeded = false, message = localizer["Name is required."].Value });
            }

            if (string.IsNullOrWhiteSpace(Code))
            {
                return new JsonResult(new { succeeded = false, message = localizer["Code is required."].Value });
            }

            var command = new CreateCurrencyCommand
            {
                Name = Name.Trim(),
                Code = Code.Trim().ToUpperInvariant(),
                HtmlCode = HtmlCode?.Trim() ?? string.Empty
            };

            var result = await mediator.Send(command);

            if (result.Success)
            {
                return new JsonResult(new
                {
                    succeeded = true,
                    message = localizer["Currency created successfully."].Value,
                    id = result.Value?.Id
                });
            }

            return new JsonResult(new { succeeded = false, message = result.Message ?? "Failed to create currency." });
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (EditId <= 0)
            {
                return new JsonResult(new { succeeded = false, message = localizer["Invalid currency ID."].Value });
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                return new JsonResult(new { succeeded = false, message = localizer["Name is required."].Value });
            }

            if (string.IsNullOrWhiteSpace(Code))
            {
                return new JsonResult(new { succeeded = false, message = localizer["Code is required."].Value });
            }

            var command = new UpdateCurrencyCommand
            {
                Id = EditId,
                Name = Name.Trim(),
                Code = Code.Trim().ToUpperInvariant(),
                HtmlCode = HtmlCode?.Trim() ?? string.Empty
            };

            var result = await mediator.Send(command);

            if (result.Success)
            {
                return new JsonResult(new
                {
                    succeeded = true,
                    message = localizer["Currency updated successfully."].Value
                });
            }

            return new JsonResult(new { succeeded = false, message = result.Message ?? "Failed to update currency." });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (id <= 0)
            {
                return new JsonResult(new { succeeded = false, message = localizer["Invalid currency ID."].Value });
            }

            var command = new DeleteCurrencyCommand { Id = id };
            var result = await mediator.Send(command);

            if (result.Success)
            {
                return new JsonResult(new
                {
                    succeeded = true,
                    message = localizer["Currency deleted successfully."].Value
                });
            }

            return new JsonResult(new { succeeded = false, message = result.Message ?? "Failed to delete currency." });
        }
    }
}

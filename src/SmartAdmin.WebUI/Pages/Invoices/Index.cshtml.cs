using Application.Common.Interfaces;
using Application.Common.Interfaces.Identity;
using Application.Invoices.Commands.AddEdit;
using Application.Invoices.Commands.Delete;
using Application.Invoices.Commands.Import;
using Application.Invoices.Commands.Upload;
using Application.Invoices.Queries.Export;
using Application.Invoices.Queries.GetRawData;
using Application.Invoices.Queries.Pagination;
using Infrastructure.Constants.Permission;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace SmartAdmin.WebUI.Pages.Invoices
{
    [Authorize(policy: Permissions.Invoices.View)]
    public class IndexModel : PageModel
    {
        [BindProperty]
        public AddEditInvoiceCommand Input { get; set; }
        [BindProperty]
        public IFormFile UploadedFile { get; set; }
        [BindProperty]
        public IFormFile UploadedPhoto { get; set; }

        private readonly IIdentityService _identityService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ISender _mediator;
        private readonly IStringLocalizer<IndexModel> _localizer;

        public IndexModel(
           IIdentityService identityService,
            IAuthorizationService authorizationService,
            ICurrentUserService currentUserService,
            ISender mediator,
            IStringLocalizer<IndexModel> localizer
            )
        {
            _identityService = identityService;
            _authorizationService = authorizationService;
            _currentUserService = currentUserService;
            _mediator = mediator;
            _localizer = localizer;
            var email = _currentUserService.UserId;
        }

        public async Task OnGetAsync()
        {
            var result = await _identityService.FetchUsers("Admin");
        }
        public async Task<IActionResult> OnGetDataAsync([FromQuery] InvoicesWithPaginationQuery command)
        {
            var result = await _mediator.Send(command);
            return new JsonResult(result);
        }
        public async Task<IActionResult> OnPostAsync()
        {
            var result = await _mediator.Send(Input);
            return new JsonResult(result);

        }

        public async Task<IActionResult> OnGetDeleteCheckedAsync([FromQuery] int[] id)
        {
            var command = new DeleteCheckedInvoicesCommand() { Id = id };
            var result = await _mediator.Send(command);
            return new JsonResult("");
        }
        public async Task<IActionResult> OnGetDeleteAsync([FromQuery] int id)
        {
            var command = new DeleteInvoiceCommand() { Id = id };
            var result = await _mediator.Send(command);
            return new JsonResult("");
        }
        public async Task<FileResult> OnPostExportAsync([FromBody] ExportInvoicesQuery command)
        {
            var result = await _mediator.Send(command);
            return File(result, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", _localizer["Invoices"] + ".xlsx");
        }
        public async Task<FileResult> OnGetCreateTemplate()
        {
            var command = new CreateInvoicesTemplateCommand();
            var result = await _mediator.Send(command);
            return File(result, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", _localizer["Invoices"] + ".xlsx");
        }
        public async Task<IActionResult> OnPostImportAsync()
        {
            var stream = new MemoryStream();
            await UploadedFile.CopyToAsync(stream);
            var command = new ImportInvoicesCommand()
            {
                FileName = UploadedFile.FileName,
                Data = stream.ToArray()
            };
            var result = await _mediator.Send(command);
            return new JsonResult(result);
        }
        public async Task<IActionResult> OnPostUploadAsync()
        {
            var stream = new MemoryStream();
            await UploadedPhoto.CopyToAsync(stream);
            var command = new UploadInvoicesCommand()
            {
                FileName = UploadedPhoto.FileName,
                Data = stream.ToArray()
            };
            var result = await _mediator.Send(command);
            return new JsonResult(result);
        }
        public async Task<IActionResult> OnGetRawData([FromQuery] int id)
        {
            var command = new GetInvoiceRawDatasQuery() { InvoiceId = id };
            var result = await _mediator.Send(command);
            return new JsonResult(result);
        }
    }
}

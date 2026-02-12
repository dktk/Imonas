using System.Text;

using Domain;
using Domain.Entities.MedalionData.Silver;

using SG.Common;

namespace Application.Features.Psps.Commands
{
    public class ImportSilverCsvCommandValidator : AbstractValidator<ImportSilverCsvCommand>
    {
        public ImportSilverCsvCommandValidator()
        {
            RuleFor(x => x.FileName)
                .NotEmpty()
                .WithMessage("File name is required.");


            RuleFor(x => x.Data)
                .NotNull()
                .WithMessage("File data is required.")
                .Must(x => x != null && x.Length > 0)
                .WithMessage("File cannot be empty.")
                .Must(x => x == null || x.Length <= 5 * 1024 * 1024)
                .WithMessage("File size must not exceed 5MB.");

            RuleFor(x => x.PspId)
                .GreaterThan(0)
                .WithMessage("PSP selection is required.");

            RuleFor(x => x.RawPaymentId)
                .GreaterThan(0)
                .WithMessage("The reference to the Bronze medallion is missing.");
        }
    }

    public class ImportSilverCsvCommand : IRequest<Result<bool>>
    {
        public string FileName { get; set; }
        public byte[] Data { get; set; }
        public int PspId { get; set; }

        /// <summary>
        /// Reference to the BronzeMedallion 
        /// </summary>
        public int RawPaymentId { get; set; }
    }

    public class ImportSilverCsvCommandHandler(
        ICurrentUserService currentUserService,
        IApplicationDbContext context,
        IComparisonThesaurus comparisonThesaurus,
        IStringLocalizer<ImportSilverCsvCommandHandler> localizer,
        ILogger<ImportSilverCsvCommandHandler> logger,
        IDateTime dateTime) : IRequestHandler<ImportSilverCsvCommand, Result<bool>>
    {

        public async Task<Result<bool>> Handle(ImportSilverCsvCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var psp = await context.Psps.FirstOrDefaultAsync(x => x.Id == request.PspId, cancellationToken);

                // Verify PSP exists

                if (psp == null)
                {
                    return Result<bool>.CreateFailure([$"No PSP exists with ID: {request.PspId}."]);
                }

                logger.LogDebug($"Found {psp.Name} in the DB.");

                var result = true.CreateSuccess();

                if (psp.IsCsvBased)
                {
                    result = await HandleCsvUpload(request, psp, cancellationToken);
                }
                else
                {
                    result = await HandleImport(request, psp, cancellationToken);
                }

                var file = await context.RawPayments.FirstOrDefaultAsync(x => x.Id == request.RawPaymentId);
                file.Status = FileStatus.Uploaded;
                await context.SaveChangesAsync(cancellationToken);

                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(exception: ex, message: request.ToJson());

                var file = await context.RawPayments.FirstOrDefaultAsync(x => x.Id == request.RawPaymentId);
                file.ErrorMessage = ex.Message;
                file.Status = FileStatus.Failed;
                await context.SaveChangesAsync(cancellationToken);

                return Result<bool>.CreateFailure(new[] { $"Error importing CSV: {ex.Message}" });
            }
        }

        private async Task<bool> ImportHomeData(string pspName, string[] transactionIds, CancellationToken token)
        {
            var psp = await context.Psps
                .Include(x => x.InternalSystem)
                .FirstOrDefaultAsync(x => x.Name == pspName, token);

            if (psp == null)
            {
                throw new InvalidOperationException($"The {pspName} is not mapped as a PspMappingSettings in the appconfig.json.");
            }

            var sourceService = comparisonThesaurus.GetInternalService(psp.InternalSystem.Name, SystemType.API);

            var data = await sourceService.GetDataByTransactions(transactionIds);

            // todo: data.NotFound
            //
            await context.InternalPayments.AddRangeAsync(data.Found.Select(x => new InternalPayment
            {
                Amount = x.Amount,
                TxDate = x.RequestDate,
                CurrencyCode = x.CurrencyCode,
                System = x.System,
                ReferenceCode = x.RefNumber,
                Status = x.Status,
                UserEmail = x.Email,
                ClientId = x.AccountId,
                Description = x.Description,
                TxId = x.Id,
                ProviderTxId = x.ProviderTxId,
                UserId = x.AccountId.ToString(),
                Hash = Hasher.Hash(new { x.Amount, x.CurrencyCode, x.Status, x.ProviderTxId }.ToJson())
            }), token);

            await context.SaveChangesAsync(token);

            var minDate = data.Found.Min(x => x.RequestDate);
            var maxDate = data.Found.Max(x => x.RequestDate);

            context.Serilogs.Add(new Domain.Entities.Log.Serilog() { Message = $"Successfully imported internal {pspName} data between {minDate} and {maxDate}. ", Level = "Information", UserName = currentUserService.DisplayName, TimeStamp = dateTime.Now });
            await context.SaveChangesAsync(token);

            return true;
        }

        private async Task<Result<bool>> HandleImport(ImportSilverCsvCommand request, Psp psp, CancellationToken cancellationToken)
        {
            // todo: uncomment this when the time comes
            // await ImportHomeData(psp.Name, transactions.Min(x => x.Date).Date, transactions.Max(x => x.Date).Date, cancellationToken);

            throw new NotImplementedException();
        }

        private async Task<Result<bool>> HandleCsvUpload(ImportSilverCsvCommand request, Psp psp, CancellationToken cancellationToken)
        {
            logger.LogInformation(localizer.GetString("Starting CSV processing flow."));

            // Parse CSV
            // todo: use the CSV Reader here
            var csvContent = Encoding.UTF8.GetString(request.Data);
            var lines = csvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length < 2)
            {
                return Result<bool>.CreateFailure(new[] { "CSV file is empty or contains only headers." });
            }
            logger.LogDebug($"Uploaded file has {lines.Length} lines.");

            logger.LogDebug($"Getting TargetService for {psp.Name}.");
            var methodService = comparisonThesaurus.GetExternalService(psp.Name, SystemTypeMapper.Map(psp.IsCsvBased));

            logger.LogDebug($"Extracting transactions from the CSV file.");
            var transactions = await methodService.GetTransactionsAsync(request.Data);

            await context.ExternalPayments.AddRangeAsync(transactions.Select(x => new ExternalPayment
            {
                Amount = x.Amount,
                BrandId = x.Merchant,
                CurrencyCode = x.Currency,
                ExternalPaymentId = x.Id,
                ExternalSystem = x.PaymentMethod,
                Action = x.Action,
                PspId = psp.Id,
                ClientId = x.ClientId,
                Email = x.Email,
                Description = x.Description,
                ReferenceCode = x.ReferenceCode,
                Status = x.TxStatus,
                TxDate = x.Date,
                TxId = x.TxId,
                RawPaymentId = request.RawPaymentId,

                Hash = Hasher.Hash(new { x.Amount, x.Currency, x.TxStatus, x.TxId }.ToJson())
            }), cancellationToken);

            logger.LogDebug($"Storing {transactions.Length} Silver RawPayments.");
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation($"Processing and storing transactions for {request.FileName} completed successfully.");

            await ImportHomeData(psp.Name, transactions.Select(x => x.TxId).ToArray(), cancellationToken);

            return Result<bool>.CreateSuccess(true);
        }
    }
}

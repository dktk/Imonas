using Domain.Entities.MedalionData.Bronze;

using SG.Common;

namespace Application.Features.Psps.Commands
{
    public class ImportBronzeCsvCommandValidator : AbstractValidator<ImportBronzeCsvCommand>
    {
        public ImportBronzeCsvCommandValidator()
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
        }
    }

    public class ImportBronzeCsvCommand : IRequest<Result<int>>
    {
        public string FileName { get; set; }
        public byte[] Data { get; set; }
        public int PspId { get; set; }
    }

    public class ImportBronzeCsvCommandHandler(
        ICurrentUserService currentUserService,
        IApplicationDbContext context,
        IDateTime dateTime) : IRequestHandler<ImportBronzeCsvCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(ImportBronzeCsvCommand request, CancellationToken cancellationToken)
        {
            //return await HandleJsonPayload(request, cancellationToken);
            return await HandleRawPayload(request, cancellationToken);
        }

        private async Task<Result<int>> HandleRawPayload(ImportBronzeCsvCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var psp = await context.Psps.FindAsync(new object[] { request.PspId }, cancellationToken);

                if (psp == null)
                {
                    return Result<int>.CreateFailure(new[] { "Selected PSP does not exist." });
                }

                var fileHash = FileHashCalculator.ComputeFileHash(request.Data);

                var currentRawPayment = await context.RawPayments.FirstOrDefaultAsync(x => x.PspId == request.PspId && x.FileHash == fileHash);
                if (currentRawPayment != null)
                {
                    return Result<int>.BuildFailure($"{currentRawPayment.FileName} has already been uploaded by {currentRawPayment.UserId} on {currentRawPayment.Created}.");
                }

                var rawPayment = new RawPayment
                {
                    RawContent = request.Data,
                    FileName = request.FileName,
                    FileSizeBytes = request.Data.Length / 1024,
                    PspId = request.PspId,
                    FileHash = fileHash,
                    Status = FileStatus.Pending
                };

                await context.RawPayments.AddAsync(rawPayment, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                context.Serilogs.Add(new Domain.Entities.Log.Serilog() { Message = $"Successfully stored {psp.Name} - {request.FileName}.", Level = "Information", UserName = currentUserService.DisplayName, TimeStamp = dateTime.Now });
                await context.SaveChangesAsync(cancellationToken);

                return Result<int>.CreateSuccess(rawPayment.Id);
            }
            catch (Exception ex)
            {
                return Result<int>.CreateFailure(new[] { $"Error importing CSV: {ex.Message}" });
            }
        }
    }
}

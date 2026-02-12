using Application.Features.Psps.DTOs;

using Dapper;

using Domain.Entities.MedalionData.Gold;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using Npgsql;

using SG.Common;
using SG.Common.Settings;

namespace Application.Features.Psps.Commands
{
    public sealed record ExternalPaymentDto(

        int Id, int Action, string ExternalSystem, string ExternalSystemPaymentId, int PspId, string PspName, bool IsCsvBased, decimal Amount, string CurrencyCode, DateTime TransactionDate, string Status, string ExternalPaymentCreatedBy, DateTime ExternalPaymentCreated, DateTime ExternalPaymentLastModified, string ExternalPaymentLastModifiedBy, string SourceFileName, int RawpaymentId, string RawPaymentCreatedBy, DateTime RawPaymentCreatedDate
    );

    public sealed record PspSettlementDto(
       int ReconciliationRunId, ReconciliationStatus ReconciliationStatus, string CurrencyCode, decimal TotalFees, decimal NetSettlement, int SettlementId, DateTime Created, string CreatedBy, DateTime LastModified, string LastModifiedBy, DateTime TransactionDate, string ExternalSystemPaymentId, string ExternalSystem, string InternalTransactionId, int Amount, int PspId, string PspName, int RawPaymentId
    );

    public class ReconcileDatesForPspCommandDto
    {
        public List<ExternalPaymentDto> UnmatchedTransactions { get; set; }
        public List<PspSettlementDto> MatchedTransactions { get; set; }
    }

    public class ReconcileDatesForPspCommand : ReconciliationDataDto, IRequest<Result<ReconcileDatesForPspCommandDto>>
    {
        public class ReconcileDateForPspCommandHandler(
            ILogger<ReconcileDatesForPspCommand> logger,
            IConfiguration configuration,
            IApplicationDbContext context,
            IOptions<PspDataGatheringSettings> settings,
            ISerilogsService serilogsService,
            ICurrentUserService currentUserService) : IRequestHandler<ReconcileDatesForPspCommand, Result<ReconcileDatesForPspCommandDto>>
        {
            public async Task<Result<ReconcileDatesForPspCommandDto>> Handle(ReconcileDatesForPspCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var (unmatched, matched) = await SettleAsync(request, cancellationToken);
                    return Result<ReconcileDatesForPspCommandDto>.CreateSuccess(new ReconcileDatesForPspCommandDto
                    {
                        UnmatchedTransactions = unmatched,
                        MatchedTransactions = matched
                    });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"An error occured while reconciliating {request.ToJson()}");

                    throw ex;
                }
            }

            private async Task<(List<ExternalPaymentDto> Unmatched, List<PspSettlementDto> Settled)> SettleAsync(ReconcileDatesForPspCommand request, CancellationToken token)
            {
                using var conn = new NpgsqlConnection(configuration.GetConnectionString("DefaultConnection"));
                await conn.OpenAsync(token);

                await using var tx = await conn.BeginTransactionAsync(IsolationLevel.ReadCommitted, token);

                // Generate unique cursor names per call (avoid collisions in same session/tx)
                var unmatchedCursor = $"unmatched_{Guid.NewGuid():N}";
                var settledCursor = $"settled_{Guid.NewGuid():N}";

                // 1) CALL the procedure (must be inside the same transaction as the FETCH)
                // Important: Cast cursor params to refcursor to avoid type issues when binding as text.
                const string callSql = @"
                                CALL gold.sp_settle_psp_transactions(
                                    @p_psp_id,
                                    @p_start_date,
                                    @p_end_date,
                                    @run_id,
                                    @not_settled_cur::refcursor,
                                    @settled_cur::refcursor
                                );";

                var callArgs = new
                {
                    p_psp_id = request.PspId,
                    p_start_date = request.StartDate,
                    p_end_date = request.EndDate,
                    run_id = request.ReconciliationRunId,
                    not_settled_cur = unmatchedCursor,
                    settled_cur = settledCursor
                };

                var callCmd = new CommandDefinition(callSql, callArgs, tx, cancellationToken: token);
                await conn.ExecuteAsync(callCmd);

                // 2) FETCH ALL FROM each cursor
                // Cursor names are identifiers in FETCH, so they must be identifier-quoted.
                var fetchUnmatchedSql = $"FETCH ALL FROM {QuoteIdentifier(unmatchedCursor)};";
                var fetchSettledSql = $"FETCH ALL FROM {QuoteIdentifier(settledCursor)};";

                var unmatchedCmd = new CommandDefinition(fetchUnmatchedSql, transaction: tx, cancellationToken: token);
                var settledCmd = new CommandDefinition(fetchSettledSql, transaction: tx, cancellationToken: token);

                var unmatched = (await conn.QueryAsync<ExternalPaymentDto>(unmatchedCmd)).AsList();
                var settled = (await conn.QueryAsync<PspSettlementDto>(settledCmd)).AsList();

                await tx.CommitAsync(token);

                // change the status of the source file if the settlement happened from a file
                //
                var rawPaymentId = settled.FirstOrDefault()?.RawPaymentId;
                if (rawPaymentId != null)
                {
                    var rawPayment = await context.RawPayments.FirstOrDefaultAsync(x => x.Id == rawPaymentId.Value);
                    if (rawPayment != null)
                    {
                        var status = unmatched.Count == 0 ? FileStatus.Uploaded : FileStatus.Failed;
                        rawPayment.Status = status;

                        await context.SaveChangesAsync(token);
                    }
                }
                

                return (unmatched, settled);
            }

            private static string QuoteIdentifier(string ident)
            {
                // PostgreSQL identifier quoting: " becomes ""
                return "\"" + ident.Replace("\"", "\"\"") + "\"";
            }
        }
    }
}

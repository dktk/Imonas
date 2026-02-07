using System.Text.Json;

using Application.Common.Interfaces;
using Application.Features.Settlement.DTOs;

using Domain.Entities.MedalionData.Gold;
using Domain.Entities.MedalionData.Silver;
using Domain.Entities.Rules;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Settlement
{
    public class SettlementService(
        IApplicationDbContext dbContext,
        IUserService userService,
        ILogger<SettlementService> logger) : ISettlementService
    {
        public async Task<SettlementResultDto> ExecuteSettlementAsync(
            int runId,
            SettlementRunOptionsDto options,
            CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            logger.LogInformation("Starting settlement execution for Run {RunId}", runId);

            // Get the reconciliation run
            var run = await dbContext.ReconciliationRuns
                .FirstOrDefaultAsync(r => r.Id == runId, cancellationToken);

            if (run == null)
            {
                throw new InvalidOperationException($"Reconciliation run {runId} not found.");
            }

            // Update run status to Running
            run.Status = RunStatus.Running;
            run.StartedAt = startTime;
            await dbContext.SaveChangesAsync(cancellationToken);

            var result = new SettlementResultDto
            {
                RunId = runId,
                RunName = run.RunName
            };

            try
            {
                // Get active rules
                var rulesList = await GetActiveRulesAsync(cancellationToken);

                if (!rulesList.Any())
                {
                    logger.LogWarning("No active matching rules found for Run {RunId}", runId);
                    run.Status = RunStatus.Failed;
                    run.ErrorMessage = "No active matching rules configured.";
                    await dbContext.SaveChangesAsync(cancellationToken);
                    return result;
                }

                // Get internal payments (unmatched)
                var internalPaymentsQuery = dbContext.InternalPayments.AsQueryable();

                if (options.StartDate.HasValue)
                    internalPaymentsQuery = internalPaymentsQuery.Where(p => p.TxDate >= options.StartDate.Value);
                if (options.EndDate.HasValue)
                    internalPaymentsQuery = internalPaymentsQuery.Where(p => p.TxDate <= options.EndDate.Value);
                if (!string.IsNullOrEmpty(options.CurrencyCode))
                    internalPaymentsQuery = internalPaymentsQuery.Where(p => p.CurrencyCode == options.CurrencyCode);

                // Exclude already matched payments
                var matchedInternalIds = await dbContext.PspSettlements
                    .Select(s => s.InternalPaymentId)
                    .ToListAsync(cancellationToken);

                var internalPayments = await internalPaymentsQuery
                    .Where(p => !matchedInternalIds.Contains(p.Id))
                    .ToListAsync(cancellationToken);

                // Get external payments (unmatched)
                var externalPaymentsQuery = dbContext.ExternalPayments.AsQueryable();

                if (options.PspId.HasValue)
                    externalPaymentsQuery = externalPaymentsQuery.Where(p => p.PspId == options.PspId.Value);
                if (options.StartDate.HasValue)
                    externalPaymentsQuery = externalPaymentsQuery.Where(p => p.TxDate >= options.StartDate.Value);
                if (options.EndDate.HasValue)
                    externalPaymentsQuery = externalPaymentsQuery.Where(p => p.TxDate <= options.EndDate.Value);
                if (!string.IsNullOrEmpty(options.CurrencyCode))
                    externalPaymentsQuery = externalPaymentsQuery.Where(p => p.CurrencyCode == options.CurrencyCode);

                // Exclude already matched payments
                var matchedExternalIds = await dbContext.PspSettlements
                    .Select(s => s.ExternalPaymentId)
                    .ToListAsync(cancellationToken);

                var externalPayments = await externalPaymentsQuery
                    .Where(p => !matchedExternalIds.Contains(p.Id))
                    .ToListAsync(cancellationToken);

                result.TotalInternalTransactions = internalPayments.Count;
                result.TotalExternalTransactions = externalPayments.Count;

                logger.LogInformation(
                    "Processing {InternalCount} internal and {ExternalCount} external transactions",
                    internalPayments.Count, externalPayments.Count);

                // Track matched external payments to avoid duplicate matches
                var matchedExternals = new HashSet<int>();

                // Process each internal payment
                foreach (var internalPayment in internalPayments)
                {
                    var availableExternals = externalPayments
                        .Where(e => !matchedExternals.Contains(e.Id))
                        .ToList();

                    var matchResult = await FindMatchForInternalPaymentAsync(
                        internalPayment,
                        availableExternals,
                        rulesList);

                    if (matchResult != null && matchResult.IsMatch)
                    {
                        // Find the matched external payment
                        var matchedExternal = availableExternals
                            .FirstOrDefault(e => EvaluateRule(
                                rulesList.First(r => r.RuleName == matchResult.RuleName),
                                internalPayment, e).IsMatch);

                        if (matchedExternal != null)
                        {
                            // Create settlement
                            var settlement = await CreateSettlementAsync(
                                internalPayment,
                                matchedExternal,
                                matchResult,
                                runId,
                                cancellationToken);

                            matchedExternals.Add(matchedExternal.Id);

                            var matchDto = new SettlementMatchDto
                            {
                                InternalPaymentId = internalPayment.Id,
                                ExternalPaymentId = matchedExternal.Id,
                                RuleApplied = matchResult.RuleName,
                                MatchScore = matchResult.Score,
                                Status = matchResult.Score >= 1.0m
                                    ? ReconciliationStatus.Successful
                                    : ReconciliationStatus.Pending,
                                AmountVariance = matchResult.AmountVariance,
                                DateVarianceDays = matchResult.DateVarianceDays,
                                Notes = matchResult.Notes
                            };

                            result.Matches.Add(matchDto);

                            if (matchResult.Score >= 1.0m)
                                result.MatchedCount++;
                            else
                                result.PartialMatchCount++;
                        }
                    }
                    else
                    {
                        // Unmatched internal payment
                        result.UnmatchedInternal.Add(new UnmatchedTransactionDto
                        {
                            Id = internalPayment.Id,
                            TransactionId = internalPayment.TxId,
                            Amount = internalPayment.Amount,
                            CurrencyCode = internalPayment.CurrencyCode,
                            TransactionDate = internalPayment.TxDate.DateTime,
                            Status = internalPayment.Status,
                            Source = "Internal"
                        });
                        result.UnmatchedInternalCount++;
                    }
                }

                // Add unmatched external payments
                foreach (var externalPayment in externalPayments.Where(e => !matchedExternals.Contains(e.Id)))
                {
                    result.UnmatchedExternal.Add(new UnmatchedTransactionDto
                    {
                        Id = externalPayment.Id,
                        TransactionId = externalPayment.ExternalPaymentId,
                        Amount = externalPayment.Amount,
                        CurrencyCode = externalPayment.CurrencyCode,
                        TransactionDate = externalPayment.TxDate,
                        Status = externalPayment.Status,
                        Source = "External"
                    });
                    result.UnmatchedExternalCount++;
                }

                // Calculate match percentage
                var totalTransactions = result.TotalInternalTransactions + result.TotalExternalTransactions;
                if (totalTransactions > 0)
                {
                    result.MatchPercentage = (decimal)(result.MatchedCount * 2 + result.PartialMatchCount) / totalTransactions * 100;
                }

                // Update run with results
                run.Status = RunStatus.Completed;
                run.CompletedAt = DateTime.UtcNow;
                run.TotalRecords = totalTransactions;
                run.MatchedRecords = result.MatchedCount;
                run.UnmatchedRecords = result.UnmatchedInternalCount + result.UnmatchedExternalCount;
                run.PartialMatchRecords = result.PartialMatchCount;
                run.MatchPercentage = result.MatchPercentage;

                await dbContext.SaveChangesAsync(cancellationToken);

                result.Duration = DateTime.UtcNow - startTime;

                logger.LogInformation(
                    "Settlement completed for Run {RunId}. Matched: {Matched}, Partial: {Partial}, Unmatched: {Unmatched}",
                    runId, result.MatchedCount, result.PartialMatchCount,
                    result.UnmatchedInternalCount + result.UnmatchedExternalCount);

                // Create cases for exceptions if enabled
                if (options.CreateCasesForExceptions && result.UnmatchedInternalCount > 0)
                {
                    await CreateExceptionCasesAsync(result.UnmatchedInternal, runId, options.CurrentUserId, cancellationToken);
                }

                // Create cases for exceptions if enabled
                if (options.CreateCasesForExceptions && result.UnmatchedExternalCount > 0)
                {
                    await CreateExceptionCasesAsync(result.UnmatchedExternal, runId, options.CurrentUserId, cancellationToken);
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Settlement execution failed for Run {RunId}", runId);

                run.Status = RunStatus.Failed;
                run.CompletedAt = DateTime.UtcNow;
                run.ErrorMessage = ex.Message;
                await dbContext.SaveChangesAsync(cancellationToken);

                throw;
            }
        }

        public async Task<RuleMatchResult?> FindMatchForInternalPaymentAsync(
            InternalPayment internalPayment,
            IEnumerable<ExternalPayment> externalPayments,
            IEnumerable<MatchingRule> rules)
        {
            RuleMatchResult? bestMatch = null;

            foreach (var rule in rules.OrderBy(r => r.Priority))
            {
                var matchedExternalPayments = externalPayments.Where(e => e.ExternalPaymentId == internalPayment.ProviderTxId);

                foreach (var externalPayment in matchedExternalPayments)
                {
                    var result = EvaluateRule(rule, internalPayment, externalPayment);

                    if (result.IsMatch)
                    {
                        if (bestMatch == null || result.Score > bestMatch.Score)
                        {
                            bestMatch = result;
                        }

                        // Stop at first match if rule specifies
                        if (rule.StopAtFirstMatch && result.Score >= 1.0m)
                        {
                            return bestMatch;
                        }
                    }
                }

                // If we found a perfect match with this rule, no need to check lower priority rules
                if (bestMatch != null && bestMatch.Score >= 1.0m)
                {
                    break;
                }
            }

            return bestMatch;
        }

        public RuleMatchResult EvaluateRule(
            MatchingRule rule,
            InternalPayment internalPayment,
            ExternalPayment externalPayment)
        {
            var result = new RuleMatchResult
            {
                RuleName = rule.RuleName,
                Score = 0m,
                IsMatch = false
            };

            try
            {
                switch (rule.RuleType)
                {
                    case RuleType.Equality:
                        return EvaluateEqualityRule(rule, internalPayment, externalPayment);

                    case RuleType.ToleranceWindow:
                        return EvaluateToleranceRule(rule, internalPayment, externalPayment);

                    case RuleType.Composite:
                        return EvaluateCompositeRule(rule, internalPayment, externalPayment);

                    case RuleType.Fuzzy:
                        return EvaluateFuzzyRule(rule, internalPayment, externalPayment);

                    default:
                        logger.LogWarning("Unknown rule type {RuleType} for rule {RuleName}", rule.RuleType, rule.RuleName);
                        return result;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error evaluating rule {RuleName}", rule.RuleName);
                return result;
            }
        }

        private RuleMatchResult EvaluateEqualityRule(
            MatchingRule rule,
            InternalPayment internalPayment,
            ExternalPayment externalPayment)
        {
            var result = new RuleMatchResult
            {
                RuleName = rule.RuleName,
                Score = 0m,
                IsMatch = false
            };

            // Parse rule definition for field mappings
            var definition = ParseRuleDefinition(rule.RuleDefinition);

            // Default equality check: Amount, Currency, and Transaction ID match
            var amountMatch = internalPayment.Amount == externalPayment.Amount;
            var currencyMatch = internalPayment.CurrencyCode == externalPayment.CurrencyCode;

            // Check for transaction ID match (ProviderTxId should match ExternalPaymentId)
            var txIdMatch = !string.IsNullOrEmpty(internalPayment.ProviderTxId) &&
                           internalPayment.ProviderTxId == externalPayment.ExternalPaymentId;

            if (amountMatch && currencyMatch && txIdMatch)
            {
                result.IsMatch = true;
                result.Score = 1.0m;
                result.Notes = "Exact match on Amount, Currency, and Transaction ID";
            }
            else if (amountMatch && currencyMatch)
            {
                // Partial match - amount and currency match but not tx id
                var allowPartial = definition.TryGetValue("allowPartialMatch", out var val) && val is bool b && b;
                result.IsMatch = allowPartial;
                result.Score = 0.7m;
                result.Notes = "Amount and Currency match, Transaction ID mismatch";
            }

            result.AmountVariance = externalPayment.Amount - internalPayment.Amount;

            return result;
        }

        private RuleMatchResult EvaluateToleranceRule(
            MatchingRule rule,
            InternalPayment internalPayment,
            ExternalPayment externalPayment)
        {
            var result = new RuleMatchResult
            {
                RuleName = rule.RuleName,
                Score = 0m,
                IsMatch = false
            };

            // Currency must always match
            if (internalPayment.CurrencyCode != externalPayment.CurrencyCode)
            {
                return result;
            }

            // Check amount tolerance
            var amountVariance = Math.Abs(externalPayment.Amount - internalPayment.Amount);
            var toleranceAmount = rule.ToleranceAmount ?? 0m;
            var amountWithinTolerance = amountVariance <= toleranceAmount;

            // Check date tolerance
            var dateVariance = Math.Abs((externalPayment.TxDate - internalPayment.TxDate.DateTime).Days);
            var toleranceDays = rule.ToleranceWindowDays ?? 0;
            var dateWithinTolerance = dateVariance <= toleranceDays;

            if (amountWithinTolerance && dateWithinTolerance)
            {
                result.IsMatch = true;

                // Calculate score based on how close the match is
                var amountScore = toleranceAmount > 0
                    ? 1m - (amountVariance / toleranceAmount) * 0.3m
                    : (amountVariance == 0 ? 1m : 0m);

                var dateScore = toleranceDays > 0
                    ? 1m - ((decimal)dateVariance / toleranceDays) * 0.2m
                    : (dateVariance == 0 ? 1m : 0m);

                result.Score = Math.Min(amountScore, dateScore);
                result.AmountVariance = externalPayment.Amount - internalPayment.Amount;
                result.DateVarianceDays = dateVariance;
                result.Notes = $"Within tolerance: Amount variance {amountVariance:C}, Date variance {dateVariance} days";
            }

            return result;
        }

        private RuleMatchResult EvaluateCompositeRule(
            MatchingRule rule,
            InternalPayment internalPayment,
            ExternalPayment externalPayment)
        {
            var result = new RuleMatchResult
            {
                RuleName = rule.RuleName,
                Score = 0m,
                IsMatch = false
            };

            var definition = ParseRuleDefinition(rule.RuleDefinition);
            var conditions = new List<(string name, bool matched, decimal weight)>();

            // Check each field specified in the composite rule
            if (definition.TryGetValue("fields", out var fieldsObj) && fieldsObj is JsonElement fields)
            {
                foreach (var field in fields.EnumerateArray())
                {
                    var fieldName = field.GetString() ?? string.Empty;
                    var weight = 100m; // todo: field.TryGetProperty("weight", out var w) ? w.GetDecimal() : 1m;
                    var matched = EvaluateFieldMatch(fieldName, internalPayment, externalPayment);
                    conditions.Add((fieldName, matched, weight));
                }
            }
            else
            {
                // Default composite: Amount, Currency, Date
                conditions.Add(("Amount", internalPayment.Amount == externalPayment.Amount, 0.4m));
                conditions.Add(("Currency", internalPayment.CurrencyCode == externalPayment.CurrencyCode, 0.2m));
                conditions.Add(("Date", Math.Abs((internalPayment.TxDate.DateTime - externalPayment.TxDate).Days) <= 1, 0.2m));
                conditions.Add(("TxId", internalPayment.ProviderTxId == externalPayment.ExternalPaymentId, 0.2m));
            }

            var totalWeight = conditions.Sum(c => c.weight);
            var matchedWeight = conditions.Where(c => c.matched).Sum(c => c.weight);

            result.Score = totalWeight > 0 ? matchedWeight / totalWeight : 0m;
            result.IsMatch = result.Score * 100 >= (rule.MinimumScore ?? 70m);
            result.AmountVariance = externalPayment.Amount - internalPayment.Amount;
            result.Notes = $"Composite match: {conditions.Count(c => c.matched)}/{conditions.Count} conditions met";

            return result;
        }

        private RuleMatchResult EvaluateFuzzyRule(
            MatchingRule rule,
            InternalPayment internalPayment,
            ExternalPayment externalPayment)
        {
            var result = new RuleMatchResult
            {
                RuleName = rule.RuleName,
                Score = 0m,
                IsMatch = false
            };

            // Currency must always match
            if (internalPayment.CurrencyCode != externalPayment.CurrencyCode)
            {
                return result;
            }

            var scores = new List<decimal>();

            // Amount similarity (with percentage tolerance)
            var amountDiff = Math.Abs(externalPayment.Amount - internalPayment.Amount);
            var maxAmount = Math.Max(externalPayment.Amount, internalPayment.Amount);
            var amountSimilarity = maxAmount > 0 ? 1m - (amountDiff / maxAmount) : 1m;
            scores.Add(amountSimilarity);

            // Date similarity
            var daysDiff = Math.Abs((externalPayment.TxDate - internalPayment.TxDate.DateTime).Days);
            var dateSimilarity = Math.Max(0, 1m - (daysDiff * 0.1m)); // 10% penalty per day
            scores.Add(dateSimilarity);

            // Transaction ID similarity (if both have values)
            if (!string.IsNullOrEmpty(internalPayment.ProviderTxId) &&
                !string.IsNullOrEmpty(externalPayment.ExternalPaymentId))
            {
                var txIdSimilarity = CalculateStringSimilarity(
                    internalPayment.ProviderTxId,
                    externalPayment.ExternalPaymentId);
                scores.Add(txIdSimilarity);
            }

            result.Score = scores.Any() ? scores.Average() : 0m;
            result.IsMatch = result.Score >= (rule.MinimumScore ?? 0.8m);
            result.AmountVariance = externalPayment.Amount - internalPayment.Amount;
            result.DateVarianceDays = daysDiff;
            result.Notes = $"Fuzzy match score: {result.Score:P2}";

            return result;
        }

        private bool EvaluateFieldMatch(string fieldName, InternalPayment internal_, ExternalPayment external)
        {
            return fieldName.ToLower() switch
            {
                "amount" => internal_.Amount == external.Amount,
                "currency" or "currencycode" => internal_.CurrencyCode == external.CurrencyCode,
                "date" or "txdate" => Math.Abs((internal_.TxDate.DateTime - external.TxDate).Days) <= 1,
                "txid" or "transactionid" => internal_.ProviderTxId == external.ExternalPaymentId,
                "status" => internal_.Status == external.Status,
                _ => false
            };
        }

        private Dictionary<string, object> ParseRuleDefinition(string ruleDefinition)
        {
            if (string.IsNullOrWhiteSpace(ruleDefinition))
                return new Dictionary<string, object>();

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object>>(ruleDefinition)
                       ?? new Dictionary<string, object>();
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }

        private decimal CalculateStringSimilarity(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
                return 0m;

            if (s1 == s2)
                return 1m;

            // Simple Levenshtein-based similarity
            var distance = LevenshteinDistance(s1.ToLower(), s2.ToLower());
            var maxLength = Math.Max(s1.Length, s2.Length);
            return maxLength > 0 ? 1m - ((decimal)distance / maxLength) : 0m;
        }

        private int LevenshteinDistance(string s1, string s2)
        {
            var n = s1.Length;
            var m = s2.Length;
            var d = new int[n + 1, m + 1];

            for (var i = 0; i <= n; i++) d[i, 0] = i;
            for (var j = 0; j <= m; j++) d[0, j] = j;

            for (var i = 1; i <= n; i++)
            {
                for (var j = 1; j <= m; j++)
                {
                    var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[n, m];
        }

        public async Task<List<MatchingRule>> GetActiveRulesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            return await dbContext.MatchingRules
                .Where(r => r.IsActive)
                .Where(r => r.EffectiveFrom <= now)
                .Where(r => r.EffectiveTo == null || r.EffectiveTo >= now)
                .OrderBy(r => r.Priority)
                .ToListAsync(cancellationToken);
        }

        public async Task<PspSettlement> CreateSettlementAsync(
            InternalPayment internalPayment,
            ExternalPayment externalPayment,
            RuleMatchResult matchResult,
            int runId,
            CancellationToken cancellationToken = default)
        {
            var settlement = new PspSettlement
            {
                InternalPaymentId = internalPayment.Id,
                ExternalPaymentId = externalPayment.Id,
                ReconciliationRunId = runId,
                PspId = externalPayment.PspId,
                CurrencyCode = externalPayment.CurrencyCode,
                Amount = (int)externalPayment.Amount,
                TxDate = externalPayment.TxDate,
                TotalFees = 0, // Would be calculated from fee contracts
                NetSettlement = externalPayment.Amount,
                ReconciliationStatus = matchResult.Score >= 1.0m
                    ? ReconciliationStatus.Successful
                    : ReconciliationStatus.Pending,
                ReconciliationComments = new List<ReconciliationComment>()
            };

            if (!string.IsNullOrEmpty(matchResult.Notes))
            {
                var reconciliationComment = new ReconciliationComment
                {
                    Text = $"Matched by rule '{matchResult.RuleName}': {matchResult.Notes}",
                    Created = DateTime.UtcNow,
                    Reconciliation = settlement
                };

                // fk_reconciliation_comments_psp_settlements_reconciliation_id
                settlement.ReconciliationComments.Add(reconciliationComment);

            }

            try
            {
                dbContext.PspSettlements.Add(settlement);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return settlement;
        }

        private async Task CreateExceptionCasesAsync(
            List<UnmatchedTransactionDto> unmatchedTransactions,
            int runId,
            string? currentUserId,
            CancellationToken cancellationToken)
        {
            // Determine who should be assigned the cases based on user role
            var assigneeId = await userService.GetCaseAssigneeAsync(currentUserId);

            foreach (var transaction in unmatchedTransactions.Take(100)) // Limit to 100 cases per run
            {
                var caseNumber = $"CASE-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

                var exceptionCase = new Domain.Entities.Cases.ExceptionCase
                {
                    CaseNumber = caseNumber,
                    Title = $"Unmatched {transaction.Source} Transaction",
                    Description = $"Transaction {transaction.TransactionId} with amount {transaction.Amount} {transaction.CurrencyCode} dated {transaction.TransactionDate:d} could not be matched during reconciliation run.",
                    Status = CaseStatus.Open,
                    Severity = transaction.Amount > 10000 ? CaseSeverity.High :
                              transaction.Amount > 1000 ? CaseSeverity.Medium : CaseSeverity.Low,
                    VarianceType = VarianceType.Amount,
                    VarianceAmount = transaction.Amount,
                    ReconciliationRunId = runId,
                    AssignedTo = assigneeId
                };

                dbContext.ExceptionCases.Add(exceptionCase);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Created {Count} exception cases for unmatched transactions, assigned to user {AssigneeId}",
                Math.Min(unmatchedTransactions.Count, 100), assigneeId ?? "unassigned");
        }
    }
}

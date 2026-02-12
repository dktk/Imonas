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
                    .Include(x => x.Psp)
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

                    var (matchResult, externalTransaction) = await FindMatchForInternalPaymentAsync(
                        internalPayment,
                        availableExternals,
                        rulesList);

                    // Completely Settled
                    //
                    if (matchResult != null && matchResult.Value == RuleMatchResultType.Match)
                    {
                        // Find the matched external payment
                        var matchedExternal = availableExternals
                            .FirstOrDefault(e => EvaluateRule(
                                rulesList.First(r => r.RuleName == matchResult.RuleName),
                                internalPayment, e).Value == RuleMatchResultType.Match);

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
                        }

                        continue;
                    }

                    var unmatchedInternal = new UnmatchedTransactionDto
                    {
                        Id = internalPayment.Id,
                        TransactionId = internalPayment.TxId,
                        Amount = internalPayment.Amount,
                        CurrencyCode = internalPayment.CurrencyCode,
                        TransactionDate = internalPayment.TxDate.DateTime,
                        Status = internalPayment.Status,
                        Source = internalPayment.System
                    };

                    var unmatchedExternal = externalTransaction != null ?
                        new UnmatchedTransactionDto
                        {
                            Amount = externalTransaction.Amount,
                            CurrencyCode = externalTransaction.CurrencyCode,
                            Id = externalTransaction.Id,
                            Source = externalTransaction.Psp.Name,
                            Status = externalTransaction.Status,
                            TransactionDate = externalTransaction.TxDate,
                            TransactionId = externalTransaction.TxId
                        }
                        : null;

                    // Partially Settled
                    //
                    if (matchResult != null && matchResult.Value == RuleMatchResultType.PartialMatch)
                    {
                        result.PartialMatches.Add((unmatchedInternal, unmatchedExternal));

                        continue;
                    }


                    // Unmatched internal payment
                    //
                    result.UnmatchedInternal.Add(unmatchedInternal);

                    if (unmatchedExternal != null)
                    {
                        result.UnmatchedExternal.Add(unmatchedExternal);
                    }
                }

                // Add unmatched external payments
                //foreach (var externalPayment in externalPayments.Where(e => !matchedExternals.Contains(e.Id)))
                //{
                //    result.UnmatchedExternal.Add(new UnmatchedTransactionDto
                //    {
                //        Id = externalPayment.Id,
                //        TransactionId = externalPayment.ExternalPaymentId,
                //        Amount = externalPayment.Amount,
                //        CurrencyCode = externalPayment.CurrencyCode,
                //        TransactionDate = externalPayment.TxDate,
                //        Status = externalPayment.Status,
                //        Source = externalPayment.Psp.Name
                //    });
                //    result.ExternalUnmatchedCount++;
                //}

                // Calculate match percentage
                var totalTransactions = result.TotalExternalTransactions + result.TotalInternalTransactions;
                if (totalTransactions > 0)
                {
                    result.MatchPercentage = (decimal)(result.MatchedCount * 2 + result.PartialMatchCount) / totalTransactions * 100;
                }

                // Update run with results
                run.Status = RunStatus.Completed;
                run.CompletedAt = DateTime.UtcNow;
                run.InternalMatchedRecordsCount = result.TotalInternalTransactions;
                run.ExternalMatchedRecordsCount = result.TotalExternalTransactions;
                run.InternalUnmatchedRecordsCount = result.InternalUnmatchedCount;
                run.ExternalUnmatchedRecordsCount = result.ExternalUnmatchedCount;

                run.PartialMatchRecordsCount = result.PartialMatchCount;
                run.MatchPercentage = result.MatchPercentage;

                await dbContext.SaveChangesAsync(cancellationToken);

                result.Duration = DateTime.UtcNow - startTime;

                logger.LogInformation(
                    "Settlement completed for Run {RunId}. Matched: {Matched}, Partial: {Partial}, Unmatched: {Unmatched}",
                    runId, result.MatchedCount, result.PartialMatchCount,
                    result.InternalUnmatchedCount + result.ExternalUnmatchedCount);

                // Create cases for exceptions if enabled
                if (options.CreateCasesForExceptions && result.InternalUnmatchedCount > 0)
                {
                    await CreateExceptionCasesAsync(result.UnmatchedInternal, runId, options.CurrentUserId, cancellationToken);
                }

                // Create cases for exceptions if enabled
                if (options.CreateCasesForExceptions && result.ExternalUnmatchedCount > 0)
                {
                    await CreateExceptionCasesAsync(result.UnmatchedExternal, runId, options.CurrentUserId, cancellationToken);
                }

                if (options.CreateCasesForExceptions && result.PartialMatchCount > 0)
                {
                    await CreateExceptionCasesAsync(result.PartialMatches, runId, options.CurrentUserId, cancellationToken);
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

        private async Task CreateExceptionCasesAsync(List<(UnmatchedTransactionDto, UnmatchedTransactionDto)> partialMatches, int runId, string currentUserId, CancellationToken cancellationToken)
        {
            // Determine who should be assigned the cases based on user role
            var assigneeId = await userService.GetCaseAssigneeAsync(currentUserId);

            foreach (var (internalTransaction, externalTransaction) in partialMatches.Take(100)) // todo: configure this limit 
            {
                var caseNumber = $"CASE-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

                var amountDiff = Math.Abs(internalTransaction.Amount - externalTransaction?.Amount ?? 0);

                var exceptionCase = new Domain.Entities.Cases.ExceptionCase
                {
                    CaseNumber = caseNumber,
                    Title = $"Partial {externalTransaction?.Source} Transaction",
                    Description = $"<strong>{internalTransaction.Source}</strong> {internalTransaction.TransactionId} partialy matches <strong>{externalTransaction.Source}</strong> {externalTransaction?.TransactionId}.",
                    InternalTransactionId = internalTransaction.Id,
                    ExternalTransactionId = externalTransaction?.Id,
                    Status = CaseStatus.Open,
                    // todo: configure the 100 value
                    Severity = amountDiff > 100 ? CaseSeverity.High : CaseSeverity.Medium,
                    ReconciliationRunId = runId,
                    AssignedToId = assigneeId
                };

                dbContext.ExceptionCases.Add(exceptionCase);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Created {Count} exception cases for partially matched transactions, assigned to user {AssigneeId}",
                Math.Min(partialMatches.Count, 100), assigneeId ?? "unassigned");
        }

        public async Task<(RuleMatchResult?, ExternalPayment)> FindMatchForInternalPaymentAsync(
            InternalPayment internalPayment,
            IEnumerable<ExternalPayment> externalPayments,
            IEnumerable<MatchingRule> rules)
        {
            RuleMatchResult? bestMatch = null;

            var matchedExternalPayments = externalPayments.Where(e => e.TxId == internalPayment.ProviderTxId);

            foreach (var rule in rules.OrderBy(r => r.Priority))
            {
                foreach (var externalPayment in matchedExternalPayments)
                {
                    var result = EvaluateRule(rule, internalPayment, externalPayment);

                    if (result.Value == RuleMatchResultType.Match || result.Value == RuleMatchResultType.PartialMatch)
                    {
                        if (bestMatch == null || result.Score > bestMatch.Score)
                        {
                            bestMatch = result;
                        }

                        // Stop at first match if rule specifies
                        if (rule.StopAtFirstMatch && result.Score >= 1.0m)
                        {
                            return (bestMatch, externalPayment);
                        }
                    }
                }

                // If we found a perfect match with this rule, no need to check lower priority rules
                if (bestMatch != null && bestMatch.Score >= 1.0m)
                {
                    break;
                }
            }

            return (bestMatch, matchedExternalPayments.LastOrDefault());
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
                Value = RuleMatchResultType.NoMatch
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
                Value = RuleMatchResultType.NoMatch
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
                result.Value = RuleMatchResultType.Match;
                result.Score = 1.0m;
                result.Notes = "Exact match on Amount, Currency, and Transaction ID";
            }
            else if (txIdMatch && currencyMatch)
            {
                // Partial match - amount and currency match but not tx id
                var allowPartial = definition.TryGetValue("allowPartialMatch", out var val) && val is bool b && b;
                // todo:
                result.Value = RuleMatchResultType.PartialMatch;
                result.Score = 0.7m;
                result.Notes = "Transaction ID and Currency match, Amount mismatch";
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
                Value = RuleMatchResultType.NoMatch
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
                result.Value = RuleMatchResultType.Match;

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

        private Dictionary<int, Dictionary<string, object>> ruleDefinitions = new Dictionary<int, Dictionary<string, object>>();
        private Dictionary<string, object> GetRuleDefinition(MatchingRule rule)
        {
            if (ruleDefinitions.ContainsKey(rule.Id))
            {
                return ruleDefinitions[rule.Id];
            }

            var definition = ParseRuleDefinition(rule.RuleDefinition);
            ruleDefinitions[rule.Id] = definition;

            return definition;
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
                Value = RuleMatchResultType.NoMatch
            };

            var definition = GetRuleDefinition(rule);

            var conditions = new List<(string name, bool matched, decimal weight)>();

            // Check each field specified in the composite rule
            if (definition.TryGetValue("fields", out var fieldsObj) && fieldsObj is JsonElement fields)
            {
                foreach (var field in fields.EnumerateArray())
                {
                    var fieldName = field.GetString() ?? string.Empty;
                    var weight = 100m; // todo: field.TryGetProperty("weight", out var w) ? w.GetDecimal() : 1m;
                    var (matched, fieldInfo) = EvaluateFieldMatch(fieldName, internalPayment, externalPayment);

                    if (!matched)
                    {
                        result.UnmatchedFields.Add(fieldInfo);
                    }

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
            result.Value = (result.Score * 100) switch
            {
                100 => RuleMatchResultType.Match,

                (> 0) => RuleMatchResultType.PartialMatch,

                _ => RuleMatchResultType.NoMatch
            };

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
                Value = RuleMatchResultType.NoMatch
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
            if (result.Score == 100)
            {
                result.Value = RuleMatchResultType.Match;
            }
            else if (result.Score > rule.MinimumScore)
            {
                result.Value = RuleMatchResultType.PartialMatch;
            } else
            {
                result.Value = RuleMatchResultType.NoMatch;
            }

            result.AmountVariance = externalPayment.Amount - internalPayment.Amount;
            result.DateVarianceDays = daysDiff;
            result.Notes = $"Fuzzy match score: {result.Score:P2}";

            return result;
        }

        private static readonly (bool, (string, string, string)) MatchedResult = (true, (String.Empty, String.Empty, String.Empty));
        private const string AMOUNT_PROP_NAME = "amount";
        private const string CURRENCY_PROP_NAME = "currency";
        private const string CURRENCY_CODE_PROP_NAME = "currencycode";
        private const string DATE_PROP_NAME = "date";
        private const string TXDATE_PROP_NAME = "txdate";
        private const string TXID_PROP_NAME = "txid";
        private const string STATUS_PROP_NAME = "status";
        private const string TRANSACTIONID_PROP_NAME = "transactionid";
        private const string EXTERNAL_PAYMENTID_PROPNAME = "externalpaymentid";
        private const string USER_EMAIL_PROP_NAME = "useremail";
        private const string USER_ID_PROP_NAME = "userid";
        private const string DESCRIPTION_PROP_NAME = "descrition";
        private const string REFERENCE_CODE_PROP_NAME = "referencenumber";

        private (bool, (string, string, string)) EvaluateFieldMatch(string fieldName, InternalPayment @internal, ExternalPayment external)
        {
            var field = fieldName.ToLower();

            // todo: unit test this
            return field switch
            {
                AMOUNT_PROP_NAME => @internal.Amount == external.Amount ? MatchedResult : (false, (field, @internal.Amount.ToString(), external.Amount.ToString())),

                CURRENCY_PROP_NAME or CURRENCY_CODE_PROP_NAME => @internal.CurrencyCode == external.CurrencyCode ? MatchedResult : (false, (field, @internal.CurrencyCode, external.CurrencyCode)),
                DATE_PROP_NAME or TXDATE_PROP_NAME => Math.Abs((@internal.TxDate.DateTime - external.TxDate).Days) <= 1 ? MatchedResult : (false, (field, @internal.TxDate.DateTime.ToString(), external.TxDate.ToString())),
                TXID_PROP_NAME or TRANSACTIONID_PROP_NAME => @internal.ProviderTxId == external.TxId ? MatchedResult : (false, (field, @internal.ProviderTxId, external.TxId.ToString())),
                STATUS_PROP_NAME => @internal.Status == external.Status ? MatchedResult : (false, (field, @internal.Status, external.Status)),
                EXTERNAL_PAYMENTID_PROPNAME => @internal.ReferenceCode == external.ExternalPaymentId ? MatchedResult : (false, (field, @internal.ReferenceCode, external.ExternalPaymentId)),
                USER_EMAIL_PROP_NAME => @internal.UserEmail == external.Email ? MatchedResult : (false, (field, @internal.UserEmail, external.Email)),
                USER_ID_PROP_NAME => @internal.ClientId == external.ClientId ? MatchedResult : (false, (field, @internal.ClientId.ToString(), external.ClientId.ToString())),
                DESCRIPTION_PROP_NAME => @internal.Description == external.Description ? MatchedResult : (false, (field, @internal.Description, external.Description)),
                REFERENCE_CODE_PROP_NAME => @internal.ReferenceCode == external.ReferenceCode ? MatchedResult : (false, (field, @internal.ReferenceCode, external.ReferenceCode)),

                _ => (false, (field, string.Empty, string.Empty))
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

        public async Task<SimulationResultDto> SimulateAsync(
            SettlementRunOptionsDto options,
            List<int> candidateRuleIds,
            decimal falsePositiveThreshold = 0.7m,
            CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            logger.LogInformation("Starting simulation with {CandidateCount} candidate rules", candidateRuleIds.Count);

            // Load ALL internal payments (no unmatched filter)
            var internalPaymentsQuery = dbContext.InternalPayments.AsQueryable();
            if (options.StartDate.HasValue)
                internalPaymentsQuery = internalPaymentsQuery.Where(p => p.TxDate >= options.StartDate.Value);
            if (options.EndDate.HasValue)
                internalPaymentsQuery = internalPaymentsQuery.Where(p => p.TxDate <= options.EndDate.Value);
            if (!string.IsNullOrEmpty(options.CurrencyCode))
                internalPaymentsQuery = internalPaymentsQuery.Where(p => p.CurrencyCode == options.CurrencyCode);
            var internalPayments = await internalPaymentsQuery.ToListAsync(cancellationToken);

            // Load ALL external payments (no unmatched filter)
            var externalPaymentsQuery = dbContext.ExternalPayments.AsQueryable();
            if (options.PspId.HasValue)
                externalPaymentsQuery = externalPaymentsQuery.Where(p => p.PspId == options.PspId.Value);
            if (options.StartDate.HasValue)
                externalPaymentsQuery = externalPaymentsQuery.Where(p => p.TxDate >= options.StartDate.Value);
            if (options.EndDate.HasValue)
                externalPaymentsQuery = externalPaymentsQuery.Where(p => p.TxDate <= options.EndDate.Value);
            if (!string.IsNullOrEmpty(options.CurrencyCode))
                externalPaymentsQuery = externalPaymentsQuery.Where(p => p.CurrencyCode == options.CurrencyCode);
            var externalPayments = await externalPaymentsQuery.ToListAsync(cancellationToken);

            // Load baseline (active) rules
            var baselineRules = await GetActiveRulesAsync(cancellationToken);

            // Load candidate rules by ID (regardless of IsActive)
            var candidateRules = await dbContext.MatchingRules
                .Where(r => candidateRuleIds.Contains(r.Id))
                .OrderBy(r => r.Priority)
                .ToListAsync(cancellationToken);

            // Load existing verified settlements for false-positive comparison
            var verifiedPairs = await dbContext.PspSettlements
                .Where(s => s.ReconciliationStatus == ReconciliationStatus.Successful)
                .Select(s => new { s.InternalPaymentId, s.ExternalPaymentId })
                .ToListAsync(cancellationToken);
            var verifiedSet = new HashSet<(int, int)>(
                verifiedPairs.Select(p => (p.InternalPaymentId, p.ExternalPaymentId)));

            // Run matching for both rule sets
            var baselineResult = RunMatchingInMemory(internalPayments, externalPayments, baselineRules);
            var candidateResult = RunMatchingInMemory(internalPayments, externalPayments, candidateRules);

            // Compute lift and false positives
            var lift = ComputeLift(baselineResult, candidateResult);
            var falsePositives = ComputeFalsePositives(candidateResult, verifiedSet, falsePositiveThreshold);

            logger.LogInformation(
                "Simulation completed. Baseline: {BaselineMatched} matched, Candidate: {CandidateMatched} matched, Lift: {Lift}",
                baselineResult.MatchedCount, candidateResult.MatchedCount, lift.MatchedDelta);

            return new SimulationResultDto
            {
                Duration = DateTime.UtcNow - startTime,
                SimulatedAt = DateTime.UtcNow,
                TotalInternalTransactions = internalPayments.Count,
                TotalExternalTransactions = externalPayments.Count,
                Baseline = baselineResult,
                Candidate = candidateResult,
                Lift = lift,
                FalsePositives = falsePositives,
                BaselineRuleIds = baselineRules.Select(r => r.Id).ToList(),
                CandidateRuleIds = candidateRuleIds
            };
        }

        private SimulationRuleSetResultDto RunMatchingInMemory(
            List<InternalPayment> internalPayments,
            List<ExternalPayment> externalPayments,
            List<MatchingRule> rules)
        {
            var result = new SimulationRuleSetResultDto();
            var matchedExternals = new HashSet<int>();

            if (!rules.Any())
                return result;

            foreach (var internalPayment in internalPayments)
            {
                var availableExternals = externalPayments
                    .Where(e => !matchedExternals.Contains(e.Id))
                    .ToList();

                var (matchResult, externalPayment) = FindMatchForInternalPaymentAsync(
                    internalPayment, availableExternals, rules).GetAwaiter().GetResult();

                if (matchResult != null && matchResult.Value == RuleMatchResultType.Match)
                {
                    var matchedExternal = availableExternals
                        .FirstOrDefault(e => EvaluateRule(
                            rules.First(r => r.RuleName == matchResult.RuleName),
                            internalPayment, e).Value == RuleMatchResultType.Match);

                    if (matchedExternal != null)
                    {
                        matchedExternals.Add(matchedExternal.Id);

                        if (result.Matches.Count < 500)
                        {
                            result.Matches.Add(new SimulationMatchDto
                            {
                                InternalPaymentId = internalPayment.Id,
                                ExternalPaymentId = matchedExternal.Id,
                                InternalTxId = internalPayment.TxId,
                                ExternalTxId = matchedExternal.TxId,
                                Amount = internalPayment.Amount,
                                CurrencyCode = internalPayment.CurrencyCode,
                                RuleApplied = matchResult.RuleName,
                                MatchScore = matchResult.Score,
                                AmountVariance = matchResult.AmountVariance,
                                DateVarianceDays = matchResult.DateVarianceDays,
                                Notes = matchResult.Notes
                            });
                        }

                        if (matchResult.Score >= 1.0m)
                            result.MatchedCount++;
                        else
                            result.PartialMatchCount++;
                    }
                }
                else
                {
                    result.InternalUnmatchedCount++;
                }
            }

            result.ExternalUnmatchedCount = externalPayments.Count(e => !matchedExternals.Contains(e.Id));

            var totalTransactions = internalPayments.Count + externalPayments.Count;
            if (totalTransactions > 0)
            {
                result.MatchPercentage = (decimal)(result.MatchedCount * 2 + result.PartialMatchCount)
                                          / totalTransactions * 100;
            }

            return result;
        }

        private static SimulationLiftDto ComputeLift(
            SimulationRuleSetResultDto baseline,
            SimulationRuleSetResultDto candidate)
        {
            var baselinePairs = baseline.Matches
                .ToDictionary(m => (m.InternalPaymentId, m.ExternalPaymentId));
            var candidatePairs = candidate.Matches
                .ToDictionary(m => (m.InternalPaymentId, m.ExternalPaymentId));

            var newMatches = candidate.Matches
                .Where(m => !baselinePairs.ContainsKey((m.InternalPaymentId, m.ExternalPaymentId)))
                .ToList();

            var lostMatches = baseline.Matches
                .Where(m => !candidatePairs.ContainsKey((m.InternalPaymentId, m.ExternalPaymentId)))
                .ToList();

            var changedMatches = candidate.Matches
                .Where(m => baselinePairs.ContainsKey((m.InternalPaymentId, m.ExternalPaymentId)))
                .Where(m =>
                {
                    var bp = baselinePairs[(m.InternalPaymentId, m.ExternalPaymentId)];
                    return bp.RuleApplied != m.RuleApplied || bp.MatchScore != m.MatchScore;
                })
                .Select(m =>
                {
                    var bp = baselinePairs[(m.InternalPaymentId, m.ExternalPaymentId)];
                    return new SimulationMatchDiffDto
                    {
                        InternalPaymentId = m.InternalPaymentId,
                        ExternalPaymentId = m.ExternalPaymentId,
                        BaselineRule = bp.RuleApplied,
                        BaselineScore = bp.MatchScore,
                        CandidateRule = m.RuleApplied,
                        CandidateScore = m.MatchScore,
                        ScoreDelta = m.MatchScore - bp.MatchScore
                    };
                })
                .ToList();

            return new SimulationLiftDto
            {
                MatchedDelta = candidate.MatchedCount - baseline.MatchedCount,
                PartialMatchDelta = candidate.PartialMatchCount - baseline.PartialMatchCount,
                UnmatchedDelta = (candidate.InternalUnmatchedCount + candidate.ExternalUnmatchedCount)
                               - (baseline.InternalUnmatchedCount + baseline.ExternalUnmatchedCount),
                MatchPercentageDelta = candidate.MatchPercentage - baseline.MatchPercentage,
                NewMatches = newMatches,
                LostMatches = lostMatches,
                ChangedMatches = changedMatches
            };
        }

        private static FalsePositiveAnalysisDto ComputeFalsePositives(
            SimulationRuleSetResultDto candidateResult,
            HashSet<(int, int)> verifiedSet,
            decimal threshold)
        {
            var unverified = candidateResult.Matches
                .Where(m => !verifiedSet.Contains((m.InternalPaymentId, m.ExternalPaymentId)))
                .ToList();

            var lowConfidence = candidateResult.Matches
                .Where(m => m.MatchScore < threshold)
                .ToList();

            return new FalsePositiveAnalysisDto
            {
                UnverifiedMatches = unverified.Take(200).ToList(),
                LowConfidenceMatches = lowConfidence.Take(200).ToList(),
                UnverifiedCount = unverified.Count,
                LowConfidenceCount = lowConfidence.Count,
                FalsePositiveThreshold = threshold
            };
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
                    Severity = transaction.Amount > 100 ? CaseSeverity.High : CaseSeverity.Medium,
                    InternalTransactionId = transaction.Id,

                    // todo:
                    VarianceType = VarianceType.Amount,
                    VarianceAmount = transaction.Amount,
                    ReconciliationRunId = runId,
                    AssignedToId = assigneeId
                };

                dbContext.ExceptionCases.Add(exceptionCase);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Created {Count} exception cases for unmatched transactions, assigned to user {AssigneeId}",
                Math.Min(unmatchedTransactions.Count, 100), assigneeId ?? "unassigned");
        }
    }
}

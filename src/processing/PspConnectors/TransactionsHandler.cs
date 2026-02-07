// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Domain;

using Infrastructure.Persistence;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PspConnectors.Configs;

namespace PspConnectors
{
    public class TransactionsHandler(ILogger<TransactionsHandler> logger,
            IOptions<RunConfigs> runConfigs,
            IComparisonThesaurus comparisonThesaurus,
            ApplicationDbContext applicationDbContext)
    {
        private readonly ILogger<TransactionsHandler> _logger;


        public async Task ProcessRun(DateTime startDate, DateTime endDate, string externalSystem, int pspId)
        {
            _logger.LogInformation($"Reconciling PSP: {pspId} data between {startDate} and {endDate}.");
        }
    }
}

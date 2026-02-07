// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Domain;

using Microsoft.Extensions.Logging;

using PspConnectors.Methods.Noda.NodaCsv;
using PspConnectors.Methods.Noda.NodaPay;

namespace PspConnectors.Methods.NodaPay
{
    public class NodaPayServiceFactory(ILogger<BaseMethodService> logger, string pspName, NodaPayCsvService nodaPayCsvService, NodaPayApiService nodaApiPayService) : BaseMethodService(logger, pspName)
    {
        public BaseMethodService GetService(SystemType systemType) => systemType == SystemType.CSV ? nodaPayCsvService : nodaApiPayService;


        // only added so that the Thesaurus can see it as a valid BaseMethodService
        public override Task<TargetData[]> GetTransactionsAsync(DateTime from, DateTime to)
        {
            throw new NotImplementedException();
        }
    }
}

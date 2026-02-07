// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

using Application.Features.Files.Queries;
using Application.Features.Psps.Commands;
using Application.Features.Psps.Queries;

using Imonas.Application.IntegrationTests;

using NUnit.Framework;

using PspConnectors.Methods.Btcbit;
using PspConnectors.Sources.Omega;

namespace Application.IntegrationTests.Psps
{
    using static Testing;

    public class BtcBitToOmegaHappyFlowTests() : TestBase
    {
        [Test]
        public async Task Go()
        {
            var dummyCsvContent = ImportSilverCsvCommandTests.GetDummyCsvContent();

            // Seed PSPs
            //
            var seedCommand = new SeedPspsCommand();
            var seedResult = await SendAsync(seedCommand);

            seedResult.ShouldBeTrue();

            var fileName = "dummy.csv";

            var allPsps = await SendAsync(new GetAllPspsQuery());
            var selectedPsp = allPsps.FirstOrDefault(x => x.Name == BtcbitService.PspName);
            var selectedPspId = selectedPsp.Id;


            // Bronze Command
            //
            var bronzeCommand = new ImportBronzeCsvCommand
            {
                Data = dummyCsvContent,
                FileName = fileName,
                PspId = selectedPspId
            };
            var bronzeResult = await SendAsync(bronzeCommand);

            bronzeResult.ShouldBeTrue();

            // Silver Command
            //
            var silverCommand = new ImportSilverCsvCommand
            {
                FileName = fileName,
                Data = dummyCsvContent,
                PspId = selectedPspId,
                RawPaymentId = bronzeResult.Value
            };
            var silverResult = await SendAsync(silverCommand);

            silverResult.ShouldBeTrue();

            // Get RawFiles
            //
            var getFilesQueryResult = await SendAsync(new GetFilesQuery { PageSize = 50 });
            getFilesQueryResult.Success.ShouldBeTrue(getFilesQueryResult);

            // Get RawFiles Stats
            //
            var getFileStatsQueryResult = await SendAsync(new GetFileStatsQuery());
            getFileStatsQueryResult.Success.ShouldBeTrue(getFileStatsQueryResult);

            // Reconcile data for given dates
            //
            var reconcileDatesForPspCommand = new ReconcileDatesForPspCommand
            {
                StartDate = new DateTime(2025, 11, 1),
                EndDate = new DateTime(2026, 1, 31),
                ExternalSystem = OmegaSourceService.Name,
                ReconciliationRunId = 1,
                PspId = selectedPspId
            };
            var reconcileDatesForPspCommandResult = await SendAsync(reconcileDatesForPspCommand);
            reconcileDatesForPspCommandResult.Success.ShouldBeTrue(reconcileDatesForPspCommandResult);
        }
    }
}

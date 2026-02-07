// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Domain;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PspConnectors.Configs;
using PspConnectors.Methods;
using PspConnectors.Methods.Btcbit;
using PspConnectors.Methods.CubixPay;
using PspConnectors.Methods.Dns;
using PspConnectors.Methods.GumBallPay;
using PspConnectors.Methods.Noda.NodaCsv;
using PspConnectors.Methods.Noda.NodaPay;
using PspConnectors.Methods.Nummuspay;
using PspConnectors.Methods.Paysage;
using PspConnectors.Methods.Rastpay;
using PspConnectors.Methods.Skrill;
using PspConnectors.Services;
using PspConnectors.Sources.Omega;
using PspConnectors.Sources.SquarePay;

namespace PspConnectors
{
    public class ServiceSetup
    {
        public static IServiceCollection Setup(IConfiguration configuration, IServiceCollection services)
        {
            return services
             .Configure<PaysageConfig>(configuration.GetSection($"PspConfigs:{nameof(PaysageConfig)}"))
             .Configure<RastPayConfig>(configuration.GetSection($"PspConfigs:{nameof(RastPayConfig)}"))
             .Configure<NummuspayConfig>(configuration.GetSection($"PspConfigs:{nameof(NummuspayConfig)}"))
             .Configure<NodaPayConfig>(configuration.GetSection($"PspConfigs:{nameof(NodaPayConfig)}"))

             .Configure<RunConfigs>(configuration.GetSection(nameof(RunConfigs)))

             .AddTransient<HttpClient, HttpClient>()

             .AddScoped<OmegaSourceService>()
             .AddScoped<SquarePaySourceService>()

             .AddScoped<TargetService>()

             .AddScoped<RastPayClient>()

             .AddScoped<PaysageService>()
             .AddScoped<DnsService>()
             .AddScoped<NodaPayCsvService>()
             .AddScoped<CubixPayService>()
             .AddScoped<GumballPayService>()
             .AddScoped<RastPayService>()
             .AddScoped<BtcbitService>()
             .AddScoped<SkrillService>()
             .AddScoped<NummusPayService>()
             .AddScoped<NodaPayApiService>()

             .AddScoped<ITargetThesaurus, ComparisonThesaurus>()
             .AddScoped<IComparisonThesaurus, ComparisonThesaurus>()

             .AddScoped<TransactionsHandler>();
        }
    }
}

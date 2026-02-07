// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Application;
using Application.Common.Interfaces;

using Infrastructure;
using Infrastructure.Services;

using PspConnectors;

using SG.Common;
using SG.Common.Settings;

using SmartAdmin.WebUI.HttpHandlers;

namespace SmartAdmin.WebUI
{
    public class Startup
    {
        public static void InitializeGlobalSettings()
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        }

        public static void RegisterServices(IConfiguration configuration, IServiceCollection services)
        {
            services.AddSingleton(new MoneyFormatter(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["EUR"] = "de-DE", 
                ["USD"] = "en-US",
                ["GBP"] = "en-GB",
            }));

            services.AddRazorPageServices(configuration);
            services.AddInfrastructureServices(configuration)
                            .AddApplicationServices()
                            .AddWorkflow(configuration);


            ServiceSetup.Setup(configuration, services);

            services.AddScoped<ICurrencyService, CurrencyService>();
            services.Configure<PspDataGatheringSettings>(configuration.GetSection(nameof(PspDataGatheringSettings)));
            services.AddScoped<FileUploadHandlers>();
        }
    }
}

using Application.Common.Interfaces;
using Infrastructure.Persistence;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Serilog;
using Serilog.Events;

using SmartAdmin.WebUI;

using Microsoft.AspNetCore.HttpOverrides;
using System.Net;

Startup.InitializeGlobalSettings();

var builder = WebApplication.CreateBuilder(args);

// Forwarded headers support (Nginx reverse proxy)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    options.ForwardLimit = null;

    // Accept forwarded headers only from local reverse proxy (Nginx on same box)
    options.KnownProxies.Add(IPAddress.Loopback);
    options.KnownProxies.Add(IPAddress.IPv6Loopback);
});

builder.Services.AddSwaggerGen();

builder.Host.UseSerilog((context, configuration) =>
{
    var minLevel = builder.Configuration["Serilog:MinimumLevel:Default"] switch
    {
        "Debug" => LogEventLevel.Debug,
        "Information" => LogEventLevel.Information,
        _ => LogEventLevel.Error
    };

    configuration.ReadFrom.Configuration(context.Configuration)
        .MinimumLevel.Override("Microsoft", minLevel)
        .MinimumLevel.Override("Microsoft.AspNetCore", minLevel)
        .MinimumLevel.Override("Serilog", minLevel)
        .Enrich.FromLogContext()
        .Enrich.WithClientIp()
        .WriteTo.Console();
});

Startup.RegisterServices(builder.Configuration, builder.Services);

var app = builder.Build();
app.Logger.LogWarning("ContentRoot: {ContentRoot} | WebRoot: {WebRoot}", app.Environment.ContentRootPath, app.Environment.WebRootPath);

// Must be early in the pipeline so downstream middleware sees correct scheme/IP.
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseDeveloperExceptionPage();    
}
else
{
    app.UseHsts();
}

app.Logger.LogInformation("Adding infrastructure services.");
app.UseInfrastructure(builder.Configuration);

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        if (context.Database.IsNpgsql())
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Trying to run migrations with the following ConnectionString: " + context.Database.GetConnectionString());

            context.Database.Migrate();
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var dateTimeService = services.GetRequiredService<IDateTime>();

        await ApplicationDbContextSeed.SeedDefaultUserAsync(userManager, roleManager, dateTimeService);
        await ApplicationDbContextSeed.SeedSampleDataAsync(context, dateTimeService);

        var currencyService = services.GetRequiredService<ICurrencyService>();
        await currencyService.Initialize();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        throw;
    }
}

await app.RunAsync();

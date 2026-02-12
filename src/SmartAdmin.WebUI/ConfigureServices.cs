using Microsoft.Extensions.Options;

using SmartAdmin.WebUI.Filters;
using SmartAdmin.WebUI.Models;

namespace SmartAdmin.WebUI;
public static class ConfigureServices
{
    public static IServiceCollection AddRazorPageServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SmartSettings>(configuration.GetSection(SmartSettings.SectionName));
        services.AddScoped(s => s.GetRequiredService<IOptions<SmartSettings>>().Value);
        services.AddHealthChecks();
        services.AddDatabaseDeveloperPageExceptionFilter();
        services.AddControllers();
        services.AddRazorPages(options =>
        {
            options.Conventions.AddPageRoute("/Dashboard/Index", "");
        })
     .AddMvcOptions(options =>
     {
         options.Filters.Add<ApiExceptionFilterAttribute>();
     })
    .AddViewLocalization()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;

    })
    .AddRazorRuntimeCompilation();
        services.AddSignalR();
        return services;
    }
}

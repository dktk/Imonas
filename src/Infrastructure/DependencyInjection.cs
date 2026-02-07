using System.Reflection;

using Application;
using Application.Common.Interfaces;

using Infrastructure.Constants.Localization;
using Infrastructure.Constants.Permission;
using Infrastructure.Localization;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Infrastructure.Services.Identity;
using Infrastructure.Services.Settlement;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration.GetValue<bool>("UseInMemoryDatabase"))
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase("Imonas.RazorDb")
                    );
            }
            else
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseNpgsql(
                        configuration.GetConnectionString("DefaultConnection"),
                        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
                    .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning))
                    );
            }
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IApplicationDbContext>(provider => provider.GetService<ApplicationDbContext>());
            services.AddScoped<IDomainEventService, DomainEventService>();
            services.AddScoped<ISerilogsService, SerilogsService>();

            services.AddIdentity<ApplicationUser, ApplicationRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddTransient<IDateTime, DateTimeService>();
            services.AddSingleton<IUptimeService, UptimeService>();
            services.AddTransient<IExcelService, ExcelService>();
            services.AddTransient<IUploadService, UploadService>();
            services.AddTransient<IIdentityService, IdentityService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ISettlementService, SettlementService>();
            services.Configure<AppConfigurationSettings>(configuration.GetSection("AppConfigurationSettings"));
            services.Configure<MailSettings>(configuration.GetSection("MailSettings"));
            services.AddTransient<IMailService, SMTPMailService>();
            services.AddTransient<IDictionaryService, DictionaryService>();
            services.AddAuthentication();
            services.Configure<IdentityOptions>(options =>
            {
                // Default SignIn settings.
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                // Default Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            });
            services.ConfigureApplicationCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromHours(1);
                options.SlidingExpiration = true;
                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = "/Identity/Account/Logout";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            });
            services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = TimeSpan.FromHours(1));
            services.AddAuthorization(options =>
            {
                options.AddPolicy("CanPurge", policy => policy.RequireRole("Administrator"));
                // Here I stored necessary permissions/roles in a constant
                foreach (var prop in typeof(Permissions).GetNestedTypes().SelectMany(c => c.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)))
                {
                    var propertyValue = prop.GetValue(null);
                    if (propertyValue is not null)
                    {
                        options.AddPolicy(propertyValue.ToString(), policy => policy.RequireClaim(ApplicationClaimTypes.Permission, propertyValue.ToString()));
                    }
                }
            });
            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationClaimsIdentityFactory>();
            // Localization
            services.AddLocalization(options =>
            {
                options.ResourcesPath = LocalizationConstants.ResourcesPath;
            });

            services.AddScoped<RequestLocalizationCookiesMiddleware>();
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.AddSupportedUICultures(LocalizationConstants.SupportedLanguages.Select(x => x.Code).ToArray());
                options.FallBackToParentUICultures = true;
            });

            return services;
        }

        public static IServiceCollection AddWorkflow(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration.GetValue<bool>("UseInMemoryDatabase"))
            {
                services.AddWorkflow();
            }
            else
            {
                services.AddWorkflow(x => x.UsePostgreSQL(configuration.GetConnectionString("DefaultConnection"), true, true));
            }
            return services;
        }
        public static IApplicationBuilder UseWorkflow(this IApplicationBuilder app)
        {
            //var host = app.ApplicationServices.GetService<IWorkflowHost>();
            //host.RegisterWorkflow<DocmentApprovalWorkflow, ApprovalData>();
            //host.Start();
            return app;
        }
    }
}

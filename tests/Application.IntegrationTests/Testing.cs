using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Application;
using Application.Common.Interfaces;

using Dapper;

using Domain.Common;

using Infrastructure;
using Infrastructure.Constants.Localization;
using Infrastructure.Extensions;
using Infrastructure.Identity;
using Infrastructure.Persistence;

using MediatR;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using Npgsql;

using NUnit.Framework;

using PspConnectors;

using SmartAdmin.WebUI;

public static class Migrator
{
    private static bool hasMigrated;

    public static async Task SafeMigrate(ApplicationDbContext context)
    {
        if (!hasMigrated)
        {
            await context.Database.MigrateAsync();
        }

        hasMigrated = true;
    }
}

[SetUpFixture]
public class Testing
{
    private static IConfigurationRoot _configuration;
    private static IServiceScopeFactory _scopeFactory;
    private static ServiceProvider _serviceProvider;
    private static string _currentUserId;
    private static string testMainDb;
    private static string mainConnectionString;

    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        Startup.InitializeGlobalSettings();

        var (configValues, dbName, connectionString) = GetDynamicConfigValues();
        testMainDb = dbName;
        mainConnectionString = connectionString;

        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true)
            .AddInMemoryCollection(configValues)
            .AddEnvironmentVariables();

        _configuration = builder.Build();

        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(_configuration);

        Startup.RegisterServices(_configuration, services);

        services.AddLocalization(options =>
        {
            options.ResourcesPath = "../../../SmartAdmin.WebUI/" + LocalizationConstants.ResourcesPath;
        });

        ServiceSetup.Setup(_configuration, services);

        services.AddInfrastructureServices(_configuration)
                .AddApplicationServices();

        // Add PspConnectors services
        ServiceSetup.Setup(_configuration, services);

        // Remove ONLY the DbContext registrations added by ServiceSetup (not IApplicationDbContext)
        // We want to keep the InMemory DbContext from Infrastructure
        var npgsqlDbContextDescriptor = services.LastOrDefault(d =>
            d.ServiceType == typeof(IApplicationDbContext) &&
            d.ImplementationType == typeof(ApplicationDbContext) &&
            d.Lifetime == ServiceLifetime.Scoped &&
            d.ImplementationFactory != null);

        if (npgsqlDbContextDescriptor != null)
        {
            services.Remove(npgsqlDbContextDescriptor);
        }

        // Replace service registration for ICurrentUserService
        // Remove existing registration
        var currentUserServiceDescriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(ICurrentUserService));

        services.Remove(currentUserServiceDescriptor);

        // Register testing version
        services.AddTransient(provider =>
            Mock.Of<ICurrentUserService>(s => s.UserId == _currentUserId));

        _serviceProvider = services.BuildServiceProvider();
        _scopeFactory = _serviceProvider.GetService<IServiceScopeFactory>();

        //using (var scope = _serviceProvider.CreateScope())
        //{
        //    try
        //    {
        //        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        //        dbContext.Database.Migrate();
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}
    }

    private static (Dictionary<string, string>, string, string) GetDynamicConfigValues()
    {
        var connectionStringKey = "DefaultConnection";
        var initialBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true)
            .AddEnvironmentVariables()
            .Build();
        var connectionString = initialBuilder.ConnectionString(connectionStringKey);

        using var connection = new NpgsqlConnection(connectionString);
        var initialDbName = connection.Database;

        connection.Open();

        var testDbName = $"integration_test_db_{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}";
        var createDbSql = $"CREATE DATABASE {testDbName};";
        var callCmd = new CommandDefinition(createDbSql);
        connection.Execute(callCmd);

        connection.ChangeDatabase(testDbName);

        var configValues = new Dictionary<string, string>
        {
            { $"ConnectionStrings:{connectionStringKey}", connectionString.Replace(initialDbName, testDbName) }
        };

        return (configValues, testDbName, connectionString);
    }

    public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using var scope = _scopeFactory.CreateScope();

        var mediator = scope.ServiceProvider.GetService<ISender>();

        return await mediator.Send(request);
    }

    public static async Task<string> RunAsDefaultUserAsync()
    {
        return await RunAsUserAsync("Demo", "Password123!", new string[] { });
    }

    public static async Task<string> RunAsAdministratorAsync()
    {
        return await RunAsUserAsync("administrator", "Password123!", new[] { "Admin" });
    }

    public static async Task<string> RunAsUserAsync(string userName, string password, string[] roles)
    {
        using var scope = _scopeFactory.CreateScope();

        var userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser { UserName = userName, Email = userName };

        var result = await userManager.CreateAsync(user, password);

        if (roles.Any())
        {
            var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>();

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }

            await userManager.AddToRolesAsync(user, roles);
        }

        if (result.Succeeded)
        {
            _currentUserId = user.Id;

            return _currentUserId;
        }

        var errors = string.Join(',', Environment.NewLine, result.ToApplicationResult().Errors);

        throw new Exception($"Unable to create {userName}.{Environment.NewLine}{errors}");
    }

    public static async Task ResetState()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetService<ApplicationDbContext>();

        // Clear all data from in-memory database
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        _currentUserId = null;
        await Task.CompletedTask;
    }

    public static async Task<TEntity> FindAsync<TEntity>(int entityId)
        where TEntity : class, IEntity
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetService<ApplicationDbContext>();

        return await context.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == entityId);
    }

    public static async Task<int> AddAsync<TEntity>(TEntity entity)
        where TEntity : class, IEntity
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetService<ApplicationDbContext>();

        context.Add(entity);

        await context.SaveChangesAsync();

        return entity.Id;

    }

    public static async Task<int> CountAsync<TEntity>() where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetService<ApplicationDbContext>();

        return await context.Set<TEntity>().CountAsync();
    }
    public static Task<T> GetRequiredService<T>()
    {
        using var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<T>();
        return Task.FromResult(service);
    }

    [OneTimeTearDown]
    public async Task RunAfterAnyTests()
    {
        _serviceProvider?.Dispose();

        // drop DB
        using var connection = new NpgsqlConnection(mainConnectionString);
        var initialDbName = connection.Database;
         
        await connection.OpenAsync();

        var createDbSql = $"DROP DATABASE {testMainDb} WITH (FORCE);";
        var callCmd = new CommandDefinition(createDbSql);
        await connection.ExecuteAsync(callCmd);

        await connection.CloseAsync();
    }
}

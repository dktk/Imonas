using Application.Common.Interfaces;

using Infrastructure.Services;

using PspConnectors;

using PspQueue.Consumer.Worker;

DotNetEnv.Env.Load();

Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json",
                         optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<RabbitMqOptions>(context.Configuration.GetSection("RabbitMq"));
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDomainEventService, EmptyDomainEventService>();
        services.AddScoped<IDateTime, DateTimeService>();

        services.AddLogging(cfg =>
                             {
                                 cfg.ClearProviders();
                                 cfg.AddConsole();
                             });

        ServiceSetup
            .Setup(context.Configuration, services)

            .AddScoped<IRabbitMqConnectionProvider, RabbitMqConnectionProvider>()
            .AddHostedService<QueueConsumerWorker>();
    })
    .Build()
    .Run();

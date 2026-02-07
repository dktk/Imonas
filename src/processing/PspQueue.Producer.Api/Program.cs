using Imonas.Exchange.Contracts;

using PspQueue.Producer.Api;

DotNetEnv.Env.Load();

var builder = WebApplication
                .CreateBuilder(args);

//builder.Configuration.AddConfiguration()
//        .ConfigureAppConfiguration((context, config) =>
//        {
//            config
//                .SetBasePath(AppContext.BaseDirectory)
//                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//                .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json",
//                             optional: true, reloadOnChange: true)
//                .AddEnvironmentVariables();
//        });


// Configuration is already wired for appsettings + environment variables.
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddScoped<IQueuePspDataProducer, QueuePspDataProducer>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapPost("/psps", (QueuePspData payload, IQueuePspDataProducer producer) =>
{
    //if (payload.correlationId == Guid.Empty)
    //{
    //    payload = payload with { correlationId = Guid.NewGuid() };
    //}

    producer.Publish(payload);

    return Results.Accepted($"/psps/{payload.correlationId}", new
    {
        correlationId = payload.correlationId,
        status = "queued"
    });
})
.WithName("EnqueuePspData")
.Produces(202);

app.MapGet("/", () => "PspQueue Producer API");

app.Run();

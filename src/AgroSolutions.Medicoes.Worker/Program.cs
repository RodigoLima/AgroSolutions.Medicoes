using AgroSolutions.Medicoes.Application;
using AgroSolutions.Medicoes.Application.Services;
using AgroSolutions.Medicoes.Infrastructure;
using AgroSolutions.Medicoes.Infrastructure.Observability;
using AgroSolutions.Medicoes.Worker;
using AgroSolutions.Medicoes.Worker.Consumers;
using MassTransit;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;


var builder = Host.CreateApplicationBuilder(args);

var environment = builder.Configuration["Environment"] ?? "development";

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)

    .Enrich.FromLogContext()
    .Enrich.WithThreadId()
    .Enrich.WithProcessId()
    .Enrich.WithProcessName()
    .Enrich.With<ActivityEnricher>()

    .WriteTo.Console(
        outputTemplate:
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} " +
            "[{Level:u3}] " +
            "{Message:lj} " +
            "{Properties:j}" +
            "{NewLine}{Exception}"
    )

    .WriteTo.OpenTelemetry(options =>
    {
        options.Endpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"];
        options.Protocol = Serilog.Sinks.OpenTelemetry.OtlpProtocol.Grpc;
    })
    .CreateLogger();


builder.Logging.AddSerilog(Log.Logger);
builder.Services.AddObservability(builder.Configuration);

builder.Services.AddHostedService<Worker>();
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("Email"));
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SensorDataConsumer>();
    x.AddConsumer<PropriedadeDataConsumer>();
    x.AddConsumer<TalhaoDataConsumer>();
    x.AddConsumer<ProdutorDataConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("admin");
            h.Password("admin123");
        });

        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();

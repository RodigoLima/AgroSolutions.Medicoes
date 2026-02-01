using AgroSolutions.Medicoes.Application;
using AgroSolutions.Medicoes.Application.Services;
using AgroSolutions.Medicoes.Infrastructure;
using AgroSolutions.Medicoes.Infrastructure.Database;
using AgroSolutions.Medicoes.Infrastructure.Messaging;
using AgroSolutions.Medicoes.Infrastructure.Observability;
using AgroSolutions.Medicoes.Worker;
using AgroSolutions.Medicoes.Worker.Consumers;
using DotNetEnv;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Sinks.OpenTelemetry;

var envPath = Path.GetFullPath(
    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".env")
);

Console.WriteLine($"Tentando carregar .env em: {envPath}");

Env.Load(envPath);

var builder = Host.CreateApplicationBuilder(args);

var environment = builder.Environment.EnvironmentName;

var otlpEndpoint =
    Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")
    ?? throw new InvalidOperationException(
        "OTEL_EXPORTER_OTLP_ENDPOINT não configurado");

// Configure Serilog
builder.Logging.ClearProviders();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithSpan()
    .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {TraceId} {SpanId} {Message:lj}{NewLine}{Exception}")
    .WriteTo.OpenTelemetry(options =>
    {
        options.Endpoint = otlpEndpoint;
        options.Protocol = OtlpProtocol.Grpc;

        options.ResourceAttributes = new Dictionary<string, object>
        {
            ["service.name"] = ActivitySourceProvider.ServiceName,
            ["service.version"] = ActivitySourceProvider.ServiceVersion,
            ["deployment.environment"] = builder.Environment.EnvironmentName
        };
    })
    .CreateLogger();

builder.Logging.AddSerilog(Log.Logger);
builder.Services.AddObservability(builder.Configuration);

builder.Services.AddHostedService<Worker>();
// builder.Services.AddHostedService<MessageTestBootstrap>();
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("Email")
);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection("RabbitMq")
);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SensorDataConsumer>();
    x.AddConsumer<PropriedadeDataConsumer>();
    x.AddConsumer<TalhaoDataConsumer>();
    x.AddConsumer<ProdutorDataConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var settings = context.GetRequiredService<IOptions<RabbitMqSettings>>().Value;
        
        cfg.Host(settings.Host, settings.VirtualHost, h =>
        {
            h.Username(settings.Username);
            h.Password(settings.Password);
        });

        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
host.Run();

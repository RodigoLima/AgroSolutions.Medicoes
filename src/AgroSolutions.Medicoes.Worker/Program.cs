using AgroSolutions.Medicoes.Application;
using AgroSolutions.Medicoes.Application.Services;
using AgroSolutions.Medicoes.Infrastructure;
using AgroSolutions.Medicoes.Infrastructure.Database;
using AgroSolutions.Medicoes.Infrastructure.Messaging;
using AgroSolutions.Medicoes.Infrastructure.Observability;
using AgroSolutions.Medicoes.Worker;
using AgroSolutions.Medicoes.Worker.Consumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;


var builder = Host.CreateApplicationBuilder(args);

var environment = builder.Environment.EnvironmentName;

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
    builder.Configuration.GetSection("Email")
);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection("RabbitMq")
);

Console.WriteLine("RabbitMq:Port = " + builder.Configuration["RabbitMq:Port"]);

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

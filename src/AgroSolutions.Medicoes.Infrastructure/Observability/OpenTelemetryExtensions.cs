using MassTransit.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace AgroSolutions.Medicoes.Infrastructure.Observability;

public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddObservability(
        this IServiceCollection services, 
        IConfiguration configuration
    )
    {
        var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"];

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: ActivitySourceProvider.ServiceName,
                    serviceVersion: ActivitySourceProvider.ServiceVersion)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = configuration["Environment"] ?? "development",
                    ["host.name"] = Environment.MachineName
                })
                .AddEnvironmentVariableDetector())
            .WithTracing(tracing => tracing
                .AddSource(ActivitySourceProvider.ServiceName)
                .AddSource(DiagnosticHeaders.DefaultListenerName)
                .AddNpgsql()
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint!);
                }))
            .WithMetrics(metrics => metrics
                .AddMeter(ActivitySourceProvider.ServiceName)
                .AddMeter("MassTransit")
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint!);
                }));

        return services;

    }
}

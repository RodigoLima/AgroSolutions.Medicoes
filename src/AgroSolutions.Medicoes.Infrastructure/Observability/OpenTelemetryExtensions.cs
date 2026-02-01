using MassTransit.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
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
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: ActivitySourceProvider.ServiceName,
                    serviceVersion: ActivitySourceProvider.ServiceVersion)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = configuration["Environment"] ?? "development",
                    ["host.name"] = Environment.MachineName
                }))
            .WithTracing(tracing => tracing
                .AddSource(ActivitySourceProvider.ServiceName)
                .AddSource(DiagnosticHeaders.DefaultListenerName)
                .AddNpgsql()
                .AddOtlpExporter())
            .WithMetrics(metrics => metrics
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                .AddMeter(ActivitySourceProvider.ServiceName)
                .AddMeter("OpenTelemetry.Instrumentation.Process")
                .AddOtlpExporter());

        return services;
    }
}
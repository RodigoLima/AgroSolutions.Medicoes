using Prometheus;

namespace AgroSolutions.Medicoes.Worker;

public class PrometheusMetricsHostedService : IHostedService
{
    private readonly IHostApplicationLifetime _lifetime;
    private KestrelMetricServer? _server;

    public PrometheusMetricsHostedService(IHostApplicationLifetime lifetime)
    {
        _lifetime = lifetime;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _server = new KestrelMetricServer(8080);
        _ = _server.StartAsync(_lifetime.ApplicationStopping);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

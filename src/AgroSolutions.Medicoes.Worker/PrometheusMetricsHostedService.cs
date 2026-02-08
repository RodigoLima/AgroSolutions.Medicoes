using Prometheus;

namespace AgroSolutions.Medicoes.Worker;

public class PrometheusMetricsHostedService : IHostedService, IDisposable
{
    private MetricServer? _server;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _server = new MetricServer(8080);
        _server.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _server?.Stop();
        return Task.CompletedTask;
    }

    public void Dispose() => _server?.Dispose();
}

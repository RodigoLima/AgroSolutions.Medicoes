using System.Diagnostics;
using Prometheus;

namespace AgroSolutions.Medicoes.Worker;

public class PrometheusMetricsHostedService : IHostedService, IDisposable
{
    private MetricServer? _server;
    private Timer? _processMetricsTimer;
    private static readonly Gauge ProcessResidentMemoryBytes = Metrics.CreateGauge(
        "process_resident_memory_bytes", "Memória residente em bytes.");
    private static readonly Gauge ProcessNumThreads = Metrics.CreateGauge(
        "process_num_threads", "Número de threads.");
    private static readonly Gauge ProcessCpuSecondsTotal = Metrics.CreateGauge(
        "process_cpu_seconds_total", "CPU total em segundos.");
    private static readonly Gauge ProcessVirtualMemoryBytes = Metrics.CreateGauge(
        "process_virtual_memory_bytes", "Memória virtual em bytes.");

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _server = new MetricServer(8080);
        _server.Start();
        _processMetricsTimer = new Timer(UpdateProcessMetrics, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        return Task.CompletedTask;
    }

    private static void UpdateProcessMetrics(object? _)
    {
        try
        {
            using var process = Process.GetCurrentProcess();
            ProcessResidentMemoryBytes.Set(process.WorkingSet64);
            ProcessVirtualMemoryBytes.Set(process.VirtualMemorySize64);
            ProcessNumThreads.Set(process.Threads.Count);
            ProcessCpuSecondsTotal.Set(process.TotalProcessorTime.TotalSeconds);
        }
        catch { }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _processMetricsTimer?.Change(Timeout.Infinite, 0);
        _server?.Stop();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _processMetricsTimer?.Dispose();
        _server?.Dispose();
    }
}

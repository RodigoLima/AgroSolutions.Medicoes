using System.Diagnostics;
using AgroSolutions.Medicoes.Application.Rules.Schedulers;

namespace AgroSolutions.Medicoes.Worker;

public class Worker : BackgroundService
{
    private static readonly ActivitySource ActivitySource =
        new("AgroSolutions.Medicoes.Worker");
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private static readonly TimeSpan INTERVALO = TimeSpan.FromMinutes(1);

    public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Iniciou o worker. Intervalo de execução: {IntervaloMinutos} minutos",
            INTERVALO.TotalMinutes
        );
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var activity = ActivitySource.StartActivity(
                    "processar-mensagens",
                    ActivityKind.Internal
                );

                activity?.SetTag("worker.loop", true);
                
                _logger.LogInformation("Iniciou a execução da regra do worker.");
                using var scope = _scopeFactory.CreateScope();
                var scheduler = scope.ServiceProvider.GetRequiredService<IRuleScheduler>();

                await scheduler.ExecuteAsync(stoppingToken);
                _logger.LogInformation(
                    "Finalizou a execução da regra do worker. Proxima execução: {ProximaExecucao}",
                    DateTime.Now.AddSeconds(INTERVALO.TotalSeconds)
                );
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(
                    ex,
                    "A operação foi cancelada."
                );

                throw;
            }
            catch(Exception ex)
            {
                _logger.LogError(
                    ex, 
                    "Erro inesperado ao executar o worker."
                );
            }

            await Task.Delay(INTERVALO, stoppingToken);
        }
    }
}

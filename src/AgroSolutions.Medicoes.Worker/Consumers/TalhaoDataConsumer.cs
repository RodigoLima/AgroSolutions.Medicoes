using AgroSolutions.Contracts;
using AgroSolutions.Medicoes.Application.Interfaces.Services;
using MassTransit;

namespace AgroSolutions.Medicoes.Worker.Consumers;

public class TalhaoDataConsumer(
    ITalhaoService _service, 
    ILogger<TalhaoDataConsumer> _logger
) : IConsumer<TalhaoDataMessage>
{
    public async Task Consume(ConsumeContext<TalhaoDataMessage> context)
    {
        _logger.LogInformation(
            "Mensagem recebida na fila de talhões: {@Message}",
            context.Message
        );

        await _service.ProcessarAsync(context.Message, context.CancellationToken);
    }
}

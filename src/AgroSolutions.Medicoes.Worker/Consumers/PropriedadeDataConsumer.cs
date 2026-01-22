using AgroSolutions.Contracts;
using AgroSolutions.Medicoes.Application.Interfaces.Services;
using MassTransit;

namespace AgroSolutions.Medicoes.Worker.Consumers;

public class PropriedadeDataConsumer(
    IPropriedadeService _service, 
    ILogger<PropriedadeDataConsumer> _logger
) : IConsumer<PropriedadeDataMessage>
{
    public async Task Consume(ConsumeContext<PropriedadeDataMessage> context)
    {
        _logger.LogInformation(
            "Mensagem recebida na fila de Propriedade: {@Message}",
            context.Message
        );

        await _service.ProcessarAsync(context.Message, context.CancellationToken);
    }
}

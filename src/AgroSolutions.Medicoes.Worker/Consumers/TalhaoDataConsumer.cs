using System.Diagnostics;
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

        Activity.Current?.SetTag("messaging.system", "rabbitmq");
        Activity.Current?.SetTag("messaging.destination", context.DestinationAddress?.ToString());
        Activity.Current?.SetTag("messaging.message_id", context.MessageId);
        Activity.Current?.SetTag("messaging.conversation_id", context.ConversationId);

        await _service.ProcessarAsync(context.Message, context.CancellationToken);
    }
}

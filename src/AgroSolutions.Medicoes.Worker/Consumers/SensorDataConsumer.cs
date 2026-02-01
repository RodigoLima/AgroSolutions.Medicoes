using System.Diagnostics;
using AgroSolutions.Contracts;
using AgroSolutions.Medicoes.Application.Interfaces.Services;
using MassTransit;

namespace AgroSolutions.Medicoes.Worker.Consumers;

public class SensorDataConsumer(
    IMedicaoService _service, 
    ILogger<SensorDataConsumer> _logger
): IConsumer<SensorDataMessage>
{
    public async Task Consume(ConsumeContext<SensorDataMessage> context)
    {
        _logger.LogInformation(
            "Mensagem recebida na fila de sensores: {@Message}",
            context.Message
        );

        Activity.Current?.SetTag("messaging.system", "rabbitmq");
        Activity.Current?.SetTag("messaging.destination", context.DestinationAddress?.ToString());
        Activity.Current?.SetTag("messaging.message_id", context.MessageId);
        Activity.Current?.SetTag("messaging.conversation_id", context.ConversationId);

        await _service.ProcessarAsync(context.Message, context.CancellationToken);
    }
}

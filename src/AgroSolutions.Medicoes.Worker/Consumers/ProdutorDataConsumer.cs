using AgroSolutions.Medicoes.Application.Contracts;
using AgroSolutions.Medicoes.Application.Interfaces.Services;
using MassTransit;

namespace AgroSolutions.Medicoes.Worker.Consumers;

public class ProdutorDataConsumer(
    IProdutorService _service, 
    ILogger<ProdutorDataConsumer> _logger
) : IConsumer<ProdutorDataMessage>
{
    public async Task Consume(ConsumeContext<ProdutorDataMessage> context)
    {
        _logger.LogInformation(
            "Mensagem recebida na fila de produtores: {@Message}",
            context.Message
        );

        await _service.ProcessarAsync(context.Message, context.CancellationToken);
    }
}

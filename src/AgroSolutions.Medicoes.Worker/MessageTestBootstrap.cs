
using System.Diagnostics;
using AgroSolutions.Medicoes.Application.Contracts;
using AgroSolutions.Medicoes.Infrastructure.Observability;
using MassTransit;

namespace AgroSolutions.Medicoes.Worker;

public class MessageTestBootstrap(
    IBus publishEndpoint) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var activity = ActivitySourceProvider.Source.StartActivity(
            "RabbitMQ.Publish TesteMensagem",
            ActivityKind.Producer);

        var mensagem = new ProdutorDataMessage(Guid.NewGuid(), "teste@mail.com");

        activity?.SetTag("messaging.system", "rabbitmq");
        activity?.SetTag("messaging.destination", "teste-mensagem");
        activity?.SetTag("messaging.message_id", mensagem.ProdutorId);

        await publishEndpoint.Publish(mensagem);
    }

    public Task StopAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;
}

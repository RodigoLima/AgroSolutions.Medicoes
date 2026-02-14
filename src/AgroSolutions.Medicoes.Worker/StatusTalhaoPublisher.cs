using AgroSolutions.Contracts;
using AgroSolutions.Medicoes.Application.Interfaces.Services;
using MassTransit;

namespace AgroSolutions.Medicoes.Worker;

public class StatusTalhaoPublisher(IPublishEndpoint _publishEndpoint) : IStatusTalhaoPublisher
{
    public async Task PublishAsync(Guid talhaoId, int status, CancellationToken cancellationToken = default)
    {
        await _publishEndpoint.Publish(new TalhaoStatusUpdateMessage(talhaoId, status), cancellationToken);
    }
}

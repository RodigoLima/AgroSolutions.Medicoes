namespace AgroSolutions.Medicoes.Application.Interfaces.Services;

public interface IStatusTalhaoPublisher
{
    Task PublishAsync(Guid talhaoId, int status, CancellationToken cancellationToken = default);
}

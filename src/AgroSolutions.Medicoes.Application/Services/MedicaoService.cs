using AgroSolutions.Contracts;
using AgroSolutions.Medicoes.Application.Interfaces.Services;
using AgroSolutions.Medicoes.Domain.Entities;
using AgroSolutions.Medicoes.Domain.Exceptions;
using AgroSolutions.Medicoes.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace AgroSolutions.Medicoes.Application.Services;

public class MedicaoService(
    IMedicaoRepository _repository, 
    ILogger<MedicaoService> _logger
) : IMedicaoService
{
    public async Task ProcessarAsync(SensorDataMessage sensorData, CancellationToken cancellationToken)
    {
        try
        {
            var medicao = new Medicao(
                sensorData.TalhaoId,
                sensorData.DataMedicao,
                sensorData.Tipo,
                sensorData.Valor
            );

            await _repository.AddAsync(medicao, cancellationToken);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(
                ex,
                "Falha ao criar medição com dados inválidos."
            );

            throw;
        }

    }
}

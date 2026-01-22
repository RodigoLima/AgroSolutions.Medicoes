using AgroSolutions.Contracts;
using AgroSolutions.Medicoes.Application.Interfaces.Services;
using AgroSolutions.Medicoes.Domain.Entities;
using AgroSolutions.Medicoes.Domain.Exceptions;
using AgroSolutions.Medicoes.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace AgroSolutions.Medicoes.Application.Services;

public class TalhaoService(
    ITalhaoRepository _repository, 
    ILogger<TalhaoService> _logger
) : ITalhaoService
{
    public async Task ProcessarAsync(TalhaoDataMessage talhaoData, CancellationToken cancellationToken)
    {
        try
        {
            var talhao = new Talhao(
                talhaoData.TalhaoId,
                talhaoData.Nome,
                talhaoData.PropriedadeId
            );

            await _repository.AddAsync(talhao, cancellationToken);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(
                ex,
                "Falha ao criar talhão com dados inválidos."
            );

            throw;
        }
    }
}

using AgroSolutions.Contracts;
using AgroSolutions.Medicoes.Application.Interfaces.Services;
using AgroSolutions.Medicoes.Domain.Entities;
using AgroSolutions.Medicoes.Domain.Exceptions;
using AgroSolutions.Medicoes.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace AgroSolutions.Medicoes.Application.Services;

public class PropriedadeService(
    IPropriedadeRepository _repository, 
    ILogger<PropriedadeService> _logger
) : IPropriedadeService
{
    public async Task ProcessarAsync(PropriedadeDataMessage propriedadeData, CancellationToken cancellationToken)
    {
        try
        {
            var propriedade = new Propriedade(
                propriedadeData.PropriedadeId,
                propriedadeData.Nome,
                propriedadeData.ProdutorId
            );

            await _repository.AddAsync(propriedade, cancellationToken);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(
                ex,
                "Falha ao criar propriedade com dados inválidos."
            );

            throw;
        }
    }
}

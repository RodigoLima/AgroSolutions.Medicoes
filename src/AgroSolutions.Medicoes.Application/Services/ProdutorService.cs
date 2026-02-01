using AgroSolutions.Medicoes.Application.Contracts;
using AgroSolutions.Medicoes.Application.Interfaces.Services;
using AgroSolutions.Medicoes.Domain.Entities;
using AgroSolutions.Medicoes.Domain.Exceptions;
using AgroSolutions.Medicoes.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace AgroSolutions.Medicoes.Application.Services;

public class ProdutorService(
    IProdutorRepository _repository, 
    ILogger<ProdutorService> _logger
) : IProdutorService
{
    public async Task ProcessarAsync(ProdutorDataMessage produtorData, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processando produtor: {@produtor}",
            produtorData
        );

        try
        {
            var produtor = new Produtor(
                produtorData.ProdutorId,
                produtorData.Email
            );

            _logger.LogInformation(
                "Inserindo produtor '{Id}'",
                produtor.Id
            );
            await _repository.AddAsync(produtor, cancellationToken);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(
                ex,
                "Falha ao criar produtor com dados inválidos."
            );

            throw;
        }
    }
}

using AgroSolutions.Contracts;
using AgroSolutions.Medicoes.Application.Services;
using AgroSolutions.Medicoes.Domain.Entities;
using AgroSolutions.Medicoes.Domain.Exceptions;
using AgroSolutions.Medicoes.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace AgroSolutions.Medicoes.Application.Tests.Services;

public class PropriedadeServiceTests
{
    private readonly Mock<IPropriedadeRepository> _repository;
    private readonly PropriedadeService _service;
    private readonly Mock<ILogger<PropriedadeService>> _mockLogger;

    public PropriedadeServiceTests()
    {
        _repository = new Mock<IPropriedadeRepository>();
        _mockLogger = new Mock<ILogger<PropriedadeService>>();
        _service = new PropriedadeService(_repository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ProcessarAsync_Deve_criar_propriedade_e_salvar()
    {
        //Arrange
        var msg = new PropriedadeDataMessage(
            Guid.NewGuid(),
            "Propriedade A",
            Guid.NewGuid()
        );

        _repository
            .Setup(p => p.AddAsync(It.IsAny<Propriedade>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        //Act
        await _service.ProcessarAsync(msg, CancellationToken.None);

        //Assert
       _repository.Verify(r => r.AddAsync(It.IsAny<Propriedade>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessarAsync_Deve_lancar_excecao_e_nao_salvar_propriedade()
    {
        //Arrange
        var msg = new PropriedadeDataMessage(
            Guid.Empty,
            "Propriedade A",
            Guid.NewGuid()
        );

        //Act & Assert
        await Should.ThrowAsync<DomainException>(
            () => _service.ProcessarAsync(msg, CancellationToken.None)
        );

        _repository.Verify(r => r.AddAsync(It.IsAny<Propriedade>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

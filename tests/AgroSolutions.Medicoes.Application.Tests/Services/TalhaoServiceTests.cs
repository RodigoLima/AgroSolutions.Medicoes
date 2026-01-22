using AgroSolutions.Contracts;
using AgroSolutions.Medicoes.Application.Services;
using AgroSolutions.Medicoes.Domain.Entities;
using AgroSolutions.Medicoes.Domain.Exceptions;
using AgroSolutions.Medicoes.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace AgroSolutions.Medicoes.Application.Tests.Services;

public class TalhaoServiceTests
{
    private readonly Mock<ITalhaoRepository> _repository;
    private readonly Mock<ILogger<TalhaoService>> _mockLogger;
    private readonly TalhaoService _service;

    public TalhaoServiceTests()
    {
        _repository = new Mock<ITalhaoRepository>();
        _mockLogger = new Mock<ILogger<TalhaoService>>();
        _service = new TalhaoService(_repository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ProcessarAsync_Deve_criar_talhao_e_salvar()
    {
        //Arrange
        var msg = new TalhaoDataMessage(
            Guid.NewGuid(),
            "Talhão A",
            Guid.NewGuid()
        );

        _repository
            .Setup(p => p.AddAsync(It.IsAny<Talhao>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        //Act
        await _service.ProcessarAsync(msg, CancellationToken.None);

        //Assert
        _repository.Verify(r => r.AddAsync(It.IsAny<Talhao>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessarAsync_Deve_lancar_excecao_e_nao_salvar_talhao()
    {
        //Arrange
        var msg = new TalhaoDataMessage(
            Guid.Empty,
            "Talhão A",
            Guid.NewGuid()
        );

        //Act & Assert
        await Should.ThrowAsync<DomainException>(
            () => _service.ProcessarAsync(msg, CancellationToken.None)
        );

        _repository.Verify(r => r.AddAsync(It.IsAny<Talhao>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

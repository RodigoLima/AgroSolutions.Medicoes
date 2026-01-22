using AgroSolutions.Medicoes.Application.Contracts;
using AgroSolutions.Medicoes.Application.Services;
using AgroSolutions.Medicoes.Domain.Entities;
using AgroSolutions.Medicoes.Domain.Exceptions;
using AgroSolutions.Medicoes.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace AgroSolutions.Medicoes.Application.Tests.Services;

public class ProdutorServiceTests
{
    private readonly Mock<IProdutorRepository> _repository;
    private readonly ProdutorService _service;
    private readonly Mock<ILogger<ProdutorService>> _mockLogger;

    public ProdutorServiceTests()
    {
        _repository = new Mock<IProdutorRepository>();
        _mockLogger = new Mock<ILogger<ProdutorService>>();
        _service = new ProdutorService(_repository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ProcessarAsync_Deve_criar_produtor_e_salvar()
    {
        //Arrange
        var msg = new ProdutorDataMessage(
            Guid.NewGuid(),
            "test@mail.com"
        );

        _repository
            .Setup(p => p.AddAsync(It.IsAny<Produtor>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        //Act
        await _service.ProcessarAsync(msg, CancellationToken.None);

        //Assert
        _repository.Verify(r => r.AddAsync(It.IsAny<Produtor>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessarAsync_Deve_lancar_excecao_e_nao_salvar_produtor()
    {
        //Arrange
        var msg = new ProdutorDataMessage(
            Guid.Empty,
            "test@mail.com"
        );

        //Act & Assert
        await Should.ThrowAsync<DomainException>(
            () => _service.ProcessarAsync(msg, CancellationToken.None)
        );

        _repository.Verify(r => r.AddAsync(It.IsAny<Produtor>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

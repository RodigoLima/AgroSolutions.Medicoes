using AgroSolutions.Contracts;
using AgroSolutions.Medicoes.Application.Services;
using AgroSolutions.Medicoes.Domain.Entities;
using AgroSolutions.Medicoes.Domain.Enums;
using AgroSolutions.Medicoes.Domain.Exceptions;
using AgroSolutions.Medicoes.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace AgroSolutions.Medicoes.Application.Tests.Services;

public class MedicaoServiceTests
{
    private readonly Mock<IMedicaoRepository> _repository;
    private readonly MedicaoService _service;
    private readonly Mock<ILogger<MedicaoService>> _mockLogger;

    public MedicaoServiceTests()
    {
        _repository = new Mock<IMedicaoRepository>();
        _mockLogger = new Mock<ILogger<MedicaoService>>();
        _service = new MedicaoService(_repository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ProcessarAsync_Deve_criar_medicao_e_salvar()
    {
        //Arrange
        var msg = new SensorDataMessage(
            Guid.NewGuid(),
            DateTime.Now,
            TipoMedicao.Precipitacao,
            10
        );

        _repository
            .Setup(p => p.AddAsync(It.IsAny<Medicao>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        //Act
        await _service.ProcessarAsync(msg, CancellationToken.None);

        //Assert
        _repository.Verify(r => r.AddAsync(It.IsAny<Medicao>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessarAsync_Deve_lancar_excecao_e_nao_salvar_medicao()
    {
        //Arrange
        var msg = new SensorDataMessage(
            Guid.Empty,
            DateTime.Now,
            TipoMedicao.Precipitacao,
            10
        );

        //Act & Assert
        await Should.ThrowAsync<DomainException>(
            () => _service.ProcessarAsync(msg, CancellationToken.None)
        );

        _repository.Verify(r => r.AddAsync(It.IsAny<Medicao>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

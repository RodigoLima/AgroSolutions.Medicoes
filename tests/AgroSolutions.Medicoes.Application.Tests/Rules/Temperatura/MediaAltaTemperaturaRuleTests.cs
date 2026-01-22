using AgroSolutions.Medicoes.Application.DTOs;
using AgroSolutions.Medicoes.Application.Interfaces.Queries;
using AgroSolutions.Medicoes.Application.Interfaces.Services;
using AgroSolutions.Medicoes.Application.Rules.Contexts;
using AgroSolutions.Medicoes.Application.Rules.Implementations.Temperatura;
using AgroSolutions.Medicoes.Domain.Enums;
using AgroSolutions.Medicoes.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace AgroSolutions.Medicoes.Application.Tests.Rules.Temperatura;

public class MediaAltaTemperaturaRuleTests
{
    private readonly Mock<IAlertaMedicaoQueryRepository> _alertaMedicaoQueryRepository;
    private readonly Mock<IEmailService> _emailService;
    private readonly Mock<IAlertaRepository> _alertaRepository;
    private readonly Mock<ILogger<MediaAltaTemperaturaRule>> _mockLogger;
    private readonly MediaAltaTemperaturaRule _rule;

    public MediaAltaTemperaturaRuleTests()
    {
        _alertaMedicaoQueryRepository = new Mock<IAlertaMedicaoQueryRepository>();
        _emailService = new Mock<IEmailService>();
        _alertaRepository = new Mock<IAlertaRepository>();
        _mockLogger = new Mock<ILogger<MediaAltaTemperaturaRule>>();
        _rule = new MediaAltaTemperaturaRule(
            _alertaMedicaoQueryRepository.Object,
            _emailService.Object,
            _alertaRepository.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public void IsApplicable_Deve_Retornar_True_Quando_Tipo_For_Temperatura()
    {
        //Arrange
        var context = new RegraPeriodoContext(
            TipoMedicao.Temperatura,
            DateTime.UtcNow
        );

        //Act
        var result = _rule.IsApplicable(context);

        //Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsApplicable_Deve_Retornar_False_Quando_Tipo_Nao_For_Temperatura()
    {
        //Arrange
        var context = new RegraPeriodoContext(
            TipoMedicao.Precipitacao,
            DateTime.UtcNow
        );

        //Act
        var result = _rule.IsApplicable(context);

        //Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task ValidateAsync_Deve_Enviar_Email_Quando_Media_For_Maior_Que_Limite()
    {
        // Arrange
        var referencia = DateTime.UtcNow;

        var context = new RegraPeriodoContext(
            TipoMedicao.Temperatura,
            referencia
        );

        _alertaMedicaoQueryRepository
            .Setup(a => a.ObtemMedicaoMediaAsync(TipoMedicao.Temperatura, referencia.AddHours(-6), referencia, CancellationToken.None))
            .ReturnsAsync(new List<MedicaoMediaDTO>
            {
                new()
                    {
                        MediaValor = 35
                    }
            });

        // Act
        await _rule.ValidateAsync(context, CancellationToken.None);

        // Assert
        // await _emailService.Received(1)
        //     .EnviarEmailAsync(
        //         Arg.Any<string>(),
        //         Arg.Any<string>(),
        //         Arg.Any<string>());

        _emailService.Verify(e => e.EnviarEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ValidateAsync_Nao_Deve_Enviar_Email_Quando_Media_For_Igual_Ao_Limite()
    {
        // Arrange
        var referencia = DateTime.UtcNow;

        var context = new RegraPeriodoContext(
            TipoMedicao.Temperatura,
            referencia
        );

        _alertaMedicaoQueryRepository
            .Setup(a => a.ObtemMedicaoMediaAsync(TipoMedicao.Temperatura, referencia.AddHours(-6), referencia, CancellationToken.None))
            .ReturnsAsync(new List<MedicaoMediaDTO>
            {
                new()
                    {
                        MediaValor = 30
                    }
            });

        // Act
        await _rule.ValidateAsync(context, CancellationToken.None);

        // Assert
        _emailService.Verify(e => e.EnviarEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_Nao_Deve_Enviar_Email_Quando_Media_For_Menor_Que_Limite()
    {
        // Arrange
        var referencia = DateTime.UtcNow;

        var context = new RegraPeriodoContext(
            TipoMedicao.Temperatura,
            referencia
        );

        _alertaMedicaoQueryRepository
            .Setup(a => a.ObtemMedicaoMediaAsync(TipoMedicao.Temperatura, referencia.AddHours(-6), referencia, CancellationToken.None))
            .ReturnsAsync(new List<MedicaoMediaDTO>
            {
                new()
                    {
                        MediaValor = 25
                    }
            });

        // Act
        await _rule.ValidateAsync(context, CancellationToken.None);

        // Assert
        _emailService.Verify(e => e.EnviarEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_Deve_Consultar_Repositorio_Com_Periodo_De_6Horas()
    {
        // Arrange
        var referencia = DateTime.UtcNow;

        var context = new RegraPeriodoContext(
            TipoMedicao.Temperatura,
            referencia
        );

        _alertaMedicaoQueryRepository
            .Setup(a => a.ObtemMedicaoMediaAsync(TipoMedicao.Temperatura, referencia.AddHours(-6), referencia, CancellationToken.None))
            .ReturnsAsync(new List<MedicaoMediaDTO>
            {
                new()
                    {
                        MediaValor = 25
                    }
            });

        // Act
        await _rule.ValidateAsync(context, CancellationToken.None);

        // Assert

        _alertaMedicaoQueryRepository.Verify(e => e.ObtemMedicaoMediaAsync(
            TipoMedicao.Temperatura,
                referencia.AddHours(-6),
                referencia,
                CancellationToken.None
        ), Times.Once);
    }
}

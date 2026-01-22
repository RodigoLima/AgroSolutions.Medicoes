using System.Diagnostics;
using AgroSolutions.Medicoes.Application.Builders;
using AgroSolutions.Medicoes.Application.Interfaces.Queries;
using AgroSolutions.Medicoes.Application.Interfaces.Services;
using AgroSolutions.Medicoes.Application.Rules.Abstractions;
using AgroSolutions.Medicoes.Application.Rules.Contexts;
using AgroSolutions.Medicoes.Domain.Entities;
using AgroSolutions.Medicoes.Domain.Enums;
using AgroSolutions.Medicoes.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace AgroSolutions.Medicoes.Application.Rules.Implementations.Temperatura;

public class MediaAltaTemperaturaRule(
    IAlertaMedicaoQueryRepository _alertaMedicaoQueryRepository, 
    IEmailService _emailService,
    IAlertaRepository _alertaRepository,
    ILogger<MediaAltaTemperaturaRule> _logger
) : IRule<RegraPeriodoContext>
{
    private const int PERIODOHORAS = 6;
    private const double LIMITE = 30;

    public bool IsApplicable(RegraPeriodoContext context)
        => context.Tipo == TipoMedicao.Temperatura;

    public async Task ValidateAsync(RegraPeriodoContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando a avaliação da regra de Altas temperaturas.");
        var stopwatch = Stopwatch.StartNew();
        var fim = context.DataReferencia;
        var inicio = fim.AddHours(-PERIODOHORAS);
        _logger.LogInformation(
            "Vai buscar a média entre os dias {inicio} e {fim} (datas em UTC)", 
            inicio, 
            fim
        );

        var mediasTemperatura = await _alertaMedicaoQueryRepository.ObtemMedicaoMediaAsync(context.Tipo, inicio, fim, cancellationToken);

        foreach(var media in mediasTemperatura)
        {
            _logger.LogInformation(
                "Calculou a média {media} para o talhão '{talhao}' da propriedade '{propriedade}'", 
                media.MediaValor, 
                media.NomeTalhao, 
                media.NomePropriedade
            );

            if(media.MediaValor > LIMITE)
            {
                var enviouAlerta = await _alertaRepository.ExistsAsync(
                    media.IdTalhao, 
                    inicio, 
                    context.Tipo, 
                    cancellationToken
                );

                if (!enviouAlerta)
                {
                    await _emailService.EnviarEmailAsync(
                        media.EmailProdutor, 
                        "Alerta de medição", 
                        MedicaoEmailBuilder.Build(
                            context.Tipo, 
                            media.MediaValor, 
                            PERIODOHORAS, 
                            media.NomeTalhao, 
                            media.NomePropriedade,
                            LIMITE
                        )
                    );

                    await _alertaRepository.AddAsync(new Alerta(media.IdTalhao, DateTime.UtcNow, context.Tipo), cancellationToken);
                }                
            }
        }
        _logger.LogInformation("Finalizou a avaliação da regra de Altas Temperaturas após {duracao}ms.", stopwatch.ElapsedMilliseconds);
    }
}

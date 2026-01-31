using System.Globalization;
using AgroSolutions.Medicoes.Application.Builders;
using AgroSolutions.Medicoes.Domain.Enums;
using Moq;
using Shouldly;

namespace AgroSolutions.Medicoes.Application.Tests.Builders;

public class MedicaoEmailBuilderTests
{
    [Theory]
    [InlineData(TipoAlerta.Alta_Temperatura, 38.5, 30)]
    [InlineData(TipoAlerta.Baixa_Temperatura, 5, 10)]
    public void Build_Deve_Gerar_Email_Com_Titulo_De_Temperatura(
        TipoAlerta tipo, 
        double valor, 
        double limiteEsperado
    )
    {
        // Arrange
        int horas = 6;

        // Act
        var html = MedicaoEmailBuilder.Build(
            tipoAlerta: tipo,
            valorMedio: valor,
            horasAnalisadas: horas,
            nomeTalhao: It.IsAny<string>(),
            nomePropriedade: It.IsAny<string>(),
            limiteEsperado: limiteEsperado          
        );

        // Assert
        html.ShouldContain("Alerta temperatura.");
        html.ShouldContain("°C");
        html.ShouldContain(valor.ToString("F2", CultureInfo.InvariantCulture));
    }

    [Theory]
    [InlineData(TipoAlerta.Alta_Umidade, 80, 50)]
    [InlineData(TipoAlerta.Baixa_Umidade, 0, 20)]
    public void Build_Deve_Gerar_Email_Com_Titulo_De_Umidade(
        TipoAlerta tipo, 
        double valor, 
        double limiteEsperado
    )
    {
        // Arrange
        int horas = 6;

        // Act
        var html = MedicaoEmailBuilder.Build(
            tipoAlerta: tipo,
            valorMedio: valor,
            horasAnalisadas: horas,
            nomeTalhao: It.IsAny<string>(),
            nomePropriedade: It.IsAny<string>(),
            limiteEsperado: limiteEsperado  
        );

        // Assert
        html.ShouldContain("Alerta de umidade.");
        html.ShouldContain("%");
        html.ShouldContain(valor.ToString("F2", CultureInfo.InvariantCulture));
    }

    [Theory]
    [InlineData(TipoAlerta.Excesso_chuva, 150, 60)]
    [InlineData(TipoAlerta.Seca, 10, 30)]
    public void Build_Deve_Gerar_Email_Com_Titulo_De_Precipitacao(
        TipoAlerta tipo, 
        double valor, 
        double limiteEsperado
    )
    {
        // Arrange
        int horas = 6;

        // Act
        var html = MedicaoEmailBuilder.Build(
            tipoAlerta: tipo,
            valorMedio: valor,
            horasAnalisadas: horas,
            nomeTalhao: It.IsAny<string>(),
            nomePropriedade: It.IsAny<string>(),
            limiteEsperado: limiteEsperado   
        );

        // Assert
        html.ShouldContain("Alerta de precipitação.");
        html.ShouldContain("mm");
        html.ShouldContain(valor.ToString("F2", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Build_Deve_Inserir_Valores_Formatados_No_Email()
    {
        // Act
        var html = MedicaoEmailBuilder.Build(
            TipoAlerta.Alta_Temperatura,
            valorMedio: 38.5678,
            horasAnalisadas: 6,
            nomeTalhao: It.IsAny<string>(),
            nomePropriedade: It.IsAny<string>(),
            limiteEsperado: 30.00
        );

        // Assert
        html.ShouldContain("38.57");
    }

    [Fact]
    public void Build_Deve_Conter_Descricao_Correta_Para_Altas_Temperaturas()
    {
        // Act
        var html = MedicaoEmailBuilder.Build(
            TipoAlerta.Alta_Temperatura,
            valorMedio: 40,
            horasAnalisadas: 3,
            nomeTalhao: It.IsAny<string>(),
            nomePropriedade: It.IsAny<string>(),
            limiteEsperado: 30  
        );

        // Assert
        html.ShouldContain(
            "A temperatura média registrada pelos sensores está acima do limite esperado."
        );
    }

    [Fact]
    public void Build_Deve_Conter_Descricao_Correta_Para_Baixas_Temperaturas()
    {
        // Act
        var html = MedicaoEmailBuilder.Build(
            TipoAlerta.Baixa_Temperatura,
            valorMedio: 5,
            horasAnalisadas: 3,
            nomeTalhao: It.IsAny<string>(),
            nomePropriedade: It.IsAny<string>(),
            limiteEsperado: 10  
        );

        // Assert
        html.ShouldContain(
            "A temperatura média registrada pelos sensores está abaixo do limite esperado."
        );
    }

    [Fact]
    public void Build_Deve_Conter_Descricao_Correta_Para_Alta_Umidade()
    {
        // Act
        var html = MedicaoEmailBuilder.Build(
            tipoAlerta:TipoAlerta.Alta_Umidade,
            valorMedio: 60,
            horasAnalisadas: 3,
            nomeTalhao: It.IsAny<string>(),
            nomePropriedade: It.IsAny<string>(),
            limiteEsperado:50 
        );

        // Assert
        html.ShouldContain(
            "A média da umidade registrada pelos sensores está acima do limite esperado."
        );
    }

    [Fact]
    public void Build_Deve_Conter_Descricao_Correta_Para_Baixa_Umidade()
    {
        // Act
        var html = MedicaoEmailBuilder.Build(
            tipoAlerta:TipoAlerta.Baixa_Umidade,
            valorMedio: 10,
            horasAnalisadas: 3,
            nomeTalhao: It.IsAny<string>(),
            nomePropriedade: It.IsAny<string>(),
            limiteEsperado:20
        );

        // Assert
        html.ShouldContain(
            "A média da umidade registrada pelos sensores está abaixo do limite esperado."
        );
    }

    [Fact]
    public void Build_Deve_Conter_Descricao_Correta_Para_Excesso_De_Chuva()
    {
        // Act
        var html = MedicaoEmailBuilder.Build(
            tipoAlerta:TipoAlerta.Excesso_chuva,
            valorMedio: 200,
            horasAnalisadas: 5,
            nomeTalhao: It.IsAny<string>(),
            nomePropriedade: It.IsAny<string>(),
            limiteEsperado: 50  
        );

        // Assert
        html.ShouldContain(
            "A precipitação média registrada pelos sensores está acima do limite esperado."
        );
    }

    [Fact]
    public void Build_Deve_Conter_Descricao_Correta_Para_Falta_De_Chuva()
    {
        // Act
        var html = MedicaoEmailBuilder.Build(
            tipoAlerta:TipoAlerta.Seca,
            valorMedio: 0,
            horasAnalisadas: 5,
            nomeTalhao: It.IsAny<string>(),
            nomePropriedade: It.IsAny<string>(),
            limiteEsperado: 10  
        );

        // Assert
        html.ShouldContain(
            "A precipitação média registrada pelos sensores está abaixo do limite esperado."
        );
    }

    [Fact]
    public void Build_Deve_Conter_Estrutura_Basica_De_Html()
    {
        // Act
        var html = MedicaoEmailBuilder.Build(
            tipoAlerta: TipoAlerta.Alta_Umidade,
            valorMedio: 90,
            horasAnalisadas: 8,
            nomeTalhao: It.IsAny<string>(),
            nomePropriedade: It.IsAny<string>(),
            limiteEsperado: 60  
        );

        // Assert
        html.ShouldStartWith("<!DOCTYPE html>");
        html.ShouldContain("<html");
        html.ShouldContain("<body>");
        html.ShouldContain("</html>");
    }
}

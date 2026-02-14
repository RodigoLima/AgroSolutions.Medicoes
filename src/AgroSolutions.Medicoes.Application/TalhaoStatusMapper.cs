using AgroSolutions.Medicoes.Domain.Enums;

namespace AgroSolutions.Medicoes.Application;

public static class TalhaoStatusMapper
{
    public static int TipoAlertaToStatus(TipoAlerta tipo)
    {
        return tipo switch
        {
            TipoAlerta.Seca => 2,
            TipoAlerta.Baixa_Umidade => 2,
            _ => 3
        };
    }
}

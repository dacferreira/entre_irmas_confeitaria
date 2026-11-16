namespace EntreIrmas.Domain.EnviromentVariables;

public interface IPedidoEnvironmentVariables
{
    /// <summary>Dias de antecedência exigidos entre o pedido e a data desejada.</summary>
    int PEDIDO_ANTECEDENCIA_MINIMA_DIAS { get; }

    /// <summary>Pedidos aceitos por minuto, por IP.</summary>
    int PEDIDO_LIMITE_POR_MINUTO { get; }

    /// <summary>Consultas de acompanhamento (pelo código) aceitas por minuto, por IP.</summary>
    int ACOMPANHAMENTO_LIMITE_POR_MINUTO { get; }
}

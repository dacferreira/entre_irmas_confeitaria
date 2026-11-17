namespace EntreIrmas.Domain.Entities.PedidoAggregate;

/// <summary>Empresa que faz a entrega do pedido.</summary>
public enum ParceiroEntrega
{
    Uber,

    /// <summary>99 (99Entrega).</summary>
    NoventaENove
}

public static class ParceiroEntregaExtensions
{
    /// <summary>Nome para mostrar ao cliente: "Uber" ou "99".</summary>
    public static string Nome(this ParceiroEntrega parceiro) => parceiro switch
    {
        ParceiroEntrega.Uber => "Uber",
        ParceiroEntrega.NoventaENove => "99",
        _ => parceiro.ToString()
    };
}

using EntreIrmas.Domain.Entities.PedidoAggregate;

namespace EntreIrmas.Domain.DTOs.Pedido;

/// <summary>
/// Etapa do pedido para o cliente, consultada pelo código. Público: não traz telefone, endereço
/// nem observação, só o primeiro nome.
/// </summary>
public class AcompanhamentoPedidoDto
{
    public string Codigo { get; set; } = string.Empty;
    public string PrimeiroNome { get; set; } = string.Empty;
    public TipoEntrega TipoEntrega { get; set; }
    public DateOnly DataDesejada { get; set; }
    public DateTime DataDoPedido { get; set; }
    public StatusPedido Status { get; set; }
    public decimal ValorTotal { get; set; }

    /// <summary>Taxa de entrega cobrada (nula na retirada e quando é combinada pelo WhatsApp).</summary>
    public decimal? TaxaEntrega { get; set; }
    public ParceiroEntrega? ParceiroEntrega { get; set; }

    /// <summary>Itens + taxa de entrega.</summary>
    public decimal TotalAPagar { get; set; }

    /// <summary>Link para acompanhar o entregador, depois que ele é chamado.</summary>
    public string? RastreioEntrega { get; set; }
    public List<ItemPedidoDto> Itens { get; set; } = new();
}

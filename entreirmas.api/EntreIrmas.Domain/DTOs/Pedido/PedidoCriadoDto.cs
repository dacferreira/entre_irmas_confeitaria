using EntreIrmas.Domain.Entities.PedidoAggregate;

namespace EntreIrmas.Domain.DTOs.Pedido;

public class PedidoCriadoDto
{
    public string Codigo { get; set; } = string.Empty;
    public string NomeCliente { get; set; } = string.Empty;
    public TipoEntrega TipoEntrega { get; set; }
    public DateOnly DataDesejada { get; set; }
    public decimal ValorTotal { get; set; }

    /// <summary>Taxa de entrega cobrada (nula na retirada e quando é combinada pelo WhatsApp).</summary>
    public decimal? TaxaEntrega { get; set; }
    public ParceiroEntrega? ParceiroEntrega { get; set; }

    /// <summary>Itens + taxa de entrega.</summary>
    public decimal TotalAPagar { get; set; }
    public StatusPedido Status { get; set; }
    public List<ItemPedidoDto> Itens { get; set; } = new();

    /// <summary>Link do WhatsApp com a mensagem do pedido pronta para envio.</summary>
    public string LinkWhatsApp { get; set; } = string.Empty;
}

public class ItemPedidoDto
{
    public long ProdutoId { get; set; }
    public string NomeProduto { get; set; } = string.Empty;
    public string Unidade { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal ValorTotal { get; set; }
}

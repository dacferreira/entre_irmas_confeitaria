using EntreIrmas.Domain.Entities.PedidoAggregate;

namespace EntreIrmas.Domain.DTOs.Pedido;

public class FiltroPedidoDto
{
    /// <summary>Data desejada a partir de (inclusive).</summary>
    public DateOnly? Inicio { get; set; }

    /// <summary>Data desejada até (inclusive).</summary>
    public DateOnly? Fim { get; set; }

    public StatusPedido? Status { get; set; }

    /// <summary>Código, nome do cliente ou telefone.</summary>
    public string? Busca { get; set; }

    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 20;
}

public class PedidoResumoDto
{
    public long Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string NomeCliente { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public TipoEntrega TipoEntrega { get; set; }
    public DateOnly DataDesejada { get; set; }
    public decimal ValorTotal { get; set; }
    public StatusPedido Status { get; set; }
    public int QuantidadeItens { get; set; }
    public string CadastradoPor { get; set; } = string.Empty;
    public DateTime DataEHoraDeCadastro { get; set; }
}

public class PedidoDetalheDto : PedidoResumoDto
{
    public string? Endereco { get; set; }
    public string? Observacao { get; set; }
    public decimal CustoTotal { get; set; }

    /// <summary>Taxa de entrega cobrada (nula na retirada e quando é combinada pelo WhatsApp).</summary>
    public decimal? TaxaEntrega { get; set; }
    public ParceiroEntrega? ParceiroEntrega { get; set; }

    /// <summary>Itens + taxa de entrega.</summary>
    public decimal TotalAPagar { get; set; }
    public SituacaoEntrega? SituacaoEntrega { get; set; }
    public string? RastreioEntrega { get; set; }
    public string? ErroEntrega { get; set; }

    /// <summary>Entrega com parceiro ainda sem entregador (ou com a chamada anterior falha).</summary>
    public bool PodeChamarEntregador { get; set; }
    public List<ItemPedidoAdminDto> Itens { get; set; } = new();
}

public class ItemPedidoAdminDto : ItemPedidoDto
{
    public decimal CustoUnitario { get; set; }
}

public class AlterarStatusPedidoDto
{
    public StatusPedido Status { get; set; }
}

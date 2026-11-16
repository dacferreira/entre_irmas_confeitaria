using EntreIrmas.Domain.DTOs.Endereco;
using EntreIrmas.Domain.Entities.PedidoAggregate;

namespace EntreIrmas.Domain.DTOs.Pedido;

public class CriarPedidoDto
{
    public string NomeCliente { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public TipoEntrega TipoEntrega { get; set; }

    /// <summary>Obrigatório na entrega; ignorado na retirada.</summary>
    public EnderecoDto? Endereco { get; set; }
    public DateOnly DataDesejada { get; set; }
    public string? Observacao { get; set; }
    public List<ItemCriarPedidoDto> Itens { get; set; } = new();
}

public class ItemCriarPedidoDto
{
    public long ProdutoId { get; set; }
    public int Quantidade { get; set; }
}

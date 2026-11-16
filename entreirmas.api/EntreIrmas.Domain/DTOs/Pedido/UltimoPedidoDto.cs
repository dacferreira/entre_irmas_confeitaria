namespace EntreIrmas.Domain.DTOs.Pedido;

/// <summary>Último pedido feito pelo site, para o aviso sonoro da área administrativa.</summary>
public class UltimoPedidoDto
{
    public long Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string NomeCliente { get; set; } = string.Empty;
}

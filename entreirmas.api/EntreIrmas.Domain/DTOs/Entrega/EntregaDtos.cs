using EntreIrmas.Domain.Entities.PedidoAggregate;

namespace EntreIrmas.Domain.DTOs.Entrega;

/// <summary>
/// Resultado do "Calcular entrega" no checkout: a cotação mais barata entre os parceiros, ou o aviso de que
/// a taxa será combinada pelo WhatsApp (nenhum parceiro configurado, endereço fora da área ou parceiro fora do ar).
/// </summary>
public class CotacaoEntregaDto
{
    public bool Disponivel { get; set; }
    public ParceiroEntrega? Parceiro { get; set; }
    public decimal? Valor { get; set; }

    /// <summary>Tempo estimado até a entrega, em minutos, depois que o entregador é chamado.</summary>
    public int? PrazoMinutos { get; set; }

    /// <summary>Explicação para o cliente quando não há cotação.</summary>
    public string? Mensagem { get; set; }
}

/// <summary>Cotação de um parceiro.</summary>
public record CotacaoParceiro(ParceiroEntrega Parceiro, decimal Valor, int PrazoMinutos);

/// <summary>Entrega criada no parceiro.</summary>
public record EntregaSolicitada(string IdEntrega, string? Rastreio);

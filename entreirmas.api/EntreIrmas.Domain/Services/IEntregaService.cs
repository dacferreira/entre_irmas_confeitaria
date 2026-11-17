using EntreIrmas.Domain.DTOs.Entrega;
using EntreIrmas.Domain.Entities.PedidoAggregate;

namespace EntreIrmas.Domain.Services;

/// <summary>Entrega por parceiros: escolhe a cotação mais barata e chama o entregador do parceiro escolhido.</summary>
public interface IEntregaService
{
    /// <summary>Há pelo menos um parceiro configurado.</summary>
    bool Disponivel { get; }

    /// <summary>
    /// Cota em todos os parceiros ao mesmo tempo e devolve a mais barata. Nulo quando nenhum atende o endereço
    /// ou todos falharam (as falhas ficam no log).
    /// </summary>
    Task<CotacaoParceiro?> CotarMaisBarataAsync(EnderecoEntrega destino, CancellationToken cancellationToken = default);

    /// <summary>
    /// Chama o entregador do parceiro do pedido e registra o resultado no pedido (chamado ou falhou).
    /// Não lança erro do parceiro: quem chamou só precisa gravar o pedido.
    /// </summary>
    Task ChamarEntregadorAsync(Pedido pedido, CancellationToken cancellationToken = default);
}

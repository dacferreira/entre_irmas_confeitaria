using EntreIrmas.Domain.DTOs.Entrega;
using EntreIrmas.Domain.Entities.PedidoAggregate;

namespace EntreIrmas.Domain.Services;

/// <summary>
/// Um parceiro de entrega (Uber Direct, 99Entrega ou o simulador do ambiente local). As implementações reais
/// ficam em Gateways; cada uma lança <see cref="Exceptions.GatewayException"/> quando o parceiro falha.
/// </summary>
public interface IEntregaParceiroService
{
    ParceiroEntrega Parceiro { get; }

    /// <summary>Cotação da loja até o endereço, ou nulo quando o parceiro não atende esse endereço.</summary>
    Task<CotacaoParceiro?> CotarAsync(EnderecoEntrega destino, CancellationToken cancellationToken = default);

    /// <summary>Chama o entregador para levar o pedido (de entrega) da loja até o cliente.</summary>
    Task<EntregaSolicitada> SolicitarAsync(Pedido pedido, CancellationToken cancellationToken = default);
}

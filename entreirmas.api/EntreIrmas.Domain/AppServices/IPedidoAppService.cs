using EntreIrmas.Domain.DTOs.Comum;
using EntreIrmas.Domain.DTOs.Pedido;
using EntreIrmas.Domain.Entities.PedidoAggregate;

namespace EntreIrmas.Domain.AppServices;

public interface IPedidoAppService
{
    Task<PedidoCriadoDto> CriarAsync(CriarPedidoDto dto, CancellationToken cancellationToken = default);

    /// <summary>Etapa do pedido para o cliente. Aceita o código com ou sem o "EI-", em maiúsculas ou não.</summary>
    Task<AcompanhamentoPedidoDto> AcompanharAsync(string codigo, CancellationToken cancellationToken = default);

    // Área administrativa
    Task<PaginaDto<PedidoResumoDto>> ListarAsync(FiltroPedidoDto filtro, CancellationToken cancellationToken = default);
    Task<PedidoDetalheDto> ObterAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>Último pedido feito pelo site (nulo se não houver), para o aviso de pedido novo.</summary>
    Task<UltimoPedidoDto?> ObterUltimoDoSiteAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Pedidos do site com id maior que <paramref name="depoisDe"/>, do mais antigo para o mais novo (no máximo
    /// 20), para o aviso e a impressão do recibo não perderem pedidos que chegaram juntos.
    /// </summary>
    Task<IReadOnlyList<PedidoDetalheDto>> ListarNovosDoSiteAsync(long depoisDe, CancellationToken cancellationToken = default);

    /// <summary>Pedido recebido fora do site (WhatsApp, Instagram, balcão). Já entra como confirmado.</summary>
    Task<PedidoDetalheDto> CriarManualAsync(CriarPedidoDto dto, CancellationToken cancellationToken = default);

    Task<PedidoDetalheDto> AlterarStatusAsync(long id, StatusPedido status, CancellationToken cancellationToken = default);
    Task ExcluirAsync(long id, CancellationToken cancellationToken = default);
}

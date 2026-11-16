using EntreIrmas.Domain.DTOs.Pedido;
using EntreIrmas.Domain.Entities.PedidoAggregate;
using EntreIrmas.Domain.Seedwork;

namespace EntreIrmas.Domain.Repositories;

public interface IPedidoRepository : IRepository<Pedido>
{
    Task<bool> ExisteCodigoAsync(string codigo, CancellationToken cancellationToken = default);

    /// <summary>Pedido ativo com os itens, só para leitura (acompanhamento pelo cliente).</summary>
    Task<Pedido?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);

    /// <summary>Pedido ativo de maior id cadastrado por <paramref name="cadastradoPor"/> (ex.: o site).</summary>
    Task<Pedido?> ObterUltimoAsync(string cadastradoPor, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pedidos ativos com os itens cadastrados por <paramref name="cadastradoPor"/> com id maior que
    /// <paramref name="depoisDe"/>, do mais antigo para o mais novo, até <paramref name="maximo"/>.
    /// </summary>
    Task<IReadOnlyList<Pedido>> ListarDepoisDeAsync(string cadastradoPor, long depoisDe, int maximo,
        CancellationToken cancellationToken = default);

    /// <summary>Página de pedidos com os itens, da data desejada mais recente para a mais antiga.</summary>
    Task<(IReadOnlyList<Pedido> Itens, int Total)> ListarAsync(FiltroPedidoDto filtro, CancellationToken cancellationToken = default);

    /// <summary>Pedidos com os itens cuja data desejada está no período (inclusive).</summary>
    Task<IReadOnlyList<Pedido>> ListarPorPeriodoAsync(DateOnly inicio, DateOnly fim, CancellationToken cancellationToken = default);
}

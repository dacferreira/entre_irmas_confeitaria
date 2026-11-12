using EntreIrmas.Domain.DTOs.Produto;
using EntreIrmas.Domain.Entities.ProdutoAggregate;
using EntreIrmas.Domain.Seedwork;

namespace EntreIrmas.Domain.Repositories;

public interface IProdutoRepository : IRepository<Produto>
{
    Task<IReadOnlyList<Produto>> ListarAsync(FiltroProdutoDto filtro, CancellationToken cancellationToken = default);
    Task<Produto?> ObterPorSlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Produto>> ObterPorIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default);
}

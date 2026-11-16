using EntreIrmas.Domain.DTOs.Produto;
using EntreIrmas.Domain.Entities.ProdutoAggregate;
using EntreIrmas.Domain.Seedwork;

namespace EntreIrmas.Domain.Repositories;

public interface IProdutoRepository : IRepository<Produto>
{
    Task<IReadOnlyList<Produto>> ListarAsync(FiltroProdutoDto filtro, CancellationToken cancellationToken = default);
    Task<Produto?> ObterPorSlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Produto>> ObterPorIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default);

    /// <summary>Ativos e inativos (sem os excluídos), com a categoria carregada.</summary>
    Task<IReadOnlyList<Produto>> ListarAdminAsync(FiltroProdutoAdminDto filtro, CancellationToken cancellationToken = default);

    /// <summary>Ativo ou inativo, com a categoria; não traz os excluídos.</summary>
    Task<Produto?> ObterAdminPorIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>Considera também os registros inativos e excluídos (o índice único vale para todos).</summary>
    Task<bool> ExisteSlugAsync(string slug, long? ignorarId, CancellationToken cancellationToken = default);

    /// <summary>Produtos não excluídos da categoria.</summary>
    Task<int> ContarPorCategoriaAsync(long categoriaId, CancellationToken cancellationToken = default);
}

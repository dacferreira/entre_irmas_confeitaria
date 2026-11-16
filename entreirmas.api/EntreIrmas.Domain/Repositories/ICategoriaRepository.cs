using EntreIrmas.Domain.Entities.CategoriaAggregate;
using EntreIrmas.Domain.Seedwork;

namespace EntreIrmas.Domain.Repositories;

public interface ICategoriaRepository : IRepository<Categoria>
{
    /// <summary>Categorias ativas em ordem de exibição, com os produtos ativos carregados.</summary>
    Task<IReadOnlyList<Categoria>> ListarComProdutosAsync(CancellationToken cancellationToken = default);

    /// <summary>Ativas e inativas (sem as excluídas), com os produtos não excluídos carregados.</summary>
    Task<IReadOnlyList<Categoria>> ListarAdminAsync(CancellationToken cancellationToken = default);

    /// <summary>Ativa ou inativa; não traz as excluídas.</summary>
    Task<Categoria?> ObterAdminPorIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>Considera também os registros inativos e excluídos (o índice único vale para todos).</summary>
    Task<bool> ExisteSlugAsync(string slug, long? ignorarId, CancellationToken cancellationToken = default);
}

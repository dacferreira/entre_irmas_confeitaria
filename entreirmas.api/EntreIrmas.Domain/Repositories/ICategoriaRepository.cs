using EntreIrmas.Domain.Entities.CategoriaAggregate;
using EntreIrmas.Domain.Seedwork;

namespace EntreIrmas.Domain.Repositories;

public interface ICategoriaRepository : IRepository<Categoria>
{
    /// <summary>Categorias ativas em ordem de exibição, com os produtos ativos carregados.</summary>
    Task<IReadOnlyList<Categoria>> ListarComProdutosAsync(CancellationToken cancellationToken = default);
}

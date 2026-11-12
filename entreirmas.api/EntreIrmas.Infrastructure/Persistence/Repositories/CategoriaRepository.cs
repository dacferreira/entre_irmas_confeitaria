using EntreIrmas.Domain.Entities.CategoriaAggregate;
using EntreIrmas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EntreIrmas.Infrastructure.Persistence.Repositories;

public class CategoriaRepository : RepositoryBase<Categoria>, ICategoriaRepository
{
    public CategoriaRepository(EntreIrmasDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Categoria>> ListarComProdutosAsync(CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .Include(c => c.Produtos)
            .OrderBy(c => c.Ordem)
            .ThenBy(c => c.Nome)
            .ToListAsync(cancellationToken);
}

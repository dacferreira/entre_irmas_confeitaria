using EntreIrmas.Domain.Entities.CategoriaAggregate;
using EntreIrmas.Domain.Repositories;
using EntreIrmas.Domain.Seedwork;
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

    public async Task<IReadOnlyList<Categoria>> ListarAdminAsync(CancellationToken cancellationToken = default) =>
        await DbSet
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(c => c.Estado != EntityStatus.Arquivado)
            .Include(c => c.Produtos.Where(p => p.Estado != EntityStatus.Arquivado))
            .OrderBy(c => c.Ordem)
            .ThenBy(c => c.Nome)
            .ToListAsync(cancellationToken);

    public Task<Categoria?> ObterAdminPorIdAsync(long id, CancellationToken cancellationToken = default) =>
        DbSet
            .IgnoreQueryFilters()
            .Include(c => c.Produtos.Where(p => p.Estado != EntityStatus.Arquivado))
            .FirstOrDefaultAsync(c => c.Id == id && c.Estado != EntityStatus.Arquivado, cancellationToken);

    public Task<bool> ExisteSlugAsync(string slug, long? ignorarId, CancellationToken cancellationToken = default) =>
        DbSet.IgnoreQueryFilters().AnyAsync(c => c.Slug == slug && c.Id != ignorarId, cancellationToken);
}

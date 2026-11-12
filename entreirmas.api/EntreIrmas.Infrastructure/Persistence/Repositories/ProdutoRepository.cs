using EntreIrmas.Domain.DTOs.Produto;
using EntreIrmas.Domain.Entities.ProdutoAggregate;
using EntreIrmas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EntreIrmas.Infrastructure.Persistence.Repositories;

public class ProdutoRepository : RepositoryBase<Produto>, IProdutoRepository
{
    public ProdutoRepository(EntreIrmasDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Produto>> ListarAsync(FiltroProdutoDto filtro, CancellationToken cancellationToken = default)
    {
        // O filtro de categoria ativa garante que produto de categoria inativada não apareça.
        var consulta = DbSet.AsNoTracking().Include(p => p.Categoria).Where(p => p.Categoria != null);

        if (!string.IsNullOrWhiteSpace(filtro.Categoria))
        {
            var categoria = filtro.Categoria.Trim().ToLowerInvariant();
            consulta = consulta.Where(p => p.Categoria!.Slug == categoria);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var busca = filtro.Busca.Trim();
            consulta = consulta.Where(p => p.Nome.Contains(busca) || p.Descricao.Contains(busca));
        }

        if (filtro.Destaque == true)
            consulta = consulta.Where(p => p.Destaque);

        return await consulta
            .OrderBy(p => p.Categoria!.Ordem)
            .ThenByDescending(p => p.Destaque)
            .ThenBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public Task<Produto?> ObterPorSlugAsync(string slug, CancellationToken cancellationToken = default) =>
        DbSet.AsNoTracking()
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.Categoria != null, cancellationToken);

    public async Task<IReadOnlyList<Produto>> ObterPorIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default)
    {
        var lista = ids.ToList();
        return await DbSet
            .Where(p => lista.Contains(p.Id) && p.Categoria != null)
            .ToListAsync(cancellationToken);
    }
}

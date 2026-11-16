using EntreIrmas.Domain.DTOs.Produto;
using EntreIrmas.Domain.Entities.ProdutoAggregate;
using EntreIrmas.Domain.Repositories;
using EntreIrmas.Domain.Seedwork;
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

        // Os de encomenda vêm por último (o cardápio mostra primeiro o que tem para pronta entrega).
        return await consulta
            .OrderBy(p => p.Encomenda)
            .ThenBy(p => p.Categoria!.Ordem)
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

    public async Task<IReadOnlyList<Produto>> ListarAdminAsync(FiltroProdutoAdminDto filtro,
        CancellationToken cancellationToken = default)
    {
        var consulta = NaoExcluidos().AsNoTracking();

        if (filtro.CategoriaId is > 0)
            consulta = consulta.Where(p => p.CategoriaId == filtro.CategoriaId);

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var busca = filtro.Busca.Trim();
            consulta = consulta.Where(p => p.Nome.Contains(busca));
        }

        if (filtro.Ativo.HasValue)
        {
            var estado = filtro.Ativo.Value ? EntityStatus.Ativo : EntityStatus.Inativo;
            consulta = consulta.Where(p => p.Estado == estado);
        }

        return await consulta
            .OrderBy(p => p.Categoria!.Ordem)
            .ThenBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public Task<Produto?> ObterAdminPorIdAsync(long id, CancellationToken cancellationToken = default) =>
        NaoExcluidos().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<bool> ExisteSlugAsync(string slug, long? ignorarId, CancellationToken cancellationToken = default) =>
        DbSet.IgnoreQueryFilters().AnyAsync(p => p.Slug == slug && p.Id != ignorarId, cancellationToken);

    public Task<int> ContarPorCategoriaAsync(long categoriaId, CancellationToken cancellationToken = default) =>
        DbSet.IgnoreQueryFilters()
            .CountAsync(p => p.CategoriaId == categoriaId && p.Estado != EntityStatus.Arquivado, cancellationToken);

    // Área administrativa: ativos e inativos, com a categoria (mesmo inativa).
    private IQueryable<Produto> NaoExcluidos() =>
        DbSet.IgnoreQueryFilters()
            .Include(p => p.Categoria)
            .Where(p => p.Estado != EntityStatus.Arquivado);
}

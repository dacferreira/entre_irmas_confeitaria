using EntreIrmas.Domain.DTOs.Pedido;
using EntreIrmas.Domain.Entities.PedidoAggregate;
using EntreIrmas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EntreIrmas.Infrastructure.Persistence.Repositories;

public class PedidoRepository : RepositoryBase<Pedido>, IPedidoRepository
{
    public const int TamanhoMaximoDaPagina = 100;

    public PedidoRepository(EntreIrmasDbContext context) : base(context)
    {
    }

    public override Task<Pedido?> ObterPorIdAsync(long id, CancellationToken cancellationToken = default) =>
        DbSet.Include(p => p.Itens).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<bool> ExisteCodigoAsync(string codigo, CancellationToken cancellationToken = default) =>
        DbSet.IgnoreQueryFilters().AnyAsync(p => p.Codigo == codigo, cancellationToken);

    public Task<Pedido?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default) =>
        DbSet.AsNoTracking().Include(p => p.Itens).FirstOrDefaultAsync(p => p.Codigo == codigo, cancellationToken);

    public Task<Pedido?> ObterUltimoAsync(string cadastradoPor, CancellationToken cancellationToken = default) =>
        DbSet.AsNoTracking()
            .Where(p => p.CadastradoPor == cadastradoPor)
            .OrderByDescending(p => p.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<Pedido>> ListarDepoisDeAsync(string cadastradoPor, long depoisDe, int maximo,
        CancellationToken cancellationToken = default) =>
        await DbSet.AsNoTracking()
            .Include(p => p.Itens)
            .Where(p => p.CadastradoPor == cadastradoPor && p.Id > depoisDe)
            .OrderBy(p => p.Id)
            .Take(maximo)
            .ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<Pedido> Itens, int Total)> ListarAsync(FiltroPedidoDto filtro,
        CancellationToken cancellationToken = default)
    {
        var consulta = DbSet.AsNoTracking();

        if (filtro.Inicio.HasValue)
            consulta = consulta.Where(p => p.DataDesejada >= filtro.Inicio.Value);
        if (filtro.Fim.HasValue)
            consulta = consulta.Where(p => p.DataDesejada <= filtro.Fim.Value);
        if (filtro.Status.HasValue)
            consulta = consulta.Where(p => p.Status == filtro.Status.Value);

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var busca = filtro.Busca.Trim();
            var digitos = new string(busca.Where(char.IsDigit).ToArray());
            consulta = consulta.Where(p =>
                p.Codigo.Contains(busca)
                || p.NomeCliente.Contains(busca)
                || (digitos.Length >= 4 && p.Telefone.Contains(digitos)));
        }

        var total = await consulta.CountAsync(cancellationToken);
        var tamanho = Math.Clamp(filtro.TamanhoPagina, 1, TamanhoMaximoDaPagina);
        var pagina = Math.Max(1, filtro.Pagina);

        var itens = await consulta
            .Include(p => p.Itens)
            .OrderByDescending(p => p.DataDesejada)
            .ThenByDescending(p => p.Id)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync(cancellationToken);

        return (itens, total);
    }

    public async Task<IReadOnlyList<Pedido>> ListarPorPeriodoAsync(DateOnly inicio, DateOnly fim,
        CancellationToken cancellationToken = default) =>
        await DbSet
            .AsNoTracking()
            .Include(p => p.Itens)
            .Where(p => p.DataDesejada >= inicio && p.DataDesejada <= fim)
            .ToListAsync(cancellationToken);
}

using EntreIrmas.Domain.Entities.PedidoAggregate;
using EntreIrmas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EntreIrmas.Infrastructure.Persistence.Repositories;

public class PedidoRepository : RepositoryBase<Pedido>, IPedidoRepository
{
    public PedidoRepository(EntreIrmasDbContext context) : base(context)
    {
    }

    public override Task<Pedido?> ObterPorIdAsync(long id, CancellationToken cancellationToken = default) =>
        DbSet.Include(p => p.Itens).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<bool> ExisteCodigoAsync(string codigo, CancellationToken cancellationToken = default) =>
        DbSet.IgnoreQueryFilters().AnyAsync(p => p.Codigo == codigo, cancellationToken);
}

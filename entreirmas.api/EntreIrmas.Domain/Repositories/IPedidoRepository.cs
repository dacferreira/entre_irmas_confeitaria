using EntreIrmas.Domain.Entities.PedidoAggregate;
using EntreIrmas.Domain.Seedwork;

namespace EntreIrmas.Domain.Repositories;

public interface IPedidoRepository : IRepository<Pedido>
{
    Task<bool> ExisteCodigoAsync(string codigo, CancellationToken cancellationToken = default);
}

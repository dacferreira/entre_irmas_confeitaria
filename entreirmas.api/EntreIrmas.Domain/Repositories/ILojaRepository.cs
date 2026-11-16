using EntreIrmas.Domain.Entities.LojaAggregate;
using EntreIrmas.Domain.Seedwork;

namespace EntreIrmas.Domain.Repositories;

public interface ILojaRepository : IRepository<Loja>
{
    /// <summary>O registro único da loja (nulo se o script do banco ainda não criou).</summary>
    Task<Loja?> ObterAsync(CancellationToken cancellationToken = default);
}

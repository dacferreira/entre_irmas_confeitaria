using EntreIrmas.Domain.Entities.LojaAggregate;
using EntreIrmas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EntreIrmas.Infrastructure.Persistence.Repositories;

public class LojaRepository : RepositoryBase<Loja>, ILojaRepository
{
    public LojaRepository(EntreIrmasDbContext context) : base(context)
    {
    }

    public Task<Loja?> ObterAsync(CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(l => l.Id == Loja.IdUnico, cancellationToken);
}

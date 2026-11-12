using EntreIrmas.Domain.Seedwork;
using Microsoft.EntityFrameworkCore;

namespace EntreIrmas.Infrastructure.Persistence.Repositories;

public abstract class RepositoryBase<T> : IRepository<T> where T : Entity, IAggregateRoot
{
    protected readonly EntreIrmasDbContext Context;
    protected readonly DbSet<T> DbSet;

    protected RepositoryBase(EntreIrmasDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual Task<T?> ObterPorIdAsync(long id, CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public virtual async Task AdicionarAsync(T entidade, CancellationToken cancellationToken = default) =>
        await DbSet.AddAsync(entidade, cancellationToken);

    public Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default) =>
        Context.SaveChangesAsync(cancellationToken);
}

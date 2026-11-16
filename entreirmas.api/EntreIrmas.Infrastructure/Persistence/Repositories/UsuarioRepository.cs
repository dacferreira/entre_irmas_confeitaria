using EntreIrmas.Domain.Entities.UsuarioAggregate;
using EntreIrmas.Domain.Repositories;
using EntreIrmas.Domain.Seedwork;
using Microsoft.EntityFrameworkCore;

namespace EntreIrmas.Infrastructure.Persistence.Repositories;

public class UsuarioRepository : RepositoryBase<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(EntreIrmasDbContext context) : base(context)
    {
    }

    public Task<Usuario?> ObterPorLoginAsync(string login, CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(u => u.Login == login, cancellationToken);

    public async Task<IReadOnlyList<Usuario>> ListarAdminAsync(CancellationToken cancellationToken = default) =>
        await NaoExcluidos()
            .AsNoTracking()
            .OrderBy(u => u.Nome)
            .ToListAsync(cancellationToken);

    public Task<Usuario?> ObterAdminPorIdAsync(long id, CancellationToken cancellationToken = default) =>
        NaoExcluidos().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<bool> ExisteLoginAsync(string login, long? ignorarId, CancellationToken cancellationToken = default) =>
        DbSet.IgnoreQueryFilters().AnyAsync(u => u.Login == login && u.Id != ignorarId, cancellationToken);

    public Task<int> ContarAtivosAsync(CancellationToken cancellationToken = default) =>
        DbSet.CountAsync(cancellationToken);

    private IQueryable<Usuario> NaoExcluidos() =>
        DbSet.IgnoreQueryFilters().Where(u => u.Estado != EntityStatus.Arquivado);
}

using EntreIrmas.Domain.DTOs.Lancamento;
using EntreIrmas.Domain.Entities.LancamentoAggregate;
using EntreIrmas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EntreIrmas.Infrastructure.Persistence.Repositories;

public class LancamentoRepository : RepositoryBase<Lancamento>, ILancamentoRepository
{
    public LancamentoRepository(EntreIrmasDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Lancamento>> ListarAsync(FiltroLancamentoDto filtro,
        CancellationToken cancellationToken = default)
    {
        var consulta = DbSet.AsNoTracking();

        if (filtro.Inicio.HasValue)
            consulta = consulta.Where(l => l.Data >= filtro.Inicio.Value);
        if (filtro.Fim.HasValue)
            consulta = consulta.Where(l => l.Data <= filtro.Fim.Value);
        if (filtro.Tipo.HasValue)
            consulta = consulta.Where(l => l.Tipo == filtro.Tipo.Value);

        return await consulta
            .OrderByDescending(l => l.Data)
            .ThenByDescending(l => l.Id)
            .ToListAsync(cancellationToken);
    }
}

using EntreIrmas.Domain.DTOs.Lancamento;
using EntreIrmas.Domain.Entities.LancamentoAggregate;
using EntreIrmas.Domain.Seedwork;

namespace EntreIrmas.Domain.Repositories;

public interface ILancamentoRepository : IRepository<Lancamento>
{
    /// <summary>Lançamentos do filtro, do mais recente para o mais antigo.</summary>
    Task<IReadOnlyList<Lancamento>> ListarAsync(FiltroLancamentoDto filtro, CancellationToken cancellationToken = default);
}

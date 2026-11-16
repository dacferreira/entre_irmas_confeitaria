using EntreIrmas.Domain.DTOs.Lancamento;

namespace EntreIrmas.Domain.AppServices;

public interface ILancamentoAppService
{
    Task<IReadOnlyList<LancamentoDto>> ListarAsync(FiltroLancamentoDto filtro, CancellationToken cancellationToken = default);
    Task<LancamentoDto> CriarAsync(SalvarLancamentoDto dto, CancellationToken cancellationToken = default);
    Task<LancamentoDto> AtualizarAsync(long id, SalvarLancamentoDto dto, CancellationToken cancellationToken = default);
    Task ExcluirAsync(long id, CancellationToken cancellationToken = default);
}

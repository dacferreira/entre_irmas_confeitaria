using EntreIrmas.Domain.DTOs.Loja;

namespace EntreIrmas.Domain.AppServices;

public interface ILojaAppService
{
    /// <summary>Situação para o site. Sem o registro no banco, a loja é considerada aberta.</summary>
    Task<SituacaoLojaDto> ObterSituacaoAsync(CancellationToken cancellationToken = default);

    // Área administrativa
    Task<LojaAdminDto> ObterAsync(CancellationToken cancellationToken = default);
    Task<LojaAdminDto> AlterarAsync(AlterarLojaDto dto, CancellationToken cancellationToken = default);
}

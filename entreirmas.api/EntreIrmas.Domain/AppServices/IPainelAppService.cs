using EntreIrmas.Domain.DTOs.Painel;

namespace EntreIrmas.Domain.AppServices;

public interface IPainelAppService
{
    /// <summary>Resumo do período. Sem datas, usa o mês corrente.</summary>
    Task<PainelDto> ObterAsync(DateOnly? inicio, DateOnly? fim, CancellationToken cancellationToken = default);
}

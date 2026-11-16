using EntreIrmas.Domain.DTOs.Autenticacao;

namespace EntreIrmas.Domain.AppServices;

public interface IAutenticacaoAppService
{
    Task<SessaoDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
}

using EntreIrmas.Domain.DTOs.Usuario;

namespace EntreIrmas.Domain.AppServices;

public interface IUsuarioAppService
{
    Task<IReadOnlyList<UsuarioDto>> ListarAsync(CancellationToken cancellationToken = default);
    Task<UsuarioDto> CriarAsync(SalvarUsuarioDto dto, CancellationToken cancellationToken = default);
    Task<UsuarioDto> AtualizarAsync(long id, SalvarUsuarioDto dto, CancellationToken cancellationToken = default);
    Task AlterarSituacaoAsync(long id, bool ativo, CancellationToken cancellationToken = default);
    Task ExcluirAsync(long id, CancellationToken cancellationToken = default);
}

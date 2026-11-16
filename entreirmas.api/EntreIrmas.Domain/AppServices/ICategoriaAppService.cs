using EntreIrmas.Domain.DTOs.Categoria;

namespace EntreIrmas.Domain.AppServices;

public interface ICategoriaAppService
{
    Task<IReadOnlyList<CategoriaDto>> ListarAsync(CancellationToken cancellationToken = default);

    // Área administrativa
    Task<IReadOnlyList<CategoriaAdminDto>> ListarAdminAsync(CancellationToken cancellationToken = default);
    Task<CategoriaAdminDto> CriarAsync(SalvarCategoriaDto dto, CancellationToken cancellationToken = default);
    Task<CategoriaAdminDto> AtualizarAsync(long id, SalvarCategoriaDto dto, CancellationToken cancellationToken = default);
    Task AlterarSituacaoAsync(long id, bool ativo, CancellationToken cancellationToken = default);
    Task ExcluirAsync(long id, CancellationToken cancellationToken = default);
}

using EntreIrmas.Domain.DTOs.Categoria;

namespace EntreIrmas.Domain.AppServices;

public interface ICategoriaAppService
{
    Task<IReadOnlyList<CategoriaDto>> ListarAsync(CancellationToken cancellationToken = default);
}

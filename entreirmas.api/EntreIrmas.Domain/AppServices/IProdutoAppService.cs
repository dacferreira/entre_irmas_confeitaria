using EntreIrmas.Domain.DTOs.Produto;

namespace EntreIrmas.Domain.AppServices;

public interface IProdutoAppService
{
    Task<IReadOnlyList<ProdutoDto>> ListarAsync(FiltroProdutoDto filtro, CancellationToken cancellationToken = default);
    Task<ProdutoDto> ObterPorSlugAsync(string slug, CancellationToken cancellationToken = default);
}

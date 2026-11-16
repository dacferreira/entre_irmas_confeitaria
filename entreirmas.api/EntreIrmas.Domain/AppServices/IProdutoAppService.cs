using EntreIrmas.Domain.DTOs.Produto;

namespace EntreIrmas.Domain.AppServices;

public interface IProdutoAppService
{
    Task<IReadOnlyList<ProdutoDto>> ListarAsync(FiltroProdutoDto filtro, CancellationToken cancellationToken = default);
    Task<ProdutoDto> ObterPorSlugAsync(string slug, CancellationToken cancellationToken = default);

    // Área administrativa
    Task<IReadOnlyList<ProdutoAdminDto>> ListarAdminAsync(FiltroProdutoAdminDto filtro, CancellationToken cancellationToken = default);
    Task<ProdutoAdminDto> ObterAdminAsync(long id, CancellationToken cancellationToken = default);
    Task<ProdutoAdminDto> CriarAsync(SalvarProdutoDto dto, CancellationToken cancellationToken = default);
    Task<ProdutoAdminDto> AtualizarAsync(long id, SalvarProdutoDto dto, CancellationToken cancellationToken = default);
    Task AlterarSituacaoAsync(long id, bool ativo, CancellationToken cancellationToken = default);
    Task ExcluirAsync(long id, CancellationToken cancellationToken = default);

    // Foto
    /// <summary>Troca (ou cria) a foto do produto. Aceita JPG, PNG ou WEBP de até 2 MB.</summary>
    Task<ProdutoAdminDto> DefinirImagemAsync(long id, byte[] conteudo, CancellationToken cancellationToken = default);

    /// <summary>Apaga a foto: o site volta a usar a ilustração.</summary>
    Task<ProdutoAdminDto> RemoverImagemAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>Foto para o site (pública). Lança ObjectNotFoundException se o produto não tiver foto.</summary>
    Task<ImagemDto> ObterImagemAsync(long id, CancellationToken cancellationToken = default);
}

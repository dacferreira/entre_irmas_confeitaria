using EntreIrmas.Domain.Entities.ProdutoAggregate;
using EntreIrmas.Domain.Seedwork;

namespace EntreIrmas.Domain.Repositories;

public interface IImagemProdutoRepository : IRepository<ImagemProduto>
{
    Task<ImagemProduto?> ObterPorProdutoAsync(long produtoId, CancellationToken cancellationToken = default);

    /// <summary>Apaga de fato (sem exclusão lógica): não há por que guardar os bytes de uma foto trocada.</summary>
    void Remover(ImagemProduto imagem);
}

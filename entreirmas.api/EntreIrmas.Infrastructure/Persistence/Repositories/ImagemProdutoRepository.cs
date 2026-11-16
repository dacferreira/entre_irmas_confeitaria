using EntreIrmas.Domain.Entities.ProdutoAggregate;
using EntreIrmas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EntreIrmas.Infrastructure.Persistence.Repositories;

public class ImagemProdutoRepository : RepositoryBase<ImagemProduto>, IImagemProdutoRepository
{
    public ImagemProdutoRepository(EntreIrmasDbContext context) : base(context)
    {
    }

    public Task<ImagemProduto?> ObterPorProdutoAsync(long produtoId, CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(i => i.ProdutoId == produtoId, cancellationToken);

    public void Remover(ImagemProduto imagem) => DbSet.Remove(imagem);
}

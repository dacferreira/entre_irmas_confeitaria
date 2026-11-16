using EntreIrmas.Domain.Entities.CategoriaAggregate;
using EntreIrmas.Domain.Entities.LancamentoAggregate;
using EntreIrmas.Domain.Entities.LojaAggregate;
using EntreIrmas.Domain.Entities.PedidoAggregate;
using EntreIrmas.Domain.Entities.ProdutoAggregate;
using EntreIrmas.Domain.Entities.UsuarioAggregate;
using Microsoft.EntityFrameworkCore;

namespace EntreIrmas.Infrastructure.Persistence;

/// <summary>
/// Contexto do EF Core. O schema é versionado pelos scripts Flyway do repositório entreirmas.dados;
/// não há migrations do EF.
/// </summary>
public class EntreIrmasDbContext : DbContext
{
    public EntreIrmasDbContext(DbContextOptions<EntreIrmasDbContext> options) : base(options)
    {
    }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<ImagemProduto> ImagensDeProduto => Set<ImagemProduto>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Lancamento> Lancamentos => Set<Lancamento>();
    public DbSet<Loja> Lojas => Set<Loja>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EntreIrmasDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

using EntreIrmas.Domain.Entities.CategoriaAggregate;
using EntreIrmas.Domain.Entities.PedidoAggregate;
using EntreIrmas.Domain.Entities.ProdutoAggregate;
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
    public DbSet<Pedido> Pedidos => Set<Pedido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EntreIrmasDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

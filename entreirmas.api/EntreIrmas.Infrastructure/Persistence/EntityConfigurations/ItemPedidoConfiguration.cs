using EntreIrmas.Domain.Entities.PedidoAggregate;
using EntreIrmas.Domain.Entities.ProdutoAggregate;
using EntreIrmas.Domain.Seedwork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntreIrmas.Infrastructure.Persistence.EntityConfigurations;

internal sealed class ItemPedidoConfiguration : IEntityTypeConfiguration<ItemPedido>
{
    public void Configure(EntityTypeBuilder<ItemPedido> builder)
    {
        builder.ToTable("ITEM_PEDIDO").HasKey(x => x.Id);
        builder.Property(b => b.Id).HasColumnName("ITPE_SQ_ITEM_PEDIDO").ValueGeneratedOnAdd();
        builder.Property(b => b.PedidoId).HasColumnName("PEDI_SQ_PEDIDO").IsRequired();
        builder.Property(b => b.ProdutoId).HasColumnName("PROD_SQ_PRODUTO").IsRequired();
        builder.Property(b => b.NomeProduto).HasColumnName("ITPE_NM_PRODUTO").HasMaxLength(100).IsRequired();
        builder.Property(b => b.Unidade).HasColumnName("ITPE_DS_UNIDADE").HasMaxLength(40).IsRequired();
        builder.Property(b => b.Quantidade).HasColumnName("ITPE_NR_QUANTIDADE").IsRequired();
        builder.Property(b => b.PrecoUnitario).HasColumnName("ITPE_NR_PRECO_UNITARIO").HasPrecision(10, 2).IsRequired();
        builder.Property(b => b.ValorTotal).HasColumnName("ITPE_NR_VALOR_TOTAL").HasPrecision(10, 2).IsRequired();
        builder.Property(b => b.CadastradoPor).HasColumnName("ITPE_NM_CADASTRADO_POR").HasMaxLength(30).IsRequired();
        builder.Property(b => b.Estado).HasColumnName("ITPE_IN_ATIVO").HasConversion<int>().IsRequired();
        builder.Property(b => b.DataEHoraDeCadastro).HasColumnName("ITPE_DT_CADASTRO").IsRequired();

        builder.HasOne<Produto>()
            .WithMany()
            .HasForeignKey(i => i.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(p => p.Estado == EntityStatus.Ativo); // exclusão lógica
    }
}

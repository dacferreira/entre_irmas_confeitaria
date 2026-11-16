using EntreIrmas.Domain.Entities.ProdutoAggregate;
using EntreIrmas.Domain.Seedwork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntreIrmas.Infrastructure.Persistence.EntityConfigurations;

internal sealed class ImagemProdutoConfiguration : IEntityTypeConfiguration<ImagemProduto>
{
    public void Configure(EntityTypeBuilder<ImagemProduto> builder)
    {
        builder.ToTable("IMAGEM_PRODUTO").HasKey(x => x.Id);
        builder.Property(b => b.Id).HasColumnName("IMPR_SQ_IMAGEM_PRODUTO").ValueGeneratedOnAdd();
        builder.Property(b => b.ProdutoId).HasColumnName("PROD_SQ_PRODUTO").IsRequired();
        builder.Property(b => b.TipoConteudo).HasColumnName("IMPR_DS_TIPO_CONTEUDO").HasMaxLength(30).IsRequired();
        builder.Property(b => b.Conteudo).HasColumnName("IMPR_BL_CONTEUDO").HasColumnType("mediumblob").IsRequired();
        builder.Property(b => b.CadastradoPor).HasColumnName("IMPR_NM_CADASTRADO_POR").HasMaxLength(30).IsRequired();
        builder.Property(b => b.Estado).HasColumnName("IMPR_IN_ATIVO").HasConversion<int>().IsRequired();
        builder.Property(b => b.DataEHoraDeCadastro).HasColumnName("IMPR_DT_CADASTRO").IsRequired();

        builder.HasOne<Produto>()
            .WithOne()
            .HasForeignKey<ImagemProduto>(i => i.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(p => p.Estado == EntityStatus.Ativo); // exclusão lógica
    }
}

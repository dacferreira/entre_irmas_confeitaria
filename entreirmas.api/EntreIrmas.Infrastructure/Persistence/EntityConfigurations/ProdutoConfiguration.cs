using EntreIrmas.Domain.Entities.ProdutoAggregate;
using EntreIrmas.Domain.Seedwork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntreIrmas.Infrastructure.Persistence.EntityConfigurations;

internal sealed class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("PRODUTO").HasKey(x => x.Id);
        builder.Property(b => b.Id).HasColumnName("PROD_SQ_PRODUTO").ValueGeneratedOnAdd();
        builder.Property(b => b.CategoriaId).HasColumnName("CATE_SQ_CATEGORIA").IsRequired();
        builder.Property(b => b.Nome).HasColumnName("PROD_NM_NOME").HasMaxLength(100).IsRequired();
        builder.Property(b => b.Slug).HasColumnName("PROD_DS_SLUG").HasMaxLength(120).IsRequired();
        builder.Property(b => b.Descricao).HasColumnName("PROD_TX_DESCRICAO").HasMaxLength(1000).IsRequired();
        builder.Property(b => b.Preco).HasColumnName("PROD_NR_PRECO").HasPrecision(10, 2).IsRequired();
        builder.Property(b => b.Unidade).HasColumnName("PROD_DS_UNIDADE").HasMaxLength(40).IsRequired();
        builder.Property(b => b.QuantidadeMinima).HasColumnName("PROD_NR_QUANTIDADE_MINIMA").IsRequired();
        builder.Property(b => b.Ilustracao).HasColumnName("PROD_CD_ILUSTRACAO").HasMaxLength(30).IsRequired();
        builder.Property(b => b.UrlImagem).HasColumnName("PROD_DS_URL_IMAGEM").HasMaxLength(500);
        builder.Property(b => b.Destaque).HasColumnName("PROD_IN_DESTAQUE").IsRequired();
        builder.Property(b => b.CadastradoPor).HasColumnName("PROD_NM_CADASTRADO_POR").HasMaxLength(30).IsRequired();
        builder.Property(b => b.Estado).HasColumnName("PROD_IN_ATIVO").HasConversion<int>().IsRequired();
        builder.Property(b => b.DataEHoraDeCadastro).HasColumnName("PROD_DT_CADASTRO").IsRequired();

        builder.HasQueryFilter(p => p.Estado == EntityStatus.Ativo); // exclusão lógica
    }
}

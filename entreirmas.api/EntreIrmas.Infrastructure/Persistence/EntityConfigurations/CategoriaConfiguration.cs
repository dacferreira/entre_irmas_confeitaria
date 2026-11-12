using EntreIrmas.Domain.Entities.CategoriaAggregate;
using EntreIrmas.Domain.Seedwork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntreIrmas.Infrastructure.Persistence.EntityConfigurations;

internal sealed class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("CATEGORIA").HasKey(x => x.Id);
        // MySQL não tem sequence: o id é AUTO_INCREMENT (ver V1.0.1__CreateTable_Categoria.sql).
        builder.Property(b => b.Id).HasColumnName("CATE_SQ_CATEGORIA").ValueGeneratedOnAdd();
        builder.Property(b => b.Nome).HasColumnName("CATE_NM_NOME").HasMaxLength(100).IsRequired();
        builder.Property(b => b.Slug).HasColumnName("CATE_DS_SLUG").HasMaxLength(100).IsRequired();
        builder.Property(b => b.Descricao).HasColumnName("CATE_DS_DESCRICAO").HasMaxLength(300);
        builder.Property(b => b.Ordem).HasColumnName("CATE_NR_ORDEM").IsRequired();
        builder.Property(b => b.CadastradoPor).HasColumnName("CATE_NM_CADASTRADO_POR").HasMaxLength(30).IsRequired();
        builder.Property(b => b.Estado).HasColumnName("CATE_IN_ATIVO").HasConversion<int>().IsRequired();
        builder.Property(b => b.DataEHoraDeCadastro).HasColumnName("CATE_DT_CADASTRO").IsRequired();

        builder.HasMany(c => c.Produtos)
            .WithOne(p => p.Categoria)
            .HasForeignKey(p => p.CategoriaId);

        builder.Navigation(c => c.Produtos).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasQueryFilter(p => p.Estado == EntityStatus.Ativo); // exclusão lógica
    }
}

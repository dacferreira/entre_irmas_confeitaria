using EntreIrmas.Domain.Entities.LancamentoAggregate;
using EntreIrmas.Domain.Seedwork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntreIrmas.Infrastructure.Persistence.EntityConfigurations;

internal sealed class LancamentoConfiguration : IEntityTypeConfiguration<Lancamento>
{
    public void Configure(EntityTypeBuilder<Lancamento> builder)
    {
        builder.ToTable("LANCAMENTO").HasKey(x => x.Id);
        builder.Property(b => b.Id).HasColumnName("LANC_SQ_LANCAMENTO").ValueGeneratedOnAdd();
        builder.Property(b => b.Tipo).HasColumnName("LANC_CD_TIPO").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(b => b.Descricao).HasColumnName("LANC_DS_LANCAMENTO").HasMaxLength(200).IsRequired();
        builder.Property(b => b.Valor).HasColumnName("LANC_NR_VALOR").HasPrecision(10, 2).IsRequired();
        builder.Property(b => b.Data).HasColumnName("LANC_DT_LANCAMENTO").IsRequired();
        builder.Property(b => b.CadastradoPor).HasColumnName("LANC_NM_CADASTRADO_POR").HasMaxLength(30).IsRequired();
        builder.Property(b => b.Estado).HasColumnName("LANC_IN_ATIVO").HasConversion<int>().IsRequired();
        builder.Property(b => b.DataEHoraDeCadastro).HasColumnName("LANC_DT_CADASTRO").IsRequired();

        builder.HasQueryFilter(p => p.Estado == EntityStatus.Ativo); // exclusão lógica
    }
}

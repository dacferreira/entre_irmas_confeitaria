using EntreIrmas.Domain.Entities.LojaAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntreIrmas.Infrastructure.Persistence.EntityConfigurations;

internal sealed class LojaConfiguration : IEntityTypeConfiguration<Loja>
{
    public void Configure(EntityTypeBuilder<Loja> builder)
    {
        // Registro único (id 1, criado pelo script): a chave não é gerada pelo banco.
        builder.ToTable("LOJA").HasKey(x => x.Id);
        builder.Property(b => b.Id).HasColumnName("LOJA_SQ_LOJA").ValueGeneratedNever();
        builder.Property(b => b.Aberta).HasColumnName("LOJA_IN_ABERTA").IsRequired();
        builder.Property(b => b.MensagemFechada).HasColumnName("LOJA_TX_MENSAGEM_FECHADA")
            .HasMaxLength(Loja.TamanhoMaximoDaMensagem);
        builder.Property(b => b.AlteradoPor).HasColumnName("LOJA_NM_ALTERADO_POR").HasMaxLength(30).IsRequired();
        builder.Property(b => b.DataEHoraDaAlteracao).HasColumnName("LOJA_DT_ALTERACAO").IsRequired();
        builder.Property(b => b.CadastradoPor).HasColumnName("LOJA_NM_CADASTRADO_POR").HasMaxLength(30).IsRequired();
        builder.Property(b => b.Estado).HasColumnName("LOJA_IN_ATIVO").HasConversion<int>().IsRequired();
        builder.Property(b => b.DataEHoraDeCadastro).HasColumnName("LOJA_DT_CADASTRO").IsRequired();
    }
}

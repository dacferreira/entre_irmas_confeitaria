using EntreIrmas.Domain.Entities.PedidoAggregate;
using EntreIrmas.Domain.Seedwork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntreIrmas.Infrastructure.Persistence.EntityConfigurations;

internal sealed class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("PEDIDO").HasKey(x => x.Id);
        builder.Property(b => b.Id).HasColumnName("PEDI_SQ_PEDIDO").ValueGeneratedOnAdd();
        builder.Property(b => b.Codigo).HasColumnName("PEDI_CD_PEDIDO").HasMaxLength(12).IsRequired();
        builder.Property(b => b.NomeCliente).HasColumnName("PEDI_NM_CLIENTE").HasMaxLength(100).IsRequired();
        builder.Property(b => b.Telefone).HasColumnName("PEDI_NR_TELEFONE").HasMaxLength(11).IsRequired();
        builder.Property(b => b.TipoEntrega).HasColumnName("PEDI_CD_TIPO_ENTREGA").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(b => b.Endereco).HasColumnName("PEDI_DS_ENDERECO").HasMaxLength(300);
        builder.Property(b => b.DataDesejada).HasColumnName("PEDI_DT_DESEJADA").IsRequired();
        builder.Property(b => b.Observacao).HasColumnName("PEDI_TX_OBSERVACAO").HasMaxLength(500);
        builder.Property(b => b.ValorTotal).HasColumnName("PEDI_NR_VALOR_TOTAL").HasPrecision(10, 2).IsRequired();
        builder.Property(b => b.Status).HasColumnName("PEDI_CD_STATUS").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(b => b.CadastradoPor).HasColumnName("PEDI_NM_CADASTRADO_POR").HasMaxLength(30).IsRequired();
        builder.Property(b => b.Estado).HasColumnName("PEDI_IN_ATIVO").HasConversion<int>().IsRequired();
        builder.Property(b => b.DataEHoraDeCadastro).HasColumnName("PEDI_DT_CADASTRO").IsRequired();

        builder.HasMany(p => p.Itens)
            .WithOne()
            .HasForeignKey(i => i.PedidoId);

        builder.Navigation(p => p.Itens).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasQueryFilter(p => p.Estado == EntityStatus.Ativo); // exclusão lógica
    }
}

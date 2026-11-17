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
        builder.Property(b => b.Endereco).HasColumnName("PEDI_DS_ENDERECO").HasMaxLength(500);

        // Endereço em partes (V1.0.11): colunas da própria tabela PEDIDO, todas nulas na retirada.
        builder.OwnsOne(b => b.EnderecoEntrega, endereco =>
        {
            endereco.Property(e => e.Cep).HasColumnName("PEDI_NR_CEP").HasMaxLength(8);
            endereco.Property(e => e.Logradouro).HasColumnName("PEDI_DS_LOGRADOURO").HasMaxLength(150);
            endereco.Property(e => e.Numero).HasColumnName("PEDI_DS_NUMERO").HasMaxLength(20);
            endereco.Property(e => e.Complemento).HasColumnName("PEDI_DS_COMPLEMENTO").HasMaxLength(100);
            endereco.Property(e => e.Bairro).HasColumnName("PEDI_NM_BAIRRO").HasMaxLength(100);
            endereco.Property(e => e.Cidade).HasColumnName("PEDI_NM_CIDADE").HasMaxLength(100);
            endereco.Property(e => e.Uf).HasColumnName("PEDI_SG_UF").HasMaxLength(2);
        });
        builder.Property(b => b.DataDesejada).HasColumnName("PEDI_DT_DESEJADA").IsRequired();
        builder.Property(b => b.Observacao).HasColumnName("PEDI_TX_OBSERVACAO").HasMaxLength(500);
        builder.Property(b => b.ValorTotal).HasColumnName("PEDI_NR_VALOR_TOTAL").HasPrecision(10, 2).IsRequired();

        // Entrega por parceiro (V1.0.13).
        builder.Property(b => b.TaxaEntrega).HasColumnName("PEDI_NR_TAXA_ENTREGA").HasPrecision(10, 2);
        builder.Property(b => b.ParceiroEntrega).HasColumnName("PEDI_CD_PARCEIRO_ENTREGA").HasConversion<string>().HasMaxLength(20);
        builder.Property(b => b.SituacaoEntrega).HasColumnName("PEDI_CD_SITUACAO_ENTREGA").HasConversion<string>().HasMaxLength(20);
        builder.Property(b => b.IdEntregaParceiro).HasColumnName("PEDI_CD_ENTREGA_PARCEIRO").HasMaxLength(100);
        builder.Property(b => b.RastreioEntrega).HasColumnName("PEDI_DS_RASTREIO_ENTREGA").HasMaxLength(500);
        builder.Property(b => b.ErroEntrega).HasColumnName("PEDI_TX_ERRO_ENTREGA").HasMaxLength(300);
        builder.Property(b => b.Status).HasColumnName("PEDI_CD_STATUS").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(b => b.CadastradoPor).HasColumnName("PEDI_NM_CADASTRADO_POR").HasMaxLength(30).IsRequired();
        builder.Property(b => b.Estado).HasColumnName("PEDI_IN_ATIVO").HasConversion<int>().IsRequired();
        builder.Property(b => b.DataEHoraDeCadastro).HasColumnName("PEDI_DT_CADASTRO").IsRequired();

        builder.Ignore(b => b.CustoTotal);
        builder.Ignore(b => b.ContaComoVenda);
        builder.Ignore(b => b.TotalAPagar);
        builder.Ignore(b => b.PodeChamarEntregador);

        builder.HasMany(p => p.Itens)
            .WithOne()
            .HasForeignKey(i => i.PedidoId);

        builder.Navigation(p => p.Itens).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasQueryFilter(p => p.Estado == EntityStatus.Ativo); // exclusão lógica
    }
}

using EntreIrmas.Domain.Entities.UsuarioAggregate;
using EntreIrmas.Domain.Seedwork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntreIrmas.Infrastructure.Persistence.EntityConfigurations;

internal sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("USUARIO").HasKey(x => x.Id);
        builder.Property(b => b.Id).HasColumnName("USUA_SQ_USUARIO").ValueGeneratedOnAdd();
        builder.Property(b => b.Login).HasColumnName("USUA_NM_LOGIN").HasMaxLength(30).IsRequired();
        builder.Property(b => b.Nome).HasColumnName("USUA_NM_USUARIO").HasMaxLength(100).IsRequired();
        builder.Property(b => b.HashSenha).HasColumnName("USUA_DS_HASH_SENHA").HasMaxLength(200).IsRequired();
        builder.Property(b => b.UltimoAcesso).HasColumnName("USUA_DT_ULTIMO_ACESSO");
        builder.Property(b => b.CadastradoPor).HasColumnName("USUA_NM_CADASTRADO_POR").HasMaxLength(30).IsRequired();
        builder.Property(b => b.Estado).HasColumnName("USUA_IN_ATIVO").HasConversion<int>().IsRequired();
        builder.Property(b => b.DataEHoraDeCadastro).HasColumnName("USUA_DT_CADASTRO").IsRequired();

        builder.HasQueryFilter(p => p.Estado == EntityStatus.Ativo); // exclusão lógica
    }
}

using EntreIrmas.Domain.DTOs.Lancamento;
using EntreIrmas.Domain.DTOs.Usuario;
using EntreIrmas.Domain.Entities.LancamentoAggregate;
using EntreIrmas.Domain.Entities.UsuarioAggregate;
using EntreIrmas.Domain.Seedwork;
using Mapster;

namespace EntreIrmas.Application.Profiles;

public class AdministracaoProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // O hash da senha nunca sai da API: o DTO nem tem esse campo.
        config.NewConfig<Usuario, UsuarioDto>()
            .Map(dest => dest.Ativo, src => src.Estado == EntityStatus.Ativo);

        config.NewConfig<Lancamento, LancamentoDto>();
    }
}

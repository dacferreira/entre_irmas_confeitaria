using EntreIrmas.Domain.DTOs.Loja;
using EntreIrmas.Domain.Entities.LojaAggregate;
using Mapster;

namespace EntreIrmas.Application.Profiles;

public class LojaProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Loja, SituacaoLojaDto>()
            .Map(dest => dest.Mensagem, src => src.MensagemFechada);

        config.NewConfig<Loja, LojaAdminDto>()
            .Map(dest => dest.Mensagem, src => src.MensagemFechada)
            .Map(dest => dest.DataDaAlteracao, src => src.DataEHoraDaAlteracao);
    }
}

using EntreIrmas.Domain.DTOs.Pedido;
using EntreIrmas.Domain.Entities.PedidoAggregate;
using Mapster;

namespace EntreIrmas.Application.Profiles;

public class PedidoProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ItemPedido, ItemPedidoDto>();

        // O link do WhatsApp é preenchido pelo AppService.
        config.NewConfig<Pedido, PedidoCriadoDto>()
            .Ignore(dest => dest.LinkWhatsApp);
    }
}

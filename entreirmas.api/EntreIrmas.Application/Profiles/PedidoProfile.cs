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

        config.NewConfig<Pedido, UltimoPedidoDto>();

        // Público: só o primeiro nome do cliente.
        config.NewConfig<Pedido, AcompanhamentoPedidoDto>()
            .Map(dest => dest.PrimeiroNome, src => src.NomeCliente.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0])
            .Map(dest => dest.DataDoPedido, src => src.DataEHoraDeCadastro);

        // Área administrativa
        config.NewConfig<ItemPedido, ItemPedidoAdminDto>();

        config.NewConfig<Pedido, PedidoResumoDto>()
            .Map(dest => dest.QuantidadeItens, src => src.Itens.Count);

        config.NewConfig<Pedido, PedidoDetalheDto>()
            .Map(dest => dest.QuantidadeItens, src => src.Itens.Count);
    }
}

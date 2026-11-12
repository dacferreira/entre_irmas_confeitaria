using EntreIrmas.Domain.Entities.PedidoAggregate;

namespace EntreIrmas.Domain.Services;

/// <summary>Monta a mensagem que o cliente envia à confeitaria pelo WhatsApp.</summary>
public interface IMensagemPedidoService
{
    string MontarMensagem(Pedido pedido);
    string MontarLinkWhatsApp(Pedido pedido);
}

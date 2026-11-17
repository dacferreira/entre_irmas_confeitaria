using System.Globalization;
using System.Text;
using EntreIrmas.Domain.Entities.PedidoAggregate;
using EntreIrmas.Domain.EnviromentVariables;
using EntreIrmas.Domain.Services;

namespace EntreIrmas.Application.Services;

public class MensagemPedidoService : IMensagemPedidoService
{
    private static readonly CultureInfo CulturaPtBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly IWhatsAppEnvironmentVariables _variaveis;

    public MensagemPedidoService(IWhatsAppEnvironmentVariables variaveis)
    {
        _variaveis = variaveis;
    }

    public string MontarMensagem(Pedido pedido)
    {
        var mensagem = new StringBuilder()
            .AppendLine($"Olá! Fiz o pedido *{pedido.Codigo}* pelo site 🧁")
            .AppendLine();

        foreach (var item in pedido.Itens)
            mensagem.AppendLine($"• {item.Quantidade}x {item.NomeProduto} ({item.Unidade}) — {Moeda(item.ValorTotal)}");

        mensagem
            .AppendLine();

        if (pedido.TaxaEntrega is { } taxa)
            mensagem.AppendLine($"Entrega ({pedido.ParceiroEntrega?.Nome()}): {Moeda(taxa)}");
        else if (pedido.TipoEntrega == TipoEntrega.Entrega)
            mensagem.AppendLine("Entrega: taxa a combinar");

        mensagem
            .AppendLine($"*Total: {Moeda(pedido.TotalAPagar)}*")
            .AppendLine()
            .AppendLine($"Nome: {pedido.NomeCliente}")
            .AppendLine($"Data desejada: {pedido.DataDesejada.ToString("dd/MM/yyyy", CulturaPtBr)}")
            .AppendLine(pedido.TipoEntrega == TipoEntrega.Entrega
                ? $"Entrega em: {pedido.Endereco}"
                : "Vou retirar o pedido");

        if (!string.IsNullOrWhiteSpace(pedido.Observacao))
            mensagem.AppendLine($"Observação: {pedido.Observacao}");

        return mensagem.ToString().TrimEnd();
    }

    public string MontarLinkWhatsApp(Pedido pedido)
    {
        var numero = new string((_variaveis.WHATSAPP_NUMERO ?? string.Empty).Where(char.IsDigit).ToArray());
        return $"https://wa.me/{numero}?text={Uri.EscapeDataString(MontarMensagem(pedido))}";
    }

    private static string Moeda(decimal valor) => valor.ToString("C", CulturaPtBr);
}

using EntreIrmas.Application.Services;
using EntreIrmas.Domain.Entities.PedidoAggregate;
using EntreIrmas.Domain.EnviromentVariables;
using EntreIrmas.Test.Config;
using FluentAssertions;
using Moq;

namespace EntreIrmas.Test.Tests.Application;

public class MensagemPedidoServiceTests
{
    private readonly MensagemPedidoService _service;

    public MensagemPedidoServiceTests()
    {
        var variaveis = new Mock<IWhatsAppEnvironmentVariables>();
        variaveis.Setup(v => v.WHATSAPP_NUMERO).Returns("+55 (21) 99022-5050");
        _service = new MensagemPedidoService(variaveis.Object);
    }

    [Fact]
    public void MontarMensagem_ComEntrega_ListaItensTotalEEndereco()
    {
        var pedido = DadosDeTeste.Pedido(TipoEntrega.Entrega, "Rua das Flores, 10", "Sem lactose");
        pedido.AdicionarItem(DadosDeTeste.Produto(1, preco: 12.5m), 2);

        var mensagem = _service.MontarMensagem(pedido);

        mensagem.Should().Contain("*EI-ABC234*")
            .And.Contain("2x Brownie (unidade)")
            .And.Contain("*Total: R$")
            .And.Contain("25,00")
            .And.Contain("Data desejada: 20/12/2026")
            .And.Contain("Entrega em: Rua das Flores, 10")
            .And.Contain("Observação: Sem lactose");
    }

    [Fact]
    public void MontarMensagem_ComRetirada_InformaRetirada()
    {
        var pedido = DadosDeTeste.Pedido();
        pedido.AdicionarItem(DadosDeTeste.Produto(), 1);

        _service.MontarMensagem(pedido).Should().Contain("Vou retirar o pedido").And.NotContain("Observação");
    }

    [Fact]
    public void MontarLinkWhatsApp_UsaSoDigitosDoNumeroETextoCodificado()
    {
        var pedido = DadosDeTeste.Pedido();
        pedido.AdicionarItem(DadosDeTeste.Produto(), 1);

        var link = _service.MontarLinkWhatsApp(pedido);

        link.Should().StartWith("https://wa.me/5521990225050?text=");
        link.Should().NotContain(" ").And.Contain("EI-ABC234");
    }
}

using EntreIrmas.Domain.Entities.PedidoAggregate;
using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Test.Config;
using FluentAssertions;

namespace EntreIrmas.Test.Tests.Domain;

public class EntregaPedidoTests
{
    private static Pedido PedidoDeEntrega()
    {
        var pedido = DadosDeTeste.Pedido(TipoEntrega.Entrega, DadosDeTeste.Endereco());
        pedido.AdicionarItem(DadosDeTeste.Produto(preco: 20m), 2);
        return pedido;
    }

    [Fact]
    public void DefinirTaxaEntrega_SomaNoTotalAPagarSemMudarOValorDosItens()
    {
        var pedido = PedidoDeEntrega();

        pedido.DefinirTaxaEntrega(ParceiroEntrega.NoventaENove, 12.345m);

        pedido.ValorTotal.Should().Be(40m);
        pedido.TaxaEntrega.Should().Be(12.34m);
        pedido.TotalAPagar.Should().Be(52.34m);
        pedido.ParceiroEntrega.Should().Be(ParceiroEntrega.NoventaENove);
        pedido.PodeChamarEntregador.Should().BeTrue();
    }

    [Fact]
    public void SemTaxa_TotalAPagarEOValorDosItensENaoHaEntregadorParaChamar()
    {
        var pedido = PedidoDeEntrega();

        pedido.TotalAPagar.Should().Be(40m);
        pedido.PodeChamarEntregador.Should().BeFalse();
    }

    [Fact]
    public void DefinirTaxaEntrega_NaRetiradaOuNegativa_LancaDomainException()
    {
        var retirada = DadosDeTeste.Pedido();
        var definirNaRetirada = () => retirada.DefinirTaxaEntrega(ParceiroEntrega.Uber, 10m);
        var definirNegativa = () => PedidoDeEntrega().DefinirTaxaEntrega(ParceiroEntrega.Uber, -1m);

        definirNaRetirada.Should().Throw<DomainException>();
        definirNegativa.Should().Throw<DomainException>();
    }

    [Fact]
    public void RegistrarEntregadorChamado_GuardaIdERastreioELimpaAFalhaAnterior()
    {
        var pedido = PedidoDeEntrega();
        pedido.DefinirTaxaEntrega(ParceiroEntrega.Uber, 10m);
        pedido.RegistrarFalhaAoChamarEntregador("Uber fora do ar");
        pedido.SituacaoEntrega.Should().Be(SituacaoEntrega.Falhou);
        pedido.PodeChamarEntregador.Should().BeTrue();

        pedido.RegistrarEntregadorChamado(" del_123 ", "https://uber.test/rastreio");

        pedido.SituacaoEntrega.Should().Be(SituacaoEntrega.Solicitada);
        pedido.IdEntregaParceiro.Should().Be("del_123");
        pedido.RastreioEntrega.Should().Be("https://uber.test/rastreio");
        pedido.ErroEntrega.Should().BeNull();
        pedido.PodeChamarEntregador.Should().BeFalse();
    }

    [Fact]
    public void ChamarEntregador_SemParceiroOuJaChamado_LancaDomainException()
    {
        var semParceiro = PedidoDeEntrega();
        var chamado = PedidoDeEntrega();
        chamado.DefinirTaxaEntrega(ParceiroEntrega.Uber, 10m);
        chamado.RegistrarEntregadorChamado("del_1", null);

        ((Action)(() => semParceiro.RegistrarEntregadorChamado("del_2", null))).Should().Throw<DomainException>();
        ((Action)(() => chamado.RegistrarEntregadorChamado("del_2", null))).Should().Throw<DomainException>();
        ((Action)(() => chamado.RegistrarFalhaAoChamarEntregador("x"))).Should().Throw<DomainException>();
    }

    [Fact]
    public void RegistrarFalha_MotivoLongoFicaCom300Caracteres()
    {
        var pedido = PedidoDeEntrega();
        pedido.DefinirTaxaEntrega(ParceiroEntrega.Uber, 10m);

        pedido.RegistrarFalhaAoChamarEntregador(new string('x', 400));

        pedido.ErroEntrega.Should().HaveLength(300);
    }

    [Theory]
    [InlineData(ParceiroEntrega.Uber, "Uber")]
    [InlineData(ParceiroEntrega.NoventaENove, "99")]
    public void NomeDoParceiro_ParaOCliente(ParceiroEntrega parceiro, string nome) =>
        parceiro.Nome().Should().Be(nome);
}

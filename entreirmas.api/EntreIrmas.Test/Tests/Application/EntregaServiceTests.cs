using EntreIrmas.Application.AppServices;
using EntreIrmas.Application.Services;
using EntreIrmas.Domain.DTOs.Endereco;
using EntreIrmas.Domain.DTOs.Entrega;
using EntreIrmas.Domain.Entities.PedidoAggregate;
using EntreIrmas.Domain.EnviromentVariables;
using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Domain.Services;
using EntreIrmas.Test.Config;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EntreIrmas.Test.Tests.Application;

public class EntregaServiceTests
{
    private static Mock<IEntregaParceiroService> Parceiro(ParceiroEntrega parceiro, CotacaoParceiro? cotacao = null,
        Exception? falha = null)
    {
        var mock = new Mock<IEntregaParceiroService>();
        mock.Setup(p => p.Parceiro).Returns(parceiro);
        var cotar = mock.Setup(p => p.CotarAsync(It.IsAny<EnderecoEntrega>(), It.IsAny<CancellationToken>()));
        if (falha is not null)
            cotar.ThrowsAsync(falha);
        else
            cotar.ReturnsAsync(cotacao);
        return mock;
    }

    private static EntregaService Servico(params Mock<IEntregaParceiroService>[] parceiros) =>
        new(parceiros.Select(p => p.Object), NullLogger<EntregaService>.Instance);

    private static Pedido PedidoComParceiro(ParceiroEntrega parceiro)
    {
        var pedido = DadosDeTeste.Pedido(TipoEntrega.Entrega, DadosDeTeste.Endereco());
        pedido.AdicionarItem(DadosDeTeste.Produto(), 1);
        pedido.DefinirTaxaEntrega(parceiro, 10m);
        return pedido;
    }

    [Fact]
    public async Task CotarMaisBarata_EscolheOMenorValorEIgnoraQuemFalhaOuNaoAtende()
    {
        var servico = Servico(
            Parceiro(ParceiroEntrega.Uber, new CotacaoParceiro(ParceiroEntrega.Uber, 14.90m, 25)),
            Parceiro(ParceiroEntrega.NoventaENove, new CotacaoParceiro(ParceiroEntrega.NoventaENove, 12.50m, 30)));

        var cotacao = await servico.CotarMaisBarataAsync(DadosDeTeste.Endereco());

        cotacao.Should().Be(new CotacaoParceiro(ParceiroEntrega.NoventaENove, 12.50m, 30));

        var comFalha = Servico(
            Parceiro(ParceiroEntrega.Uber, new CotacaoParceiro(ParceiroEntrega.Uber, 14.90m, 25)),
            Parceiro(ParceiroEntrega.NoventaENove, falha: new GatewayException("99", "fora do ar")));
        (await comFalha.CotarMaisBarataAsync(DadosDeTeste.Endereco()))!.Parceiro.Should().Be(ParceiroEntrega.Uber);

        var ninguemAtende = Servico(Parceiro(ParceiroEntrega.Uber), Parceiro(ParceiroEntrega.NoventaENove));
        (await ninguemAtende.CotarMaisBarataAsync(DadosDeTeste.Endereco())).Should().BeNull();
        ninguemAtende.Disponivel.Should().BeTrue();
        Servico().Disponivel.Should().BeFalse();
    }

    [Fact]
    public async Task ChamarEntregador_UsaOParceiroDoPedidoERegistraOId()
    {
        var uber = Parceiro(ParceiroEntrega.Uber);
        var noventaENove = Parceiro(ParceiroEntrega.NoventaENove);
        var pedido = PedidoComParceiro(ParceiroEntrega.NoventaENove);
        noventaENove.Setup(p => p.SolicitarAsync(pedido, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EntregaSolicitada("99-abc", "https://99.test/r"));

        await Servico(uber, noventaENove).ChamarEntregadorAsync(pedido);

        pedido.SituacaoEntrega.Should().Be(SituacaoEntrega.Solicitada);
        pedido.IdEntregaParceiro.Should().Be("99-abc");
        uber.Verify(p => p.SolicitarAsync(It.IsAny<Pedido>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ChamarEntregador_FalhaDoParceiroOuParceiroDesligado_FicaRegistradaNoPedido()
    {
        var uber = Parceiro(ParceiroEntrega.Uber);
        uber.Setup(p => p.SolicitarAsync(It.IsAny<Pedido>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new GatewayException("Uber", "A Uber demorou para responder."));
        var pedidoUber = PedidoComParceiro(ParceiroEntrega.Uber);
        var pedido99 = PedidoComParceiro(ParceiroEntrega.NoventaENove);

        await Servico(uber).ChamarEntregadorAsync(pedidoUber);
        await Servico(uber).ChamarEntregadorAsync(pedido99);

        pedidoUber.SituacaoEntrega.Should().Be(SituacaoEntrega.Falhou);
        pedidoUber.ErroEntrega.Should().Be("A Uber demorou para responder.");
        pedido99.SituacaoEntrega.Should().Be(SituacaoEntrega.Falhou);
        pedido99.ErroEntrega.Should().Contain("A 99 não está configurada");
    }

    [Fact]
    public async Task ChamarEntregador_RetiradaOuJaChamado_NaoFazNada()
    {
        var uber = Parceiro(ParceiroEntrega.Uber);
        var chamado = PedidoComParceiro(ParceiroEntrega.Uber);
        chamado.RegistrarEntregadorChamado("del_1", null);

        await Servico(uber).ChamarEntregadorAsync(DadosDeTeste.Pedido());
        await Servico(uber).ChamarEntregadorAsync(chamado);

        uber.Verify(p => p.SolicitarAsync(It.IsAny<Pedido>(), It.IsAny<CancellationToken>()), Times.Never);
        chamado.IdEntregaParceiro.Should().Be("del_1");
    }
}

public class EntregaSimuladaServiceTests
{
    private static IEntregaEnvironmentVariables Loja()
    {
        var variaveis = new Mock<IEntregaEnvironmentVariables>();
        variaveis.Setup(v => v.LOJA_CEP).Returns("23580250");
        variaveis.Setup(v => v.LOJA_UF).Returns("RJ");
        return variaveis.Object;
    }

    private static EnderecoEntrega Destino(string cep, string uf = "RJ") =>
        new(cep, "Rua B", "1", null, "Bairro", "Cidade", uf);

    [Fact]
    public async Task Cotar_UberMaisBarataPertoE99MaisBarataLonge_SempreOMesmoValor()
    {
        var uber = new EntregaSimuladaService(ParceiroEntrega.Uber, Loja());
        var noventaENove = new EntregaSimuladaService(ParceiroEntrega.NoventaENove, Loja());

        var uberPerto = await uber.CotarAsync(Destino("23585000"));
        var noventaENovePerto = await noventaENove.CotarAsync(Destino("23585000"));
        var uberLonge = await uber.CotarAsync(Destino("21000000"));
        var noventaENoveLonge = await noventaENove.CotarAsync(Destino("21000000"));

        // 23580 x 23585: 2,25 km; 23580 x 21000: limite de 35 km.
        uberPerto.Should().Be(new CotacaoParceiro(ParceiroEntrega.Uber, 10.21m, 21));
        noventaENovePerto!.Valor.Should().BeGreaterThan(uberPerto!.Valor);
        uberLonge!.Valor.Should().BeGreaterThan(noventaENoveLonge!.Valor);
        (await uber.CotarAsync(Destino("23585000"))).Should().Be(uberPerto);
    }

    [Fact]
    public async Task Cotar_ForaDaUfDaLoja_NaoAtende() =>
        (await new EntregaSimuladaService(ParceiroEntrega.Uber, Loja()).CotarAsync(Destino("01001000", "SP")))
        .Should().BeNull();

    [Fact]
    public async Task Solicitar_DevolveUmIdDeSimulacao()
    {
        var entrega = await new EntregaSimuladaService(ParceiroEntrega.NoventaENove, Loja())
            .SolicitarAsync(DadosDeTeste.Pedido(codigo: "EI-XYZ234"));

        entrega.Should().Be(new EntregaSolicitada("simulada-noventaenove-EI-XYZ234", null));
    }
}

public class EntregaAppServiceTests
{
    private static readonly EnderecoDto Endereco = new()
    {
        Cep = "23585-000", Logradouro = "Rua B", Numero = "1", Bairro = "Paciência", Cidade = "Rio de Janeiro", Uf = "RJ"
    };

    [Fact]
    public async Task Cotar_SemParceiros_AvisaQueATaxaECombinada()
    {
        var entrega = new Mock<IEntregaService>();

        var cotacao = await new EntregaAppService(entrega.Object).CotarAsync(Endereco);

        cotacao.Disponivel.Should().BeFalse();
        cotacao.Mensagem.Should().Be(EntregaAppService.MensagemTaxaCombinada);
        entrega.Verify(e => e.CotarMaisBarataAsync(It.IsAny<EnderecoEntrega>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Cotar_SemCotacaoParaOEndereco_AvisaECombinaPeloWhatsApp()
    {
        var entrega = new Mock<IEntregaService>();
        entrega.Setup(e => e.Disponivel).Returns(true);

        var cotacao = await new EntregaAppService(entrega.Object).CotarAsync(Endereco);

        cotacao.Disponivel.Should().BeFalse();
        cotacao.Mensagem.Should().Be(EntregaAppService.MensagemSemCotacao);
    }

    [Fact]
    public async Task Cotar_DevolveAMaisBarataComOEnderecoNormalizado()
    {
        var entrega = new Mock<IEntregaService>();
        entrega.Setup(e => e.Disponivel).Returns(true);
        entrega.Setup(e => e.CotarMaisBarataAsync(It.Is<EnderecoEntrega>(d => d.Cep == "23585000" && d.Uf == "RJ"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CotacaoParceiro(ParceiroEntrega.Uber, 11.05m, 21));

        var cotacao = await new EntregaAppService(entrega.Object).CotarAsync(Endereco);

        cotacao.Should().BeEquivalentTo(new CotacaoEntregaDto
        {
            Disponivel = true, Parceiro = ParceiroEntrega.Uber, Valor = 11.05m, PrazoMinutos = 21
        });
    }
}

using EntreIrmas.Application.AppServices;
using EntreIrmas.Domain.DTOs.Pedido;
using EntreIrmas.Domain.Entities.LojaAggregate;
using EntreIrmas.Domain.Entities.PedidoAggregate;
using EntreIrmas.Domain.Entities.ProdutoAggregate;
using EntreIrmas.Domain.EnviromentVariables;
using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Domain.Repositories;
using EntreIrmas.Domain.Services;
using EntreIrmas.Test.Config;
using FluentAssertions;
using MapsterMapper;
using Moq;
using Moq.AutoMock;

namespace EntreIrmas.Test.Tests.Application;

public class PedidoAppServiceTests
{
    private static readonly DateTimeOffset Agora = new(2026, 11, 11, 10, 0, 0, TimeSpan.FromHours(-3));

    private readonly AutoMocker _mocker = new();
    private readonly PedidoAppService _appService;

    public PedidoAppServiceTests()
    {
        _mocker.Use<IMapper>(DadosDeTeste.CriarMapper());
        _mocker.Use<TimeProvider>(new TempoFixo(Agora));
        _mocker.GetMock<IPedidoEnvironmentVariables>().Setup(v => v.PEDIDO_ANTECEDENCIA_MINIMA_DIAS).Returns(1);
        _mocker.GetMock<IMensagemPedidoService>()
            .Setup(s => s.MontarLinkWhatsApp(It.IsAny<Pedido>()))
            .Returns("https://wa.me/5521990225050?text=ola");

        ComLoja(aberta: true);

        _appService = _mocker.CreateInstance<PedidoAppService>();
    }

    private void ComLoja(bool aberta)
    {
        var loja = new Loja("TESTE");
        if (!aberta)
            loja.Fechar("Voltamos amanhã", "TESTE");
        _mocker.GetMock<ILojaRepository>()
            .Setup(r => r.ObterAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(loja);
    }

    private void ComProdutos(params Produto[] produtos) =>
        _mocker.GetMock<IProdutoRepository>()
            .Setup(r => r.ObterPorIdsAsync(It.IsAny<IEnumerable<long>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(produtos);

    private static CriarPedidoDto Dto(DateOnly? data = null, params ItemCriarPedidoDto[] itens) => new()
    {
        NomeCliente = " Maria ",
        Telefone = "(21) 99999-8888",
        TipoEntrega = TipoEntrega.Retirada,
        DataDesejada = data ?? new DateOnly(2026, 11, 13),
        Itens = itens.Length > 0 ? itens.ToList() : [new ItemCriarPedidoDto { ProdutoId = 1, Quantidade = 2 }]
    };

    [Fact]
    public async Task CriarAsync_PedidoValido_GravaComPrecoDoCadastroELinkDoWhatsApp()
    {
        ComProdutos(DadosDeTeste.Produto(1, preco: 12m));
        Pedido? gravado = null;
        _mocker.GetMock<IPedidoRepository>()
            .Setup(r => r.AdicionarAsync(It.IsAny<Pedido>(), It.IsAny<CancellationToken>()))
            .Callback<Pedido, CancellationToken>((p, _) => gravado = p);

        var resultado = await _appService.CriarAsync(Dto());

        resultado.Codigo.Should().MatchRegex("^EI-[2-9A-HJKMNP-Z]{6}$");
        resultado.ValorTotal.Should().Be(24m);
        resultado.NomeCliente.Should().Be("Maria");
        resultado.Itens.Should().ContainSingle().Which.PrecoUnitario.Should().Be(12m);
        resultado.LinkWhatsApp.Should().StartWith("https://wa.me/");
        gravado!.Telefone.Should().Be("21999998888");
        gravado.CadastradoPor.Should().Be(PedidoAppService.CadastradoPor);
        _mocker.Verify<IPedidoRepository>(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_ComEncomendaSemAntecedencia_LancaDomainException()
    {
        ComProdutos(DadosDeTeste.Produto(1), DadosDeTeste.Produto(2, nome: "Bolo", encomenda: true));

        var criar = () => _appService.CriarAsync(Dto(new DateOnly(2026, 11, 11),
            new ItemCriarPedidoDto { ProdutoId = 1, Quantidade = 1 },
            new ItemCriarPedidoDto { ProdutoId = 2, Quantidade = 1 }));

        await criar.Should().ThrowAsync<DomainException>().WithMessage("*sob encomenda*a partir de 12/11/2026*");
        _mocker.Verify<IPedidoRepository>(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CriarAsync_PedidoNormal_EParaHojeQualquerQueSejaADataEnviada()
    {
        ComProdutos(DadosDeTeste.Produto(1));
        var gravados = new List<Pedido>();
        _mocker.GetMock<IPedidoRepository>()
            .Setup(r => r.AdicionarAsync(It.IsAny<Pedido>(), It.IsAny<CancellationToken>()))
            .Callback<Pedido, CancellationToken>((p, _) => gravados.Add(p));

        await _appService.CriarAsync(Dto(new DateOnly(2026, 11, 10)));
        await _appService.CriarAsync(Dto(new DateOnly(2026, 12, 25)));

        gravados.Select(p => p.DataDesejada).Should().AllBeEquivalentTo(new DateOnly(2026, 11, 11));
    }

    [Fact]
    public async Task CriarAsync_LojaFechada_RecusaPedidoNormal()
    {
        ComLoja(aberta: false);
        ComProdutos(DadosDeTeste.Produto(1));

        var criar = () => _appService.CriarAsync(Dto());

        await criar.Should().ThrowAsync<DomainException>().WithMessage(PedidoAppService.MensagemLojaFechada);
        _mocker.Verify<IPedidoRepository>(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CriarAsync_LojaFechada_AceitaEncomendaInclusiveComDocesDeProntaEntrega()
    {
        ComLoja(aberta: false);
        ComProdutos(DadosDeTeste.Produto(1), DadosDeTeste.Produto(2, nome: "Bolo", encomenda: true));

        var resultado = await _appService.CriarAsync(Dto(new DateOnly(2026, 11, 20),
            new ItemCriarPedidoDto { ProdutoId = 1, Quantidade = 1 },
            new ItemCriarPedidoDto { ProdutoId = 2, Quantidade = 1 }));

        resultado.DataDesejada.Should().Be(new DateOnly(2026, 11, 20));
    }

    [Fact]
    public async Task CriarManualAsync_LojaFechada_AceitaPedidoNormalComADataInformada()
    {
        ComLoja(aberta: false);
        ComProdutos(DadosDeTeste.Produto(1));

        var resultado = await _appService.CriarManualAsync(Dto(new DateOnly(2026, 11, 5)));

        resultado.DataDesejada.Should().Be(new DateOnly(2026, 11, 5));
    }

    [Fact]
    public async Task CriarAsync_SemAntecedenciaConfigurada_AceitaHojeERecusaPassado()
    {
        _mocker.GetMock<IPedidoEnvironmentVariables>().Setup(v => v.PEDIDO_ANTECEDENCIA_MINIMA_DIAS).Returns(0);
        ComProdutos(DadosDeTeste.Produto(1, encomenda: true));

        await _appService.CriarAsync(Dto(new DateOnly(2026, 11, 11)));
        var noPassado = () => _appService.CriarAsync(Dto(new DateOnly(2026, 11, 10)));

        await noPassado.Should().ThrowAsync<DomainException>().WithMessage("*passado*");
    }

    [Fact]
    public async Task CriarAsync_ProdutoIndisponivel_LancaDomainException()
    {
        ComProdutos(DadosDeTeste.Produto(1));

        var criar = () => _appService.CriarAsync(Dto(null,
            new ItemCriarPedidoDto { ProdutoId = 1, Quantidade = 1 },
            new ItemCriarPedidoDto { ProdutoId = 99, Quantidade = 1 }));

        await criar.Should().ThrowAsync<DomainException>().WithMessage("*não está mais disponível*");
    }

    [Fact]
    public async Task CriarAsync_CodigoSempreRepetido_LancaApplicationException()
    {
        ComProdutos(DadosDeTeste.Produto(1));
        _mocker.GetMock<IPedidoRepository>()
            .Setup(r => r.ExisteCodigoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var criar = () => _appService.CriarAsync(Dto());

        await criar.Should().ThrowAsync<EntreIrmas.Domain.Exceptions.ApplicationException>();
    }

    [Fact]
    public async Task CriarManualAsync_DataNoPassado_GravaConfirmadoComOLoginDeQuemLancou()
    {
        ComProdutos(DadosDeTeste.Produto(1, preco: 12m, custo: 5m));
        _mocker.GetMock<IAuthentication>().Setup(a => a.Login).Returns("suellen");
        Pedido? gravado = null;
        _mocker.GetMock<IPedidoRepository>()
            .Setup(r => r.AdicionarAsync(It.IsAny<Pedido>(), It.IsAny<CancellationToken>()))
            .Callback<Pedido, CancellationToken>((p, _) => gravado = p);

        var resultado = await _appService.CriarManualAsync(Dto(new DateOnly(2026, 11, 1)));

        resultado.Status.Should().Be(StatusPedido.Confirmado);
        resultado.CustoTotal.Should().Be(10m);
        resultado.QuantidadeItens.Should().Be(1);
        resultado.Itens.Should().ContainSingle().Which.CustoUnitario.Should().Be(5m);
        gravado!.CadastradoPor.Should().Be("suellen");
    }

    [Fact]
    public async Task AlterarStatusAsync_PedidoExistente_SalvaONovoStatus()
    {
        var pedido = DadosDeTeste.Pedido();
        pedido.AdicionarItem(DadosDeTeste.Produto(1), 1);
        _mocker.GetMock<IPedidoRepository>()
            .Setup(r => r.ObterPorIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pedido);

        var resultado = await _appService.AlterarStatusAsync(5, StatusPedido.Entregue);

        resultado.Status.Should().Be(StatusPedido.Entregue);
        _mocker.Verify<IPedidoRepository>(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExcluirEObter_PedidoInexistente_LancaObjectNotFound()
    {
        var excluir = () => _appService.ExcluirAsync(99);
        var obter = () => _appService.ObterAsync(99);

        await excluir.Should().ThrowAsync<ObjectNotFoundException>();
        await obter.Should().ThrowAsync<ObjectNotFoundException>();
    }

    [Fact]
    public async Task ExcluirAsync_PedidoExistente_Arquiva()
    {
        var pedido = DadosDeTeste.Pedido();
        _mocker.GetMock<IPedidoRepository>()
            .Setup(r => r.ObterPorIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pedido);

        await _appService.ExcluirAsync(5);

        pedido.Estado.Should().Be(EntreIrmas.Domain.Seedwork.EntityStatus.Arquivado);
    }

    [Fact]
    public async Task ListarAsync_MapeiaAPaginaEValidaOPeriodo()
    {
        var pedido = DadosDeTeste.Pedido();
        pedido.AdicionarItem(DadosDeTeste.Produto(1), 2);
        _mocker.GetMock<IPedidoRepository>()
            .Setup(r => r.ListarAsync(It.IsAny<FiltroPedidoDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Pedido> { pedido }, 31));

        var pagina = await _appService.ListarAsync(new FiltroPedidoDto { Pagina = 2, TamanhoPagina = 10 });
        var invalido = () => _appService.ListarAsync(new FiltroPedidoDto
        {
            Inicio = new DateOnly(2026, 11, 10),
            Fim = new DateOnly(2026, 11, 1)
        });

        pagina.Total.Should().Be(31);
        pagina.Pagina.Should().Be(2);
        pagina.Itens.Should().ContainSingle().Which.QuantidadeItens.Should().Be(1);
        await invalido.Should().ThrowAsync<DomainException>();
    }

    [Theory]
    [InlineData("EI-ABC234")]
    [InlineData("ei-abc234")]
    [InlineData(" EI ABC 234 ")]
    [InlineData("EIABC234")]
    [InlineData("abc234")]
    public void NormalizarCodigo_AceitaVariacoesDeDigitacao(string digitado)
    {
        PedidoAppService.NormalizarCodigo(digitado).Should().Be("EI-ABC234");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("EI-ABC23")]
    [InlineData("EI-ABC2345")]
    [InlineData("EI-ABC10O")] // 0, 1 e O não existem nos códigos
    [InlineData("XX-ABC234")]
    public void NormalizarCodigo_ForaDoFormato_LancaDomainException(string? digitado)
    {
        var normalizar = () => PedidoAppService.NormalizarCodigo(digitado);

        normalizar.Should().Throw<DomainException>().WithMessage(PedidoAppService.MensagemCodigoInvalido);
    }

    [Fact]
    public async Task AcompanharAsync_DevolveAEtapaSemDadosPessoais()
    {
        var pedido = new Pedido("EI-ABC234", "Maria da Silva", "21999998888", TipoEntrega.Entrega,
            DadosDeTeste.Endereco(), new DateOnly(2026, 12, 20), "Sem açúcar", "TESTE");
        pedido.AdicionarItem(DadosDeTeste.Produto(1, preco: 12m), 2);
        pedido.AlterarStatus(StatusPedido.EmPreparo);
        _mocker.GetMock<IPedidoRepository>()
            .Setup(r => r.ObterPorCodigoAsync("EI-ABC234", It.IsAny<CancellationToken>()))
            .ReturnsAsync(pedido);

        var resultado = await _appService.AcompanharAsync("ei-abc234");

        resultado.Codigo.Should().Be("EI-ABC234");
        resultado.PrimeiroNome.Should().Be("Maria");
        resultado.Status.Should().Be(StatusPedido.EmPreparo);
        resultado.TipoEntrega.Should().Be(TipoEntrega.Entrega);
        resultado.ValorTotal.Should().Be(24m);
        resultado.Itens.Should().ContainSingle().Which.Quantidade.Should().Be(2);
        typeof(AcompanhamentoPedidoDto).GetProperties().Select(p => p.Name)
            .Should().NotContain(["Telefone", "Endereco", "EnderecoEntrega", "Observacao", "NomeCliente"]);
    }

    [Fact]
    public async Task AcompanharAsync_CodigoInexistente_LancaObjectNotFound()
    {
        var acompanhar = () => _appService.AcompanharAsync("EI-ZZZZZZ");

        await acompanhar.Should().ThrowAsync<ObjectNotFoundException>().WithMessage(PedidoAppService.MensagemNaoEncontrado);
    }

    [Fact]
    public async Task ObterUltimoDoSiteAsync_BuscaOsPedidosDoSiteEDevolveNuloSemPedido()
    {
        var pedido = DadosDeTeste.Pedido();
        DadosDeTeste.DefinirId(pedido, 42);
        _mocker.GetMock<IPedidoRepository>()
            .Setup(r => r.ObterUltimoAsync(PedidoAppService.CadastradoPor, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pedido);

        var ultimo = await _appService.ObterUltimoDoSiteAsync();

        ultimo!.Id.Should().Be(42);
        ultimo.Codigo.Should().Be("EI-ABC234");
        ultimo.NomeCliente.Should().Be("Maria");

        _mocker.GetMock<IPedidoRepository>()
            .Setup(r => r.ObterUltimoAsync(PedidoAppService.CadastradoPor, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pedido?)null);
        (await _appService.ObterUltimoDoSiteAsync()).Should().BeNull();
    }

    [Fact]
    public async Task ListarNovosDoSiteAsync_BuscaOsPedidosDoSiteDepoisDoIdComLimiteEDevolveODetalhe()
    {
        var pedido = DadosDeTeste.Pedido();
        DadosDeTeste.DefinirId(pedido, 43);
        _mocker.GetMock<IPedidoRepository>()
            .Setup(r => r.ListarDepoisDeAsync(PedidoAppService.CadastradoPor, 42, PedidoAppService.MaximoDeNovosDoSite,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { pedido });

        var novos = await _appService.ListarNovosDoSiteAsync(42);

        novos.Should().ContainSingle();
        novos[0].Id.Should().Be(43);
        novos[0].Codigo.Should().Be("EI-ABC234");
    }
}

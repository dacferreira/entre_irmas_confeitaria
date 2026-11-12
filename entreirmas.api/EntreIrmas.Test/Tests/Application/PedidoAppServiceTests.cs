using EntreIrmas.Application.AppServices;
using EntreIrmas.Domain.DTOs.Pedido;
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

        _appService = _mocker.CreateInstance<PedidoAppService>();
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
    public async Task CriarAsync_DataSemAntecedencia_LancaDomainException()
    {
        ComProdutos(DadosDeTeste.Produto(1));

        var criar = () => _appService.CriarAsync(Dto(new DateOnly(2026, 11, 11)));

        await criar.Should().ThrowAsync<DomainException>().WithMessage("*a partir de 12/11/2026*");
        _mocker.Verify<IPedidoRepository>(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CriarAsync_SemAntecedenciaConfigurada_AceitaHojeERecusaPassado()
    {
        _mocker.GetMock<IPedidoEnvironmentVariables>().Setup(v => v.PEDIDO_ANTECEDENCIA_MINIMA_DIAS).Returns(0);
        ComProdutos(DadosDeTeste.Produto(1));

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

    private sealed class TempoFixo : TimeProvider
    {
        private readonly DateTimeOffset _agora;

        public TempoFixo(DateTimeOffset agora) => _agora = agora;

        public override DateTimeOffset GetUtcNow() => _agora.ToUniversalTime();

        public override TimeZoneInfo LocalTimeZone =>
            TimeZoneInfo.CreateCustomTimeZone("BRT", TimeSpan.FromHours(-3), "BRT", "BRT");
    }
}

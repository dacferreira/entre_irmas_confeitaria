using EntreIrmas.Application.AppServices;
using EntreIrmas.Domain.DTOs.Loja;
using EntreIrmas.Domain.Entities.LojaAggregate;
using EntreIrmas.Domain.Repositories;
using EntreIrmas.Domain.Services;
using EntreIrmas.Test.Config;
using FluentAssertions;
using MapsterMapper;
using Moq;
using Moq.AutoMock;

namespace EntreIrmas.Test.Tests.Application;

public class LojaAppServiceTests
{
    private readonly AutoMocker _mocker = new();
    private readonly LojaAppService _appService;

    public LojaAppServiceTests()
    {
        _mocker.Use<IMapper>(DadosDeTeste.CriarMapper());
        _mocker.GetMock<IAuthentication>().Setup(a => a.Login).Returns("suellen");
        _appService = _mocker.CreateInstance<LojaAppService>();
    }

    private void ComLoja(Loja? loja) =>
        _mocker.GetMock<ILojaRepository>()
            .Setup(r => r.ObterAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(loja);

    [Fact]
    public async Task ObterSituacaoAsync_SemRegistroNoBanco_ConsideraAberta()
    {
        ComLoja(null);

        var situacao = await _appService.ObterSituacaoAsync();

        situacao.Aberta.Should().BeTrue();
        situacao.Mensagem.Should().BeNull();
    }

    [Fact]
    public async Task ObterSituacaoAsync_Fechada_DevolveAMensagem()
    {
        var loja = new Loja("TESTE");
        loja.Fechar("Voltamos amanhã", "tatiane");
        ComLoja(loja);

        var situacao = await _appService.ObterSituacaoAsync();
        var admin = await _appService.ObterAsync();

        situacao.Aberta.Should().BeFalse();
        situacao.Mensagem.Should().Be("Voltamos amanhã");
        admin.AlteradoPor.Should().Be("tatiane");
    }

    [Fact]
    public async Task AlterarAsync_FechaEAbreComOUsuarioLogado()
    {
        var loja = new Loja("TESTE");
        ComLoja(loja);

        var fechada = await _appService.AlterarAsync(new AlterarLojaDto { Aberta = false, Mensagem = "Feriado" });
        fechada.Aberta.Should().BeFalse();
        fechada.Mensagem.Should().Be("Feriado");
        fechada.AlteradoPor.Should().Be("suellen");

        var aberta = await _appService.AlterarAsync(new AlterarLojaDto { Aberta = true, Mensagem = "ignorada" });
        aberta.Aberta.Should().BeTrue();
        aberta.Mensagem.Should().BeNull();

        _mocker.Verify<ILojaRepository>(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        _mocker.Verify<ILojaRepository>(r => r.AdicionarAsync(It.IsAny<Loja>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AlterarAsync_SemRegistroNoBanco_CriaOUnico()
    {
        ComLoja(null);

        var resultado = await _appService.AlterarAsync(new AlterarLojaDto { Aberta = false });

        resultado.Aberta.Should().BeFalse();
        _mocker.Verify<ILojaRepository>(r => r.AdicionarAsync(
            It.Is<Loja>(l => l.Id == Loja.IdUnico && !l.Aberta), It.IsAny<CancellationToken>()), Times.Once);
    }
}

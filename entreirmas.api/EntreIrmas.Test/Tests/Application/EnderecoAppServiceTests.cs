using EntreIrmas.Application.AppServices;
using EntreIrmas.Domain.DTOs.Endereco;
using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Domain.Services;
using FluentAssertions;
using Moq;
using Moq.AutoMock;

namespace EntreIrmas.Test.Tests.Application;

public class EnderecoAppServiceTests
{
    private readonly AutoMocker _mocker = new();
    private readonly EnderecoAppService _appService;

    public EnderecoAppServiceTests()
    {
        _appService = _mocker.CreateInstance<EnderecoAppService>();
    }

    [Fact]
    public async Task ConsultarCep_ComMascara_ConsultaSoOsDigitos()
    {
        var esperado = new EnderecoCepDto { Cep = "01001000", Logradouro = "Praça da Sé" };
        _mocker.GetMock<IViaCepService>()
            .Setup(s => s.ConsultarAsync("01001000", It.IsAny<CancellationToken>()))
            .ReturnsAsync(esperado);

        (await _appService.ConsultarCepAsync(" 01001-000 ")).Should().BeSameAs(esperado);
    }

    [Fact]
    public async Task ConsultarCep_Incompleto_LancaDomainExceptionSemChamarOViaCep()
    {
        var consultar = () => _appService.ConsultarCepAsync("0100-100");

        await consultar.Should().ThrowAsync<DomainException>().WithMessage("*8 números*");
        _mocker.Verify<IViaCepService>(s => s.ConsultarAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConsultarCep_Inexistente_LancaObjectNotFound()
    {
        var consultar = () => _appService.ConsultarCepAsync("99999999");

        await consultar.Should().ThrowAsync<ObjectNotFoundException>().WithMessage(EnderecoAppService.MensagemNaoEncontrado);
    }

    [Fact]
    public async Task ConsultarCep_ViaCepFora_Lanca503ComMensagemParaPreencherAMao()
    {
        _mocker.GetMock<IViaCepService>()
            .Setup(s => s.ConsultarAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new GatewayException("ViaCEP", "fora"));

        var consultar = () => _appService.ConsultarCepAsync("01001000");

        var erro = (await consultar.Should().ThrowAsync<ApiException>()).Which;
        erro.StatusCode.Should().Be(503);
        erro.Message.Should().Be(EnderecoAppService.MensagemIndisponivel);
    }
}

using EntreIrmas.Application.AppServices;
using EntreIrmas.Application.Services;
using EntreIrmas.Domain.DTOs.Autenticacao;
using EntreIrmas.Domain.Entities.UsuarioAggregate;
using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Domain.Repositories;
using EntreIrmas.Domain.Services;
using EntreIrmas.Test.Config;
using FluentAssertions;
using Moq;
using Moq.AutoMock;

namespace EntreIrmas.Test.Tests.Application;

public class AutenticacaoAppServiceTests
{
    private static readonly DateTimeOffset Agora = new(2026, 11, 13, 9, 0, 0, TimeSpan.FromHours(-3));

    private readonly AutoMocker _mocker = new();
    private readonly AutenticacaoAppService _appService;

    public AutenticacaoAppServiceTests()
    {
        _mocker.Use<ISenhaService>(DadosDeTeste.CriarSenhaService());
        _mocker.Use<TimeProvider>(new TempoFixo(Agora));
        _mocker.GetMock<ITokenService>()
            .Setup(t => t.GerarSessao(It.IsAny<Usuario>()))
            .Returns<Usuario>(u => new SessaoDto { Token = "token", Login = u.Login, Nome = u.Nome });

        _appService = _mocker.CreateInstance<AutenticacaoAppService>();
    }

    private void ComUsuario(Usuario? usuario) =>
        _mocker.GetMock<IUsuarioRepository>()
            .Setup(r => r.ObterPorLoginAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

    [Fact]
    public async Task LoginAsync_SenhaCorreta_RegistraAcessoEDevolveSessao()
    {
        var usuario = DadosDeTeste.Usuario();
        ComUsuario(usuario);

        var sessao = await _appService.LoginAsync(new LoginDto { Login = " ADMIN.TESTE ", Senha = DadosDeTeste.SenhaDeTeste });

        sessao.Token.Should().Be("token");
        usuario.UltimoAcesso.Should().Be(Agora.DateTime);
        _mocker.Verify<IUsuarioRepository>(r => r.ObterPorLoginAsync("admin.teste", It.IsAny<CancellationToken>()));
        _mocker.Verify<IUsuarioRepository>(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_SenhaErrada_Lanca401SemRegistrarAcesso()
    {
        var usuario = DadosDeTeste.Usuario();
        ComUsuario(usuario);

        var entrar = () => _appService.LoginAsync(new LoginDto { Login = "admin.teste", Senha = "errada" });

        (await entrar.Should().ThrowAsync<ApiException>()).Which.StatusCode.Should().Be(401);
        usuario.UltimoAcesso.Should().BeNull();
        _mocker.Verify<ITokenService>(t => t.GerarSessao(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_UsuarioInexistente_LancaAMesmaMensagem()
    {
        ComUsuario(null);

        var entrar = () => _appService.LoginAsync(new LoginDto { Login = "ninguem", Senha = "qualquer" });

        await entrar.Should().ThrowAsync<ApiException>().WithMessage(AutenticacaoAppService.MensagemDeFalha);
    }
}

public class SenhaServiceTests
{
    private readonly SenhaService _senhas = DadosDeTeste.CriarSenhaService();

    [Fact]
    public void GerarHash_UsaSalAleatorioEConfereASenha()
    {
        var hash1 = _senhas.GerarHash("doce-de-leite");
        var hash2 = _senhas.GerarHash("doce-de-leite");

        hash1.Should().StartWith("PBKDF2$1000$").And.NotBe(hash2);
        _senhas.Conferir("doce-de-leite", hash1).Should().BeTrue();
        _senhas.Conferir("doce-de-leitE", hash1).Should().BeFalse();
    }

    [Fact]
    public void Conferir_UsaAsIteracoesGravadasNoHash()
    {
        var hashAntigo = new SenhaService(500).GerarHash("brigadeiro");

        new SenhaService().Conferir("brigadeiro", hashAntigo).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("texto-qualquer")]
    [InlineData("PBKDF2$abc$sal$hash")]
    [InlineData("PBKDF2$1000$@@@$###")]
    public void Conferir_HashInvalido_RetornaFalse(string hash)
    {
        _senhas.Conferir("senha", hash).Should().BeFalse();
    }

    [Fact]
    public void GerarHash_SenhaVazia_LancaArgumentException()
    {
        var gerar = () => _senhas.GerarHash("");

        gerar.Should().Throw<ArgumentException>();
    }
}

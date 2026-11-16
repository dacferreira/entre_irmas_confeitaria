using System.Net;
using System.Text;
using EntreIrmas.Domain.EnviromentVariables;
using EntreIrmas.Domain.Exceptions;
using EntreIrmas.ViaCepService;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EntreIrmas.Test.Tests.Gateways;

public class ViaCepServiceTests
{
    // Exemplo real da documentação do ViaCEP (https://viacep.com.br/ws/01001000/json/).
    private const string RespostaPracaDaSe = """
        {
          "cep": "01001-000", "logradouro": "Praça da Sé", "complemento": "lado ímpar", "unidade": "",
          "bairro": "Sé", "localidade": "São Paulo", "uf": "SP", "estado": "São Paulo", "regiao": "Sudeste",
          "ibge": "3550308", "gia": "1004", "ddd": "11", "siafi": "7107"
        }
        """;

    private readonly ManipuladorFalso _manipulador = new();
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());
    private readonly ViaCepService.ViaCepService _service;

    public ViaCepServiceTests()
    {
        var variaveis = new Mock<IViaCepEnvironmentVariables>();
        variaveis.Setup(v => v.VIACEP_CACHE_HORAS).Returns(24);
        _service = new ViaCepService.ViaCepService(FabricaCom(_manipulador), _cache, variaveis.Object,
            NullLogger<ViaCepService.ViaCepService>.Instance);
    }

    internal static IHttpClientFactory FabricaCom(HttpMessageHandler manipulador)
    {
        var fabrica = new Mock<IHttpClientFactory>();
        fabrica.Setup(f => f.CreateClient(ViaCepService.ViaCepService.NomeDoCliente))
            .Returns(() => new HttpClient(manipulador, disposeHandler: false) { BaseAddress = new Uri("https://viacep.test/ws/") });
        return fabrica.Object;
    }

    [Fact]
    public async Task Consultar_CepExistente_MapeiaOEndereco()
    {
        _manipulador.Responder(HttpStatusCode.OK, RespostaPracaDaSe);

        var endereco = await _service.ConsultarAsync("01001000");

        endereco.Should().BeEquivalentTo(new
        {
            Cep = "01001000",
            Logradouro = "Praça da Sé",
            Complemento = "lado ímpar",
            Bairro = "Sé",
            Cidade = "São Paulo",
            Uf = "SP"
        });
        _manipulador.UltimaUrl.Should().Be("https://viacep.test/ws/01001000/json/");
    }

    [Theory]
    [InlineData("""{ "erro": "true" }""")]
    [InlineData("""{ "erro": true }""")]
    public async Task Consultar_CepInexistente_RetornaNulo(string resposta)
    {
        _manipulador.Responder(HttpStatusCode.OK, resposta);

        (await _service.ConsultarAsync("99999999")).Should().BeNull();
    }

    [Fact]
    public async Task Consultar_FormatoRecusadoPeloViaCep_RetornaNulo()
    {
        _manipulador.Responder(HttpStatusCode.BadRequest, "<h1>Bad Request</h1>");

        (await _service.ConsultarAsync("00000000")).Should().BeNull();
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError, "erro")]
    [InlineData(HttpStatusCode.OK, "<html>não é json</html>")]
    public async Task Consultar_ServicoComProblema_LancaGatewayException(HttpStatusCode status, string corpo)
    {
        _manipulador.Responder(status, corpo);

        var consultar = () => _service.ConsultarAsync("01001000");

        (await consultar.Should().ThrowAsync<GatewayException>()).Which.Sistema.Should().Be("ViaCEP");
    }

    [Fact]
    public async Task Consultar_SemResposta_LancaGatewayException()
    {
        _manipulador.Falhar(new HttpRequestException("sem rede"));
        var semRede = () => _service.ConsultarAsync("01001000");
        await semRede.Should().ThrowAsync<GatewayException>();

        // O timeout do HttpClient chega como cancelamento sem que quem chamou tenha cancelado.
        _manipulador.Falhar(new TaskCanceledException("timeout"));
        var timeout = () => _service.ConsultarAsync("01001000");
        await timeout.Should().ThrowAsync<GatewayException>().WithMessage("*demorou*");
    }

    [Fact]
    public async Task Consultar_CanceladoPorQuemChamou_RepassaOCancelamento()
    {
        _manipulador.Falhar(new TaskCanceledException("cancelado"));
        using var cancelamento = new CancellationTokenSource();
        await cancelamento.CancelAsync();

        var consultar = () => _service.ConsultarAsync("01001000", cancelamento.Token);

        await consultar.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Consultar_GuardaEncontradosENaoEncontradosMasNaoAsFalhas()
    {
        _manipulador.Responder(HttpStatusCode.OK, RespostaPracaDaSe);
        await _service.ConsultarAsync("01001000");
        await _service.ConsultarAsync("01001000");
        _manipulador.Chamadas.Should().Be(1);

        _manipulador.Responder(HttpStatusCode.OK, """{ "erro": "true" }""");
        await _service.ConsultarAsync("99999999");
        await _service.ConsultarAsync("99999999");
        _manipulador.Chamadas.Should().Be(2);

        _manipulador.Responder(HttpStatusCode.InternalServerError, "erro");
        var falha = () => _service.ConsultarAsync("22222222");
        await falha.Should().ThrowAsync<GatewayException>();
        await falha.Should().ThrowAsync<GatewayException>();
        _manipulador.Chamadas.Should().Be(4);
    }

    [Theory]
    [InlineData("0100100")]
    [InlineData("01001-000")]
    public async Task Consultar_CepForaDoFormato_LancaArgumentException(string cep)
    {
        var consultar = () => _service.ConsultarAsync(cep);

        await consultar.Should().ThrowAsync<ArgumentException>();
        _manipulador.Chamadas.Should().Be(0);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, HealthStatus.Healthy)]
    [InlineData(HttpStatusCode.ServiceUnavailable, HealthStatus.Degraded)]
    public async Task HealthCheck_RespondeConformeOViaCep(HttpStatusCode status, HealthStatus esperado)
    {
        _manipulador.Responder(status, RespostaPracaDaSe);

        var resultado = await new ViaCepHealthCheck(FabricaCom(_manipulador)).CheckHealthAsync(Contexto());

        resultado.Status.Should().Be(esperado);
    }

    [Fact]
    public async Task HealthCheck_SemRede_FicaDegradado()
    {
        _manipulador.Falhar(new HttpRequestException("sem rede"));

        var resultado = await new ViaCepHealthCheck(FabricaCom(_manipulador)).CheckHealthAsync(Contexto());

        resultado.Status.Should().Be(HealthStatus.Degraded);
    }

    private static HealthCheckContext Contexto() => new()
    {
        Registration = new HealthCheckRegistration("viacep", Mock.Of<IHealthCheck>(), HealthStatus.Degraded, null)
    };

    /// <summary>Responde às chamadas HTTP sem sair da máquina.</summary>
    internal sealed class ManipuladorFalso : HttpMessageHandler
    {
        private HttpStatusCode _status = HttpStatusCode.OK;
        private string _corpo = "{}";
        private Exception? _falha;

        public int Chamadas { get; private set; }
        public string? UltimaUrl { get; private set; }

        public void Responder(HttpStatusCode status, string corpo)
        {
            _status = status;
            _corpo = corpo;
            _falha = null;
        }

        public void Falhar(Exception falha) => _falha = falha;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Chamadas++;
            UltimaUrl = request.RequestUri?.ToString();
            if (_falha is not null)
                throw _falha;

            return Task.FromResult(new HttpResponseMessage(_status)
            {
                Content = new StringContent(_corpo, Encoding.UTF8, "application/json")
            });
        }
    }
}

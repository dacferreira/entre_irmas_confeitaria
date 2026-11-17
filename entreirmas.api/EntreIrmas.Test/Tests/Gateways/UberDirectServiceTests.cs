using System.Net;
using System.Text;
using System.Text.Json;
using EntreIrmas.Domain.Entities.PedidoAggregate;
using EntreIrmas.Domain.EnviromentVariables;
using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Test.Config;
using EntreIrmas.UberDirectService;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EntreIrmas.Test.Tests.Gateways;

public class UberDirectServiceTests
{
    private const string Token = """{ "access_token": "token-123", "token_type": "Bearer", "expires_in": 2592000, "scope": "eats.deliveries" }""";

    private readonly ServidorUberFalso _servidor = new();
    private readonly Mock<IUberEnvironmentVariables> _uber = new();
    private readonly UberDirectService.UberDirectService _service;

    public UberDirectServiceTests()
    {
        _uber.Setup(v => v.UBER_CLIENT_ID).Returns("cliente");
        _uber.Setup(v => v.UBER_CLIENT_SECRET).Returns("segredo");
        _uber.Setup(v => v.UBER_CUSTOMER_ID).Returns("cust-1");
        _uber.Setup(v => v.UBER_AUTH_URL).Returns("https://auth.uber.test/oauth/v2/token");

        var loja = new Mock<IEntregaEnvironmentVariables>();
        loja.Setup(v => v.LOJA_NOME).Returns("Entre Irmãs Confeitaria");
        loja.Setup(v => v.LOJA_LOGRADOURO).Returns("Rua A, 41");
        loja.Setup(v => v.LOJA_BAIRRO).Returns("Paciência");
        loja.Setup(v => v.LOJA_CIDADE).Returns("Rio de Janeiro");
        loja.Setup(v => v.LOJA_UF).Returns("RJ");
        loja.Setup(v => v.LOJA_CEP).Returns("23580250");
        loja.Setup(v => v.LOJA_LATITUDE).Returns(-22.9161966);
        loja.Setup(v => v.LOJA_LONGITUDE).Returns(-43.6447006);

        var whatsApp = new Mock<IWhatsAppEnvironmentVariables>();
        whatsApp.Setup(v => v.WHATSAPP_NUMERO).Returns("5521990225050");

        var fabrica = new Mock<IHttpClientFactory>();
        fabrica.Setup(f => f.CreateClient(UberDirectService.UberDirectService.NomeDoCliente))
            .Returns(() => new HttpClient(_servidor, disposeHandler: false) { BaseAddress = new Uri("https://api.uber.test/v1/") });
        fabrica.Setup(f => f.CreateClient(UberDirectService.UberDirectService.NomeDoClienteDeAutenticacao))
            .Returns(() => new HttpClient(_servidor, disposeHandler: false));

        _service = new UberDirectService.UberDirectService(fabrica.Object, new MemoryCache(new MemoryCacheOptions()),
            _uber.Object, loja.Object, whatsApp.Object, NullLogger<UberDirectService.UberDirectService>.Instance);
    }

    private static Pedido PedidoDeEntrega()
    {
        var pedido = DadosDeTeste.Pedido(TipoEntrega.Entrega, DadosDeTeste.Endereco());
        pedido.AdicionarItem(DadosDeTeste.Produto(nome: "Brownie"), 3);
        pedido.DefinirTaxaEntrega(ParceiroEntrega.Uber, 12m);
        return pedido;
    }

    [Fact]
    public async Task Cotar_PedeOTokenUmaVezEConverteCentavosEmReais()
    {
        _servidor.Responder("oauth/v2/token", HttpStatusCode.OK, Token);
        _servidor.Responder("delivery_quotes", HttpStatusCode.OK,
            """{ "kind": "delivery_quote", "id": "dqt_1", "fee": 1290, "currency_type": "BRL", "duration": 34 }""");

        var cotacao = await _service.CotarAsync(DadosDeTeste.Endereco());
        await _service.CotarAsync(DadosDeTeste.Endereco());

        cotacao.Should().Be(new EntreIrmas.Domain.DTOs.Entrega.CotacaoParceiro(ParceiroEntrega.Uber, 12.90m, 34));
        _servidor.Chamadas("oauth/v2/token").Should().Be(1);

        var autenticacao = _servidor.Corpos("oauth/v2/token")[0];
        autenticacao.Should().Contain("grant_type=client_credentials").And.Contain("scope=eats.deliveries")
            .And.Contain("client_id=cliente");

        var requisicao = _servidor.Requisicoes("delivery_quotes")[0];
        requisicao.Url.Should().Be("https://api.uber.test/v1/customers/cust-1/delivery_quotes");
        requisicao.Autorizacao.Should().Be("Bearer token-123");

        using var corpo = JsonDocument.Parse(requisicao.Corpo);
        corpo.RootElement.GetProperty("pickup_latitude").GetDouble().Should().Be(-22.9161966);
        corpo.RootElement.GetProperty("pickup_phone_number").GetString().Should().Be("+5521990225050");
        using var destino = JsonDocument.Parse(corpo.RootElement.GetProperty("dropoff_address").GetString()!);
        destino.RootElement.GetProperty("street_address")[0].GetString().Should().Be("Rua das Flores, 10");
        destino.RootElement.GetProperty("street_address")[1].GetString().Should().Be("Apto 2 - Centro");
        destino.RootElement.GetProperty("zip_code").GetString().Should().Be("20000-000");
        destino.RootElement.GetProperty("state").GetString().Should().Be("RJ");
        destino.RootElement.GetProperty("country").GetString().Should().Be("BR");
    }

    [Fact]
    public async Task Cotar_EnderecoForaDaArea_RetornaNulo()
    {
        _servidor.Responder("oauth/v2/token", HttpStatusCode.OK, Token);
        _servidor.Responder("delivery_quotes", HttpStatusCode.BadRequest,
            """{ "code": "address_undeliverable", "message": "The specified location is not in a deliverable area." }""");

        (await _service.CotarAsync(DadosDeTeste.Endereco())).Should().BeNull();
    }

    [Fact]
    public async Task Cotar_ErroDaUberOuCredencialRecusada_LancaGatewayException()
    {
        _servidor.Responder("oauth/v2/token", HttpStatusCode.OK, Token);
        _servidor.Responder("delivery_quotes", HttpStatusCode.InternalServerError, """{ "code": "internal_server_error" }""");

        var cotar = () => _service.CotarAsync(DadosDeTeste.Endereco());
        await cotar.Should().ThrowAsync<GatewayException>().WithMessage("*calcular a entrega (500)*");

        _servidor.Responder("oauth/v2/token", HttpStatusCode.Unauthorized, """{ "error": "invalid_client" }""");
        var service = NovoServicoSemToken();
        var cotarSemCredencial = () => service.CotarAsync(DadosDeTeste.Endereco());
        await cotarSemCredencial.Should().ThrowAsync<GatewayException>().WithMessage("*credenciais*");
    }

    [Fact]
    public async Task Cotar_TokenRecusado_DescartaParaAProximaChamadaPedirOutro()
    {
        _servidor.Responder("oauth/v2/token", HttpStatusCode.OK, Token);
        _servidor.Responder("delivery_quotes", HttpStatusCode.Unauthorized, """{ "code": "unauthorized" }""");

        var cotar = () => _service.CotarAsync(DadosDeTeste.Endereco());
        await cotar.Should().ThrowAsync<GatewayException>();
        await cotar.Should().ThrowAsync<GatewayException>();

        _servidor.Chamadas("oauth/v2/token").Should().Be(2);
    }

    [Fact]
    public async Task Cotar_RedeFora_LancaGatewayException()
    {
        _servidor.Responder("oauth/v2/token", HttpStatusCode.OK, Token);
        _servidor.Falhar("delivery_quotes", new HttpRequestException("sem rede"));

        var cotar = () => _service.CotarAsync(DadosDeTeste.Endereco());

        await cotar.Should().ThrowAsync<GatewayException>().WithMessage("Não foi possível calcular a entrega na Uber.");
    }

    [Fact]
    public async Task Solicitar_MandaLojaClienteItensEModoTesteEDevolveIdERastreio()
    {
        _uber.Setup(v => v.UBER_MODO_TESTE).Returns(true);
        _servidor.Responder("oauth/v2/token", HttpStatusCode.OK, Token);
        _servidor.Responder("deliveries", HttpStatusCode.OK,
            """{ "id": "del_abc", "status": "pending", "tracking_url": "https://www.ubereats.com/orders/abc" }""");

        var entrega = await _service.SolicitarAsync(PedidoDeEntrega());

        entrega.IdEntrega.Should().Be("del_abc");
        entrega.Rastreio.Should().Be("https://www.ubereats.com/orders/abc");

        var requisicao = _servidor.Requisicoes("deliveries")[0];
        requisicao.Url.Should().Be("https://api.uber.test/v1/customers/cust-1/deliveries");
        using var corpo = JsonDocument.Parse(requisicao.Corpo);
        var raiz = corpo.RootElement;
        raiz.GetProperty("pickup_name").GetString().Should().Be("Entre Irmãs Confeitaria");
        raiz.GetProperty("dropoff_name").GetString().Should().Be("Maria");
        raiz.GetProperty("dropoff_phone_number").GetString().Should().Be("+5521999998888");
        raiz.GetProperty("dropoff_notes").GetString().Should().Be("Apto 2");
        raiz.GetProperty("manifest_reference").GetString().Should().Be("EI-ABC234");
        raiz.GetProperty("manifest_items")[0].GetProperty("name").GetString().Should().Be("Brownie");
        raiz.GetProperty("manifest_items")[0].GetProperty("quantity").GetInt32().Should().Be(3);
        raiz.GetProperty("test_specifications").GetProperty("robo_courier_specification").GetProperty("mode")
            .GetString().Should().Be("auto");
        using var loja = JsonDocument.Parse(raiz.GetProperty("pickup_address").GetString()!);
        loja.RootElement.GetProperty("street_address")[0].GetString().Should().Be("Rua A, 41");
        loja.RootElement.GetProperty("zip_code").GetString().Should().Be("23580-250");
    }

    [Fact]
    public async Task Solicitar_ForaDoModoTeste_NaoMandaEspecificacaoDeTeste()
    {
        _servidor.Responder("oauth/v2/token", HttpStatusCode.OK, Token);
        _servidor.Responder("deliveries", HttpStatusCode.OK, """{ "id": "del_abc" }""");

        var entrega = await _service.SolicitarAsync(PedidoDeEntrega());

        entrega.Rastreio.Should().BeNull();
        _servidor.Requisicoes("deliveries")[0].Corpo.Should().NotContain("test_specifications");
    }

    [Fact]
    public async Task Solicitar_SemIdOuRecusado_LancaGatewayException()
    {
        _servidor.Responder("oauth/v2/token", HttpStatusCode.OK, Token);
        _servidor.Responder("deliveries", HttpStatusCode.OK, """{ "status": "pending" }""");
        var semId = () => _service.SolicitarAsync(PedidoDeEntrega());
        await semId.Should().ThrowAsync<GatewayException>().WithMessage("*id da entrega*");

        _servidor.Responder("deliveries", HttpStatusCode.BadRequest, """{ "code": "address_undeliverable" }""");
        var recusado = () => _service.SolicitarAsync(PedidoDeEntrega());
        await recusado.Should().ThrowAsync<GatewayException>().WithMessage("*chamar o entregador (400)*");
    }

    [Theory]
    [InlineData("21999998888", "+5521999998888")]
    [InlineData("5521990225050", "+5521990225050")]
    [InlineData("(21) 3333-4444", "+552133334444")]
    public void TelefoneInternacional_FormatoE164(string telefone, string esperado) =>
        UberDirectService.UberDirectService.TelefoneInternacional(telefone).Should().Be(esperado);

    [Fact]
    public void UberConfigurada_SoComAsTresCredenciais()
    {
        UberDirectApiServiceExtension.UberConfigurada(_uber.Object).Should().BeTrue();

        _uber.Setup(v => v.UBER_CUSTOMER_ID).Returns("");
        UberDirectApiServiceExtension.UberConfigurada(_uber.Object).Should().BeFalse();
    }

    private UberDirectService.UberDirectService NovoServicoSemToken()
    {
        var fabrica = new Mock<IHttpClientFactory>();
        fabrica.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(_servidor, disposeHandler: false) { BaseAddress = new Uri("https://api.uber.test/v1/") });
        return new UberDirectService.UberDirectService(fabrica.Object, new MemoryCache(new MemoryCacheOptions()),
            _uber.Object, new Mock<IEntregaEnvironmentVariables>().Object, new Mock<IWhatsAppEnvironmentVariables>().Object,
            NullLogger<UberDirectService.UberDirectService>.Instance);
    }

    /// <summary>Responde por trecho da URL e guarda cada requisição (URL, autorização e corpo).</summary>
    private sealed class ServidorUberFalso : HttpMessageHandler
    {
        private readonly Dictionary<string, (HttpStatusCode Status, string Corpo)> _respostas = new();
        private readonly Dictionary<string, Exception> _falhas = new();
        private readonly List<(string Url, string? Autorizacao, string Corpo)> _requisicoes = new();

        public void Responder(string trecho, HttpStatusCode status, string corpo)
        {
            _respostas[trecho] = (status, corpo);
            _falhas.Remove(trecho);
        }

        public void Falhar(string trecho, Exception falha) => _falhas[trecho] = falha;

        public List<(string Url, string? Autorizacao, string Corpo)> Requisicoes(string trecho) =>
            _requisicoes.Where(r => r.Url.EndsWith(trecho, StringComparison.Ordinal)).ToList();

        public List<string> Corpos(string trecho) => Requisicoes(trecho).Select(r => r.Corpo).ToList();

        public int Chamadas(string trecho) => Requisicoes(trecho).Count;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var url = request.RequestUri!.ToString();
            var corpo = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
            _requisicoes.Add((url, request.Headers.Authorization?.ToString(), corpo));

            var trecho = _respostas.Keys.Concat(_falhas.Keys).FirstOrDefault(t => url.EndsWith(t, StringComparison.Ordinal))
                         ?? throw new InvalidOperationException($"Sem resposta configurada para {url}");
            if (_falhas.TryGetValue(trecho, out var falha))
                throw falha;

            var (status, conteudo) = _respostas[trecho];
            return new HttpResponseMessage(status) { Content = new StringContent(conteudo, Encoding.UTF8, "application/json") };
        }
    }
}

using System.Net;
using System.Net.Http.Json;
using EntreIrmas.Domain.DTOs.Produto;
using EntreIrmas.Test.Config;
using FluentAssertions;

namespace EntreIrmas.Test.Tests.Api;

[Collection(ApiCollection.Nome)]
public class PedidosControllerTests
{
    private readonly HttpClient _cliente;

    public PedidosControllerTests(ApiFixture fixture)
    {
        _cliente = fixture.Cliente;
    }

    private async Task<long> IdDoProdutoAsync(string slug) =>
        (await _cliente.GetFromJsonAsync<ProdutoDto>($"/api/produtos/{slug}"))!.Id;

    private static string DataFutura() => DateTime.Today.AddDays(5).ToString("yyyy-MM-dd");

    [Fact]
    public async Task PostPedido_Valido_Retorna201ComCodigoTotalELinkDoWhatsApp()
    {
        var bolo = await IdDoProdutoAsync("bolo-de-pote-de-ninho");

        var resposta = await _cliente.PostAsJsonAsync("/api/pedidos", new
        {
            nomeCliente = "Maria",
            telefone = "(21) 99999-8888",
            tipoEntrega = "Entrega",
            endereco = new
            {
                cep = "20000-000",
                logradouro = "Rua das Flores",
                numero = "10",
                bairro = "Centro",
                cidade = "Rio de Janeiro",
                uf = "RJ"
            },
            dataDesejada = DataFutura(),
            itens = new[] { new { produtoId = bolo, quantidade = 2, preco = 0.01 } } // preço do cliente é ignorado
        });

        resposta.StatusCode.Should().Be(HttpStatusCode.Created);
        var corpo = await resposta.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        corpo!["codigo"].ToString().Should().StartWith("EI-");
        corpo["valorTotal"].ToString().Should().Be("30");
        corpo["tipoEntrega"].ToString().Should().Be("Entrega");
        corpo["linkWhatsApp"].ToString().Should().StartWith("https://wa.me/5521990225050?text=");
    }

    [Fact]
    public async Task PostPedido_DadosInvalidos_Retorna400ComErrosPorCampo()
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/pedidos", new
        {
            nomeCliente = "",
            telefone = "123",
            tipoEntrega = "Entrega",
            dataDesejada = DataFutura(),
            itens = Array.Empty<object>()
        });

        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var texto = await resposta.Content.ReadAsStringAsync();
        texto.Should().Contain("\"campo\":\"nomeCliente\"")
            .And.Contain("\"campo\":\"telefone\"")
            .And.Contain("\"campo\":\"endereco\"")
            .And.Contain("\"campo\":\"itens\"");
    }

    [Fact]
    public async Task PostPedido_AbaixoDoMinimo_Retorna400ComMensagemDoDominio()
    {
        var brigadeiro = await IdDoProdutoAsync("brigadeiro-gourmet");

        var resposta = await _cliente.PostAsJsonAsync("/api/pedidos", new
        {
            nomeCliente = "João",
            telefone = "21999998888",
            tipoEntrega = "Retirada",
            dataDesejada = DataFutura(),
            itens = new[] { new { produtoId = brigadeiro, quantidade = 5 } }
        });

        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await resposta.Content.ReadAsStringAsync()).Should().Contain("pedido mínimo de Brigadeiro Gourmet");
    }

    [Fact]
    public async Task GetPedido_PeloCodigo_DevolveAEtapaSemTelefoneNemEndereco()
    {
        var bolo = await IdDoProdutoAsync("bolo-de-pote-de-ninho");
        var criado = await _cliente.PostAsJsonAsync("/api/pedidos", new
        {
            nomeCliente = "Ana Paula",
            telefone = "21999998888",
            tipoEntrega = "Entrega",
            endereco = new
            {
                cep = "20000000",
                logradouro = "Rua das Flores",
                numero = "10",
                bairro = "Centro",
                cidade = "Rio de Janeiro",
                uf = "RJ"
            },
            dataDesejada = DataFutura(),
            itens = new[] { new { produtoId = bolo, quantidade = 1 } }
        });
        var codigo = (await criado.Content.ReadFromJsonAsync<Dictionary<string, object>>())!["codigo"].ToString()!;

        var resposta = await _cliente.GetAsync($"/api/pedidos/{codigo.ToLowerInvariant()}");

        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
        var texto = await resposta.Content.ReadAsStringAsync();
        texto.Should().Contain($"\"codigo\":\"{codigo}\"")
            .And.Contain("\"primeiroNome\":\"Ana\"")
            .And.Contain("\"status\":\"Recebido\"")
            .And.NotContain("21999998888")
            .And.NotContain("Rua das Flores");
    }

    [Theory]
    [InlineData("EI-ZZZZZZ", HttpStatusCode.NotFound)]
    [InlineData("abc", HttpStatusCode.BadRequest)]
    public async Task GetPedido_CodigoInexistenteOuInvalido_RetornaErro(string codigo, HttpStatusCode esperado)
    {
        var resposta = await _cliente.GetAsync($"/api/pedidos/{codigo}");

        resposta.StatusCode.Should().Be(esperado);
    }

    [Fact]
    public async Task Swagger_DesligadoNoAmbienteTesting()
    {
        var resposta = await _cliente.GetAsync("/api/swagger/index.html");

        resposta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

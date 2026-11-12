using System.Net;
using System.Net.Http.Json;
using EntreIrmas.Domain.DTOs.Categoria;
using EntreIrmas.Domain.DTOs.Produto;
using EntreIrmas.Test.Config;
using FluentAssertions;

namespace EntreIrmas.Test.Tests.Api;

[Collection(ApiCollection.Nome)]
public class CatalogoControllerTests
{
    private readonly HttpClient _cliente;

    public CatalogoControllerTests(ApiFixture fixture)
    {
        _cliente = fixture.Cliente;
    }

    [Fact]
    public async Task GetCategorias_SemLogin_RetornaSoCategoriasComProdutos()
    {
        var categorias = await _cliente.GetFromJsonAsync<List<CategoriaDto>>("/api/categorias");

        categorias!.Select(c => c.Slug).Should().Equal("bolos", "docinhos");
        categorias[0].QuantidadeProdutos.Should().Be(2);
    }

    [Fact]
    public async Task GetProdutos_ComFiltroDeCategoria_RetornaProdutosDaCategoria()
    {
        var produtos = await _cliente.GetFromJsonAsync<List<ProdutoDto>>("/api/produtos?categoria=docinhos");

        var brigadeiro = produtos.Should().ContainSingle().Subject;
        brigadeiro.CategoriaNome.Should().Be("Docinhos");
        brigadeiro.QuantidadeMinima.Should().Be(25);
    }

    [Fact]
    public async Task GetProdutoPorSlug_Existente_RetornaDetalhe()
    {
        var produto = await _cliente.GetFromJsonAsync<ProdutoDto>("/api/produtos/bolo-de-chocolate-com-morango");

        produto!.Preco.Should().Be(95m);
        produto.CategoriaSlug.Should().Be("bolos");
    }

    [Fact]
    public async Task GetProdutoPorSlug_Inexistente_Retorna404()
    {
        var resposta = await _cliente.GetAsync("/api/produtos/nao-existe");

        resposta.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await resposta.Content.ReadAsStringAsync()).Should().Contain("Produto não encontrado");
    }

    [Fact]
    public async Task Cors_OrigemDoSite_EhPermitida()
    {
        using var requisicao = new HttpRequestMessage(HttpMethod.Get, "/api/categorias");
        requisicao.Headers.Add("Origin", "http://localhost:4200");

        var resposta = await _cliente.SendAsync(requisicao);

        resposta.Headers.GetValues("Access-Control-Allow-Origin").Should().Contain("http://localhost:4200");
    }
}

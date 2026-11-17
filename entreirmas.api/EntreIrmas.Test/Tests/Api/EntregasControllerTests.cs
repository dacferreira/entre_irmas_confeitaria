using System.Net;
using System.Net.Http.Json;
using EntreIrmas.Application.Services;
using EntreIrmas.Domain.DTOs.Comum;
using EntreIrmas.Domain.DTOs.Entrega;
using EntreIrmas.Domain.DTOs.Pedido;
using EntreIrmas.Domain.DTOs.Produto;
using EntreIrmas.Domain.Entities.PedidoAggregate;
using EntreIrmas.Test.Config;
using FluentAssertions;

namespace EntreIrmas.Test.Tests.Api;

/// <summary>No ambiente Testing a entrega usa o simulador (ENTREGA_SIMULADOR = true).</summary>
[Collection(ApiCollection.Nome)]
public class EntregasControllerTests
{
    private readonly ApiFixture _fixture;

    public EntregasControllerTests(ApiFixture fixture)
    {
        _fixture = fixture;
    }

    private static object Endereco(string cep = "23585-000", string numero = "10") => new
    {
        cep, logradouro = "Rua B", numero, bairro = "Paciência", cidade = "Rio de Janeiro", uf = "RJ"
    };

    [Fact]
    public async Task Cotar_DevolveAMaisBarataEntreUberE99()
    {
        var resposta = await _fixture.Cliente.PostAsJsonAsync("/api/entregas/cotacao", Endereco());

        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
        var cotacao = await resposta.Content.ReadFromJsonAsync<CotacaoEntregaDto>(ApiFixture.OpcoesJson);
        cotacao!.Disponivel.Should().BeTrue();
        cotacao.Parceiro.Should().Be(ParceiroEntrega.Uber); // perto da loja, a Uber simulada é a mais barata
        cotacao.Valor.Should().Be(10.21m);
        cotacao.PrazoMinutos.Should().Be(21);
    }

    [Fact]
    public async Task Cotar_EnderecoIncompleto_Retorna400()
    {
        var resposta = await _fixture.Cliente.PostAsJsonAsync("/api/entregas/cotacao", Endereco(numero: ""));

        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await resposta.Content.ReadAsStringAsync()).Should().Contain("Número");
    }

    [Fact]
    public async Task PedidoDeEntrega_CobraATaxaEChamaOEntregadorAoFicarPronto()
    {
        var admin = await _fixture.ClienteLogadoAsync();
        var bolo = (await _fixture.Cliente.GetFromJsonAsync<ProdutoDto>("/api/produtos/bolo-de-pote-de-ninho"))!;
        var criado = await (await _fixture.Cliente.PostAsJsonAsync("/api/pedidos", new
            {
                nomeCliente = "Maria",
                telefone = "21999998888",
                tipoEntrega = "Entrega",
                endereco = Endereco(),
                dataDesejada = DateTime.Today.ToString("yyyy-MM-dd"),
                itens = new[] { new { produtoId = bolo.Id, quantidade = 1 } }
            }))
            .Content.ReadFromJsonAsync<PedidoCriadoDto>(ApiFixture.OpcoesJson);

        criado!.TaxaEntrega.Should().Be(10.21m);
        criado.ParceiroEntrega.Should().Be(ParceiroEntrega.Uber);
        criado.TotalAPagar.Should().Be(criado.ValorTotal + 10.21m);

        var pagina = await admin.GetFromJsonAsync<PaginaDto<PedidoResumoDto>>(
            $"/api/admin/pedidos?busca={criado.Codigo}", ApiFixture.OpcoesJson);
        var id = pagina!.Itens.Single().Id;

        var pronto = await admin.PatchAsJsonAsync($"/api/admin/pedidos/{id}/status", new { status = "Pronto" });
        var detalhe = await pronto.Content.ReadFromJsonAsync<PedidoDetalheDto>(ApiFixture.OpcoesJson);

        detalhe!.SituacaoEntrega.Should().Be(SituacaoEntrega.Solicitada);
        detalhe.PodeChamarEntregador.Should().BeFalse();
        detalhe.TotalAPagar.Should().Be(criado.TotalAPagar);

        var acompanhamento = await _fixture.Cliente.GetFromJsonAsync<AcompanhamentoPedidoDto>(
            $"/api/pedidos/{criado.Codigo}", ApiFixture.OpcoesJson);
        acompanhamento!.TaxaEntrega.Should().Be(10.21m);
        acompanhamento.TotalAPagar.Should().Be(criado.TotalAPagar);

        // Já chamado: chamar de novo é recusado.
        var deNovo = await admin.PostAsync($"/api/admin/pedidos/{id}/entregador", null);
        deNovo.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChamarEntregador_SemLogin_Retorna401()
    {
        var resposta = await _fixture.Cliente.PostAsync("/api/admin/pedidos/1/entregador", null);

        resposta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public void IdDaEntregaSimulada_TemPrefixoProprio() =>
        EntregaSimuladaService.PrefixoDoId.Should().Be("simulada-");
}

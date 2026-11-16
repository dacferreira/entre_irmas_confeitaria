using System.Net;
using System.Net.Http.Json;
using EntreIrmas.Domain.DTOs.Loja;
using EntreIrmas.Domain.DTOs.Pedido;
using EntreIrmas.Domain.DTOs.Produto;
using EntreIrmas.Test.Config;
using FluentAssertions;

namespace EntreIrmas.Test.Tests.Api;

[Collection(ApiCollection.Nome)]
public class LojaControllerTests
{
    private readonly ApiFixture _fixture;

    public LojaControllerTests(ApiFixture fixture)
    {
        _fixture = fixture;
    }

    private object PedidoNormal(long produtoId) => new
    {
        nomeCliente = "Maria",
        telefone = "21999998888",
        tipoEntrega = "Retirada",
        dataDesejada = DateTime.Today.ToString("yyyy-MM-dd"),
        itens = new[] { new { produtoId, quantidade = 1 } }
    };

    [Fact]
    public async Task FecharLoja_BloqueiaPedidoNormalNoSiteEAvisaOsVisitantes()
    {
        var admin = await _fixture.ClienteLogadoAsync();
        var bolo = (await _fixture.Cliente.GetFromJsonAsync<ProdutoDto>("/api/produtos/bolo-de-pote-de-ninho"))!.Id;

        try
        {
            var fechar = await admin.PutAsJsonAsync("/api/admin/loja", new { aberta = false, mensagem = "Voltamos amanhã" });
            fechar.StatusCode.Should().Be(HttpStatusCode.OK);
            (await fechar.Content.ReadFromJsonAsync<LojaAdminDto>())!.AlteradoPor.Should().Be(DadosDeTeste.LoginDeTeste);

            var situacao = await _fixture.Cliente.GetFromJsonAsync<SituacaoLojaDto>("/api/loja");
            situacao!.Aberta.Should().BeFalse();
            situacao.Mensagem.Should().Be("Voltamos amanhã");

            var pedido = await _fixture.Cliente.PostAsJsonAsync("/api/pedidos", PedidoNormal(bolo));
            pedido.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            (await pedido.Content.ReadAsStringAsync()).Should().Contain("só aceita encomendas");
        }
        finally
        {
            await admin.PutAsJsonAsync("/api/admin/loja", new { aberta = true });
        }

        (await _fixture.Cliente.GetFromJsonAsync<SituacaoLojaDto>("/api/loja"))!.Aberta.Should().BeTrue();
        (await _fixture.Cliente.PostAsJsonAsync("/api/pedidos", PedidoNormal(bolo))).StatusCode
            .Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task AlterarLoja_MensagemLonga_Retorna400()
    {
        var admin = await _fixture.ClienteLogadoAsync();

        var resposta = await admin.PutAsJsonAsync("/api/admin/loja", new { aberta = false, mensagem = new string('a', 201) });

        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UltimoDoSite_DevolveOPedidoMaisRecenteDoSite()
    {
        var admin = await _fixture.ClienteLogadoAsync();
        var bolo = (await _fixture.Cliente.GetFromJsonAsync<ProdutoDto>("/api/produtos/bolo-de-pote-de-ninho"))!.Id;
        var criado = await (await _fixture.Cliente.PostAsJsonAsync("/api/pedidos", PedidoNormal(bolo)))
            .Content.ReadFromJsonAsync<PedidoCriadoDto>(ApiFixture.OpcoesJson);

        var ultimo = await admin.GetFromJsonAsync<UltimoPedidoDto>("/api/admin/pedidos/ultimo-do-site");

        ultimo!.Codigo.Should().Be(criado!.Codigo);
        ultimo.NomeCliente.Should().Be("Maria");
    }

    [Fact]
    public async Task NovosDoSite_DevolveOsPedidosDepoisDoIdComOsItens()
    {
        var admin = await _fixture.ClienteLogadoAsync();
        var bolo = (await _fixture.Cliente.GetFromJsonAsync<ProdutoDto>("/api/produtos/bolo-de-pote-de-ninho"))!.Id;
        var respostaAntes = await admin.GetAsync("/api/admin/pedidos/ultimo-do-site");
        var antes = respostaAntes.StatusCode == HttpStatusCode.NoContent
            ? null
            : await respostaAntes.Content.ReadFromJsonAsync<UltimoPedidoDto>(ApiFixture.OpcoesJson);
        var primeiro = await (await _fixture.Cliente.PostAsJsonAsync("/api/pedidos", PedidoNormal(bolo)))
            .Content.ReadFromJsonAsync<PedidoCriadoDto>(ApiFixture.OpcoesJson);
        var segundo = await (await _fixture.Cliente.PostAsJsonAsync("/api/pedidos", PedidoNormal(bolo)))
            .Content.ReadFromJsonAsync<PedidoCriadoDto>(ApiFixture.OpcoesJson);

        var novos = await admin.GetFromJsonAsync<List<PedidoDetalheDto>>(
            $"/api/admin/pedidos/novos-do-site?depoisDe={antes?.Id ?? 0}", ApiFixture.OpcoesJson);

        novos.Should().NotBeNull();
        novos!.Select(p => p.Codigo).Should().Equal(primeiro!.Codigo, segundo!.Codigo);
        novos[0].Itens.Should().ContainSingle();
    }
}

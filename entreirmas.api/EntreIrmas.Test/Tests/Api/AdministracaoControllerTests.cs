using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using EntreIrmas.Domain.DTOs.Autenticacao;
using EntreIrmas.Domain.DTOs.Categoria;
using EntreIrmas.Domain.DTOs.Comum;
using EntreIrmas.Domain.DTOs.Lancamento;
using EntreIrmas.Domain.DTOs.Painel;
using EntreIrmas.Domain.DTOs.Pedido;
using EntreIrmas.Domain.DTOs.Produto;
using EntreIrmas.Domain.DTOs.Usuario;
using EntreIrmas.Test.Config;
using FluentAssertions;

namespace EntreIrmas.Test.Tests.Api;

[Collection(ApiCollection.Nome)]
public class AdministracaoControllerTests
{
    private readonly ApiFixture _fixture;

    public AdministracaoControllerTests(ApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Login_Valido_DevolveTokenComOsDadosDoUsuario()
    {
        var resposta = await _fixture.Cliente.PostAsJsonAsync("/api/autenticacao/login",
            new { login = "Admin.Teste", senha = DadosDeTeste.SenhaDeTeste });

        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
        var sessao = (await resposta.Content.ReadFromJsonAsync<SessaoDto>())!;
        sessao.Nome.Should().Be("Pessoa de Teste");
        sessao.ExpiraEm.Should().BeAfter(DateTime.UtcNow.AddHours(11));

        var token = new JwtSecurityTokenHandler().ReadJwtToken(sessao.Token);
        token.Claims.Should().Contain(c => c.Type == "login" && c.Value == DadosDeTeste.LoginDeTeste);
        token.Claims.Should().NotContain(c => c.Value.Contains("PBKDF2"));
    }

    [Theory]
    [InlineData("admin.teste", "senha-errada")]
    [InlineData("nao.existe", "qualquer-senha")]
    [InlineData("inativo", DadosDeTeste.SenhaDeTeste)]
    public async Task Login_Invalido_Retorna401ComAMesmaMensagem(string login, string senha)
    {
        var resposta = await _fixture.Cliente.PostAsJsonAsync("/api/autenticacao/login", new { login, senha });

        resposta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await resposta.Content.ReadAsStringAsync()).Should().Contain("Usuário ou senha inválidos.");
    }

    [Fact]
    public async Task Login_SemSenha_Retorna400()
    {
        var resposta = await _fixture.Cliente.PostAsJsonAsync("/api/autenticacao/login", new { login = "x" });

        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("/api/admin/painel")]
    [InlineData("/api/admin/pedidos")]
    [InlineData("/api/admin/produtos")]
    [InlineData("/api/admin/categorias")]
    [InlineData("/api/admin/lancamentos")]
    [InlineData("/api/admin/usuarios")]
    [InlineData("/api/admin/loja")]
    [InlineData("/api/admin/pedidos/ultimo-do-site")]
    [InlineData("/api/admin/pedidos/novos-do-site?depoisDe=0")]
    public async Task RotasAdmin_SemToken_Retornam401(string rota)
    {
        var resposta = await _fixture.Cliente.GetAsync(rota);

        resposta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RotasAdmin_ComTokenAdulterado_Retornam401()
    {
        var cliente = _fixture.CreateClient();
        cliente.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiJ9.e30.assinatura");

        var resposta = await cliente.GetAsync("/api/admin/painel");

        resposta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CicloDoProduto_CriaEditaInativaEExclui()
    {
        var cliente = await _fixture.ClienteLogadoAsync();
        var categorias = await cliente.GetFromJsonAsync<List<CategoriaAdminDto>>("/api/admin/categorias");
        var docinhos = categorias!.Single(c => c.Slug == "docinhos");

        var criar = await cliente.PostAsJsonAsync("/api/admin/produtos", new
        {
            categoriaId = docinhos.Id,
            nome = "Beijinho de Côco",
            descricao = "Coco fresco",
            preco = 2.5,
            custo = 0.9,
            unidade = "unidade",
            quantidadeMinima = 25,
            ilustracao = "brigadeiro",
            encomenda = true
        });
        criar.StatusCode.Should().Be(HttpStatusCode.Created);
        var criado = (await criar.Content.ReadFromJsonAsync<ProdutoAdminDto>())!;
        criado.Slug.Should().Be("beijinho-de-coco");
        criado.CategoriaNome.Should().Be("Docinhos");
        criado.Encomenda.Should().BeTrue();

        // No cardápio público, o produto de encomenda vem depois dos de pronta entrega.
        var cardapio = (await _fixture.Cliente.GetFromJsonAsync<List<ProdutoDto>>("/api/produtos"))!;
        cardapio[^1].Slug.Should().Be("beijinho-de-coco");
        cardapio[^1].Encomenda.Should().BeTrue();

        var editar = await cliente.PutAsJsonAsync($"/api/admin/produtos/{criado.Id}", new
        {
            categoriaId = docinhos.Id,
            nome = "Beijinho",
            descricao = "Coco fresco",
            preco = 3,
            custo = 1,
            unidade = "unidade",
            quantidadeMinima = 25,
            ilustracao = "brigadeiro"
        });
        editar.StatusCode.Should().Be(HttpStatusCode.OK);
        (await _fixture.Cliente.GetAsync("/api/produtos/beijinho")).StatusCode.Should().Be(HttpStatusCode.OK);

        var inativar = await cliente.PatchAsJsonAsync($"/api/admin/produtos/{criado.Id}/situacao", new { ativo = false });
        inativar.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await _fixture.Cliente.GetAsync("/api/produtos/beijinho")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await cliente.GetFromJsonAsync<ProdutoAdminDto>($"/api/admin/produtos/{criado.Id}"))!.Ativo.Should().BeFalse();

        (await cliente.DeleteAsync($"/api/admin/produtos/{criado.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await cliente.GetAsync($"/api/admin/produtos/{criado.Id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static MultipartFormDataContent Foto(byte[] conteudo, string nome = "foto.png", string tipo = "image/png")
    {
        var arquivo = new ByteArrayContent(conteudo);
        arquivo.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(tipo);
        return new MultipartFormDataContent { { arquivo, "arquivo", nome } };
    }

    [Fact]
    public async Task FotoDoProduto_EnviaEntregaComCacheETroca()
    {
        var cliente = await _fixture.ClienteLogadoAsync();
        var produto = (await cliente.GetFromJsonAsync<List<ProdutoAdminDto>>("/api/admin/produtos?busca=Pote"))!.Single();

        var envio = await cliente.PutAsync($"/api/admin/produtos/{produto.Id}/imagem", Foto(DadosDeTeste.ImagemPng()));
        envio.StatusCode.Should().Be(HttpStatusCode.OK);
        var comFoto = (await envio.Content.ReadFromJsonAsync<ProdutoAdminDto>())!;
        comFoto.UrlImagem.Should().StartWith($"produtos/{produto.Id}/imagem?v=");

        // A foto é pública (o site mostra sem login) e pode ficar em cache: a URL muda a cada envio.
        var foto = await _fixture.Cliente.GetAsync($"/api/{comFoto.UrlImagem}");
        foto.StatusCode.Should().Be(HttpStatusCode.OK);
        foto.Content.Headers.ContentType!.MediaType.Should().Be("image/png");
        foto.Headers.CacheControl!.ToString().Should().Contain("immutable");
        (await foto.Content.ReadAsByteArrayAsync()).Should().Equal(DadosDeTeste.ImagemPng());
        (await _fixture.Cliente.GetFromJsonAsync<ProdutoDto>("/api/produtos/bolo-de-pote-de-ninho"))!
            .UrlImagem.Should().Be(comFoto.UrlImagem);

        var remocao = await cliente.DeleteAsync($"/api/admin/produtos/{produto.Id}/imagem");
        (await remocao.Content.ReadFromJsonAsync<ProdutoAdminDto>())!.UrlImagem.Should().BeNull();
        (await _fixture.Cliente.GetAsync($"/api/produtos/{produto.Id}/imagem")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task FotoDoProduto_ArquivoQueNaoEFotoOuSemLogin_EhRecusado()
    {
        var cliente = await _fixture.ClienteLogadoAsync();
        var produto = (await cliente.GetFromJsonAsync<List<ProdutoAdminDto>>("/api/admin/produtos?busca=Brigadeiro"))!.Single();

        // Texto com nome e tipo de foto: a API olha os bytes, não o nome.
        var falsa = await cliente.PutAsync($"/api/admin/produtos/{produto.Id}/imagem",
            Foto("nao sou foto"u8.ToArray(), "foto.jpg", "image/jpeg"));
        var semArquivo = await cliente.PutAsync($"/api/admin/produtos/{produto.Id}/imagem", new MultipartFormDataContent());
        var semLogin = await _fixture.Cliente.PutAsync($"/api/admin/produtos/{produto.Id}/imagem", Foto(DadosDeTeste.ImagemPng()));

        falsa.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await falsa.Content.ReadAsStringAsync()).Should().Contain("JPG, PNG ou WEBP");
        semArquivo.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        semLogin.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Produto_DadosInvalidos_Retorna400PorCampo()
    {
        var cliente = await _fixture.ClienteLogadoAsync();

        var resposta = await cliente.PostAsJsonAsync("/api/admin/produtos", new { nome = "", preco = 0 });

        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await resposta.Content.ReadAsStringAsync()).Should().Contain("\"campo\":\"categoriaId\"").And.Contain("\"campo\":\"preco\"");
    }

    [Fact]
    public async Task CicloDaCategoria_CriaEditaEExclui()
    {
        var cliente = await _fixture.ClienteLogadoAsync();

        var criar = await cliente.PostAsJsonAsync("/api/admin/categorias", new { nome = "Pães de Mel", ordem = 9 });
        var criada = (await criar.Content.ReadFromJsonAsync<CategoriaAdminDto>())!;
        var editar = await cliente.PutAsJsonAsync($"/api/admin/categorias/{criada.Id}", new { nome = "Pão de Mel", ordem = 9 });
        var repetida = await cliente.PostAsJsonAsync("/api/admin/categorias", new { nome = "Pão de Mel" });
        var situacao = await cliente.PatchAsJsonAsync($"/api/admin/categorias/{criada.Id}/situacao", new { ativo = false });
        var excluir = await cliente.DeleteAsync($"/api/admin/categorias/{criada.Id}");
        var recriar = await cliente.PostAsJsonAsync("/api/admin/categorias", new { nome = "Pão de Mel" });

        criar.StatusCode.Should().Be(HttpStatusCode.Created);
        (await editar.Content.ReadFromJsonAsync<CategoriaAdminDto>())!.Slug.Should().Be("pao-de-mel");
        repetida.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        situacao.StatusCode.Should().Be(HttpStatusCode.NoContent);
        excluir.StatusCode.Should().Be(HttpStatusCode.NoContent);
        recriar.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Categoria_ComProdutos_NaoPodeSerExcluida()
    {
        var cliente = await _fixture.ClienteLogadoAsync();
        var bolos = (await cliente.GetFromJsonAsync<List<CategoriaAdminDto>>("/api/admin/categorias"))!
            .Single(c => c.Slug == "bolos");

        var resposta = await cliente.DeleteAsync($"/api/admin/categorias/{bolos.Id}");

        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await resposta.Content.ReadAsStringAsync()).Should().Contain("Mova ou exclua os produtos");
    }

    [Fact]
    public async Task PedidoManual_EntraConfirmadoEAparecesNoPainel()
    {
        var cliente = await _fixture.ClienteLogadoAsync();
        var bolo = (await cliente.GetFromJsonAsync<List<ProdutoAdminDto>>("/api/admin/produtos?busca=Chocolate"))!.Single();
        var data = new DateOnly(2025, 3, 10); // no passado: pedido manual aceita

        var criar = await cliente.PostAsJsonAsync("/api/admin/pedidos", new
        {
            nomeCliente = "Cliente do WhatsApp",
            telefone = "21988887777",
            tipoEntrega = "Retirada",
            dataDesejada = data.ToString("yyyy-MM-dd"),
            itens = new[] { new { produtoId = bolo.Id, quantidade = 2 } }
        });
        criar.StatusCode.Should().Be(HttpStatusCode.Created);
        var pedido = (await criar.Content.ReadFromJsonAsync<PedidoDetalheDto>(ApiFixture.OpcoesJson))!;
        pedido.Status.ToString().Should().Be("Confirmado");
        pedido.CadastradoPor.Should().Be(DadosDeTeste.LoginDeTeste);
        pedido.CustoTotal.Should().Be(80m);

        var lancamento = await cliente.PostAsJsonAsync("/api/admin/lancamentos", new
        {
            tipo = "Perda",
            descricao = "Bolo que caiu",
            valor = 30,
            data = data.ToString("yyyy-MM-dd")
        });
        lancamento.StatusCode.Should().Be(HttpStatusCode.Created);

        var painel = await cliente.GetFromJsonAsync<PainelDto>("/api/admin/painel?inicio=2025-03-01&fim=2025-03-31", ApiFixture.OpcoesJson);
        painel!.Receita.Should().Be(190m);
        painel.CustoProdutos.Should().Be(80m);
        painel.Perdas.Should().Be(30m);
        painel.Lucro.Should().Be(80m);

        var status = await cliente.PatchAsJsonAsync($"/api/admin/pedidos/{pedido.Id}/status", new { status = "Cancelado" });
        status.StatusCode.Should().Be(HttpStatusCode.OK);
        var depois = await cliente.GetFromJsonAsync<PainelDto>("/api/admin/painel?inicio=2025-03-01&fim=2025-03-31", ApiFixture.OpcoesJson);
        depois!.Receita.Should().Be(0m);
        depois.Cancelados.Quantidade.Should().Be(1);

        var lista = await cliente.GetFromJsonAsync<PaginaDto<PedidoResumoDto>>(
            "/api/admin/pedidos?status=Cancelado&inicio=2025-03-01&fim=2025-03-31", ApiFixture.OpcoesJson);
        lista!.Itens.Should().ContainSingle().Which.Codigo.Should().Be(pedido.Codigo);
        (await cliente.GetAsync($"/api/admin/pedidos/{pedido.Id}")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await cliente.DeleteAsync($"/api/admin/pedidos/{pedido.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Painel_PeriodoInvertido_Retorna400()
    {
        var cliente = await _fixture.ClienteLogadoAsync();

        var resposta = await cliente.GetAsync("/api/admin/painel?inicio=2026-11-10&fim=2026-11-01");

        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CicloDoLancamento_EditaListaEExclui()
    {
        var cliente = await _fixture.ClienteLogadoAsync();
        var criar = await cliente.PostAsJsonAsync("/api/admin/lancamentos",
            new { tipo = "Despesa", descricao = "Gás", valor = 120, data = "2024-06-01" });
        var criado = (await criar.Content.ReadFromJsonAsync<LancamentoDto>(ApiFixture.OpcoesJson))!;

        var editar = await cliente.PutAsJsonAsync($"/api/admin/lancamentos/{criado.Id}",
            new { tipo = "Despesa", descricao = "Gás de cozinha", valor = 125.5, data = "2024-06-01" });
        var lista = await cliente.GetFromJsonAsync<List<LancamentoDto>>("/api/admin/lancamentos?inicio=2024-06-01&fim=2024-06-30", ApiFixture.OpcoesJson);
        var excluir = await cliente.DeleteAsync($"/api/admin/lancamentos/{criado.Id}");

        editar.StatusCode.Should().Be(HttpStatusCode.OK);
        lista.Should().ContainSingle().Which.Descricao.Should().Be("Gás de cozinha");
        excluir.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Usuarios_CriaENaoDeixaDesativarOProprio()
    {
        var cliente = await _fixture.ClienteLogadoAsync();

        var criar = await cliente.PostAsJsonAsync("/api/admin/usuarios",
            new { login = "nova.pessoa", nome = "Nova Pessoa", senha = "uma-senha-boa" });
        var lista = await cliente.GetFromJsonAsync<List<UsuarioDto>>("/api/admin/usuarios");
        var eu = lista!.Single(u => u.Login == DadosDeTeste.LoginDeTeste);
        var desativarAMimMesmo = await cliente.PatchAsJsonAsync($"/api/admin/usuarios/{eu.Id}/situacao", new { ativo = false });
        var nova = lista!.Single(u => u.Login == "nova.pessoa");
        var editarNova = await cliente.PutAsJsonAsync($"/api/admin/usuarios/{nova.Id}",
            new { login = "nova.pessoa", nome = "Nova", senha = "" });
        var excluirNova = await cliente.DeleteAsync($"/api/admin/usuarios/{nova.Id}");

        criar.StatusCode.Should().Be(HttpStatusCode.Created);
        (await criar.Content.ReadAsStringAsync()).Should().NotContain("hash").And.NotContain("PBKDF2");
        desativarAMimMesmo.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        editarNova.StatusCode.Should().Be(HttpStatusCode.OK);
        excluirNova.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}

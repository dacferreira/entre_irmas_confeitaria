using EntreIrmas.Domain.DTOs.Produto;
using EntreIrmas.Domain.Entities.PedidoAggregate;
using EntreIrmas.Infrastructure.Persistence;
using EntreIrmas.Infrastructure.Persistence.Repositories;
using EntreIrmas.Test.Config;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace EntreIrmas.Test.Tests.Infrastructure;

public class RepositoriosTests : IDisposable
{
    private readonly EntreIrmasDbContext _contexto;

    public RepositoriosTests()
    {
        var opcoes = new DbContextOptionsBuilder<EntreIrmasDbContext>()
            .UseInMemoryDatabase($"repositorios-{Guid.NewGuid()}")
            .Options;

        _contexto = new EntreIrmasDbContext(opcoes);
        DadosDeTeste.PopularCardapio(_contexto);
    }

    [Fact]
    public async Task ProdutoListar_SemFiltro_IgnoraInativosEOrdenaPorCategoria()
    {
        var produtos = await new ProdutoRepository(_contexto).ListarAsync(new FiltroProdutoDto());

        produtos.Select(p => p.Slug).Should().Equal(
            "bolo-de-chocolate-com-morango", "bolo-de-pote-de-ninho", "brigadeiro-gourmet");
        produtos.Should().OnlyContain(p => p.Categoria != null);
    }

    [Fact]
    public async Task ProdutoListar_ComFiltros_AplicaCategoriaBuscaEDestaque()
    {
        var repositorio = new ProdutoRepository(_contexto);

        (await repositorio.ListarAsync(new FiltroProdutoDto { Categoria = " DOCINHOS " }))
            .Should().ContainSingle().Which.Slug.Should().Be("brigadeiro-gourmet");
        (await repositorio.ListarAsync(new FiltroProdutoDto { Busca = "Ninho" }))
            .Should().ContainSingle().Which.Slug.Should().Be("bolo-de-pote-de-ninho");
        (await repositorio.ListarAsync(new FiltroProdutoDto { Destaque = true }))
            .Should().HaveCount(2);
    }

    [Fact]
    public async Task ProdutoObterPorSlugEPorIds_TrazApenasAtivos()
    {
        var repositorio = new ProdutoRepository(_contexto);
        var todos = await _contexto.Produtos.IgnoreQueryFilters().ToListAsync();

        (await repositorio.ObterPorSlugAsync("bolo-antigo")).Should().BeNull();
        (await repositorio.ObterPorSlugAsync("brigadeiro-gourmet"))!.Categoria!.Slug.Should().Be("docinhos");
        (await repositorio.ObterPorIdsAsync(todos.Select(p => p.Id))).Should().HaveCount(3);
    }

    [Fact]
    public async Task CategoriaListarComProdutos_OrdenaECarregaSoProdutosAtivos()
    {
        var categorias = await new CategoriaRepository(_contexto).ListarComProdutosAsync();

        categorias.Select(c => c.Slug).Should().Equal("bolos", "docinhos", "tortas");
        categorias[0].Produtos.Should().HaveCount(2);
        categorias[2].Produtos.Should().BeEmpty();
    }

    [Fact]
    public async Task PedidoAdicionar_GravaItensEDetectaCodigoExistente()
    {
        var repositorio = new PedidoRepository(_contexto);
        var produto = await _contexto.Produtos.FirstAsync(p => p.Slug == "bolo-de-pote-de-ninho");
        var pedido = DadosDeTeste.Pedido();
        pedido.AdicionarItem(produto, 3);

        await repositorio.AdicionarAsync(pedido);
        await repositorio.SalvarAlteracoesAsync();

        var gravado = await repositorio.ObterPorIdAsync(pedido.Id);
        gravado!.Itens.Should().ContainSingle().Which.ValorTotal.Should().Be(45m);
        gravado.Status.Should().Be(StatusPedido.Recebido);
        (await repositorio.ExisteCodigoAsync("EI-ABC234")).Should().BeTrue();
        (await repositorio.ExisteCodigoAsync("EI-ZZZZZZ")).Should().BeFalse();
    }

    public void Dispose() => _contexto.Dispose();
}

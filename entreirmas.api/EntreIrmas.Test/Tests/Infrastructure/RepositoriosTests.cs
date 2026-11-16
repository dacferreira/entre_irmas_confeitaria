using EntreIrmas.Domain.DTOs.Lancamento;
using EntreIrmas.Domain.DTOs.Pedido;
using EntreIrmas.Domain.DTOs.Produto;
using EntreIrmas.Domain.Entities.LancamentoAggregate;
using EntreIrmas.Domain.Entities.LojaAggregate;
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

    [Fact]
    public async Task PedidoObterPorCodigo_TrazItensEIgnoraOsExcluidos()
    {
        var repositorio = new PedidoRepository(_contexto);
        var produto = await _contexto.Produtos.FirstAsync(p => p.Slug == "bolo-de-pote-de-ninho");
        var ativo = DadosDeTeste.Pedido(codigo: "EI-ABC234");
        ativo.AdicionarItem(produto, 1);
        var excluido = DadosDeTeste.Pedido(codigo: "EI-DEF567");
        excluido.AdicionarItem(produto, 1);
        excluido.Arquivar();
        await repositorio.AdicionarAsync(ativo);
        await repositorio.AdicionarAsync(excluido);
        await repositorio.SalvarAlteracoesAsync();
        _contexto.ChangeTracker.Clear();

        var encontrado = await repositorio.ObterPorCodigoAsync("EI-ABC234");

        encontrado!.Itens.Should().ContainSingle();
        (await repositorio.ObterPorCodigoAsync("EI-DEF567")).Should().BeNull();
        (await repositorio.ObterPorCodigoAsync("EI-ZZZZZZ")).Should().BeNull();
    }

    [Fact]
    public async Task PedidoObterUltimo_ConsideraSoQuemCadastrouEIgnoraExcluidos()
    {
        var repositorio = new PedidoRepository(_contexto);
        var produto = await _contexto.Produtos.FirstAsync(p => p.Slug == "bolo-de-pote-de-ninho");
        Pedido Novo(string codigo, string cadastradoPor)
        {
            var pedido = new Pedido(codigo, "Maria", "21999998888", TipoEntrega.Retirada, null,
                new DateOnly(2026, 12, 20), null, cadastradoPor);
            pedido.AdicionarItem(produto, 1);
            return pedido;
        }

        var doSite = Novo("EI-AAAAA2", "SITE");
        var manual = Novo("EI-AAAAA3", "suellen");
        var excluido = Novo("EI-AAAAA4", "SITE");
        excluido.Arquivar();
        foreach (var pedido in new[] { doSite, manual, excluido })
            await repositorio.AdicionarAsync(pedido);
        await repositorio.SalvarAlteracoesAsync();

        (await repositorio.ObterUltimoAsync("SITE"))!.Codigo.Should().Be("EI-AAAAA2");
        (await repositorio.ObterUltimoAsync("ninguem")).Should().BeNull();
    }

    [Fact]
    public async Task PedidoListarDepoisDe_TrazOsDoSiteComIdMaiorEmOrdemComItensERespeitaOMaximo()
    {
        var repositorio = new PedidoRepository(_contexto);
        var produto = await _contexto.Produtos.FirstAsync(p => p.Slug == "bolo-de-pote-de-ninho");
        Pedido Novo(string codigo, string cadastradoPor)
        {
            var pedido = new Pedido(codigo, "Maria", "21999998888", TipoEntrega.Retirada, null,
                new DateOnly(2026, 12, 20), null, cadastradoPor);
            pedido.AdicionarItem(produto, 1);
            return pedido;
        }

        var antigo = Novo("EI-BBBBB2", "SITE");
        await repositorio.AdicionarAsync(antigo);
        await repositorio.SalvarAlteracoesAsync();
        var excluido = Novo("EI-BBBBB5", "SITE");
        excluido.Arquivar();
        foreach (var pedido in new[] { Novo("EI-BBBBB3", "SITE"), Novo("EI-BBBBB4", "suellen"), excluido, Novo("EI-BBBBB6", "SITE") })
            await repositorio.AdicionarAsync(pedido);
        await repositorio.SalvarAlteracoesAsync();

        var novos = await repositorio.ListarDepoisDeAsync("SITE", antigo.Id, 10);

        novos.Select(p => p.Codigo).Should().Equal("EI-BBBBB3", "EI-BBBBB6");
        novos.Should().OnlyContain(p => p.Itens.Count == 1);
        (await repositorio.ListarDepoisDeAsync("SITE", antigo.Id, 1)).Select(p => p.Codigo).Should().Equal("EI-BBBBB3");
    }

    [Fact]
    public async Task LojaObter_DevolveORegistroUnicoOuNulo()
    {
        var repositorio = new LojaRepository(_contexto);
        (await repositorio.ObterAsync()).Should().BeNull();

        await repositorio.AdicionarAsync(new Loja("TESTE"));
        await repositorio.SalvarAlteracoesAsync();

        (await repositorio.ObterAsync())!.Aberta.Should().BeTrue();
    }

    [Fact]
    public async Task ProdutoListar_ColocaOsDeEncomendaPorUltimo()
    {
        var bolo = await _contexto.Produtos.FirstAsync(p => p.Slug == "bolo-de-chocolate-com-morango");
        bolo.Atualizar(bolo.CategoriaId, bolo.Nome, bolo.Slug, bolo.Descricao, bolo.Preco, bolo.Custo, bolo.Unidade,
            bolo.QuantidadeMinima, bolo.Ilustracao, bolo.Destaque, encomenda: true);
        await _contexto.SaveChangesAsync();

        var produtos = await new ProdutoRepository(_contexto).ListarAsync(new FiltroProdutoDto());

        produtos.Select(p => p.Slug).Should().Equal(
            "bolo-de-pote-de-ninho", "brigadeiro-gourmet", "bolo-de-chocolate-com-morango");
    }

    [Fact]
    public async Task ProdutoAdmin_TrazInativosEsconderExcluidos()
    {
        var repositorio = new ProdutoRepository(_contexto);
        var excluido = await _contexto.Produtos.FirstAsync(p => p.Slug == "bolo-de-pote-de-ninho");
        excluido.Excluir();
        await _contexto.SaveChangesAsync();
        var bolos = await _contexto.Categorias.FirstAsync(c => c.Slug == "bolos");

        var todos = await repositorio.ListarAdminAsync(new FiltroProdutoAdminDto());
        var inativos = await repositorio.ListarAdminAsync(new FiltroProdutoAdminDto { Ativo = false });
        var daCategoria = await repositorio.ListarAdminAsync(new FiltroProdutoAdminDto { CategoriaId = bolos.Id, Busca = "Choco" });

        todos.Select(p => p.Slug).Should().BeEquivalentTo("bolo-antigo", "bolo-de-chocolate-com-morango", "brigadeiro-gourmet");
        todos.Should().OnlyContain(p => p.Categoria != null);
        inativos.Should().ContainSingle().Which.Slug.Should().Be("bolo-antigo");
        daCategoria.Should().ContainSingle().Which.Slug.Should().Be("bolo-de-chocolate-com-morango");
        (await repositorio.ObterAdminPorIdAsync(excluido.Id)).Should().BeNull();
        (await repositorio.ExisteSlugAsync("bolo-antigo", null)).Should().BeTrue();
        (await repositorio.ExisteSlugAsync("bolo-de-pote-de-ninho", null)).Should().BeFalse();
        (await repositorio.ContarPorCategoriaAsync(bolos.Id)).Should().Be(2);
    }

    [Fact]
    public async Task CategoriaAdmin_TrazInativasComOsProdutosNaoExcluidos()
    {
        var repositorio = new CategoriaRepository(_contexto);
        var tortas = await _contexto.Categorias.FirstAsync(c => c.Slug == "tortas");
        tortas.Inativar();
        await _contexto.SaveChangesAsync();

        var categorias = await repositorio.ListarAdminAsync();
        var bolos = await repositorio.ObterAdminPorIdAsync(categorias[0].Id);

        categorias.Select(c => c.Slug).Should().Equal("bolos", "docinhos", "tortas");
        bolos!.Produtos.Should().HaveCount(3);
        (await repositorio.ExisteSlugAsync("tortas", null)).Should().BeTrue();
        (await repositorio.ExisteSlugAsync("tortas", tortas.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task Usuario_LoginSoDeAtivosEContagem()
    {
        DadosDeTeste.PopularUsuarios(_contexto);
        var repositorio = new UsuarioRepository(_contexto);

        (await repositorio.ObterPorLoginAsync(DadosDeTeste.LoginDeTeste)).Should().NotBeNull();
        (await repositorio.ObterPorLoginAsync("inativo")).Should().BeNull();
        (await repositorio.ListarAdminAsync()).Should().HaveCount(3);
        (await repositorio.ContarAtivosAsync()).Should().Be(2);
        (await repositorio.ExisteLoginAsync("inativo", null)).Should().BeTrue();
        var inativo = (await repositorio.ListarAdminAsync()).First(u => u.Login == "inativo");
        (await repositorio.ObterAdminPorIdAsync(inativo.Id)).Should().NotBeNull();
    }

    [Fact]
    public async Task PedidoListar_FiltraPaginaEOrdena()
    {
        var repositorio = new PedidoRepository(_contexto);
        var produto = await _contexto.Produtos.FirstAsync(p => p.Slug == "bolo-de-pote-de-ninho");
        for (var dia = 1; dia <= 5; dia++)
        {
            var pedido = DadosDeTeste.Pedido(data: new DateOnly(2026, 11, dia), codigo: $"EI-00000{dia}");
            pedido.AdicionarItem(produto, 1);
            if (dia == 2)
                pedido.AlterarStatus(StatusPedido.Cancelado);
            await repositorio.AdicionarAsync(pedido);
        }
        await repositorio.SalvarAlteracoesAsync();

        var (pagina, total) = await repositorio.ListarAsync(new FiltroPedidoDto { Pagina = 2, TamanhoPagina = 2 });
        var (cancelados, _) = await repositorio.ListarAsync(new FiltroPedidoDto { Status = StatusPedido.Cancelado });
        var (porCodigo, _) = await repositorio.ListarAsync(new FiltroPedidoDto { Busca = "EI-000004" });
        var (porTelefone, totalTelefone) = await repositorio.ListarAsync(new FiltroPedidoDto { Busca = "(21) 99999" });
        var (noPeriodo, _) = await repositorio.ListarAsync(new FiltroPedidoDto
        {
            Inicio = new DateOnly(2026, 11, 2),
            Fim = new DateOnly(2026, 11, 3)
        });

        total.Should().Be(5);
        pagina.Select(p => p.Codigo).Should().Equal("EI-000003", "EI-000002");
        pagina[0].Itens.Should().ContainSingle();
        cancelados.Should().ContainSingle().Which.Codigo.Should().Be("EI-000002");
        porCodigo.Should().ContainSingle();
        totalTelefone.Should().Be(5);
        porTelefone.Should().HaveCount(5);
        noPeriodo.Should().HaveCount(2);
        (await repositorio.ListarPorPeriodoAsync(new DateOnly(2026, 11, 4), new DateOnly(2026, 11, 30))).Should().HaveCount(2);
    }

    [Fact]
    public async Task Lancamento_FiltraPorPeriodoETipo()
    {
        var repositorio = new LancamentoRepository(_contexto);
        await repositorio.AdicionarAsync(DadosDeTeste.Lancamento(TipoLancamento.Despesa, 10m, new DateOnly(2026, 11, 1)));
        await repositorio.AdicionarAsync(DadosDeTeste.Lancamento(TipoLancamento.Perda, 20m, new DateOnly(2026, 11, 5)));
        await repositorio.AdicionarAsync(DadosDeTeste.Lancamento(TipoLancamento.Despesa, 30m, new DateOnly(2026, 12, 1)));
        await repositorio.SalvarAlteracoesAsync();

        var novembro = await repositorio.ListarAsync(new FiltroLancamentoDto
        {
            Inicio = new DateOnly(2026, 11, 1),
            Fim = new DateOnly(2026, 11, 30)
        });
        var perdas = await repositorio.ListarAsync(new FiltroLancamentoDto { Tipo = TipoLancamento.Perda });

        novembro.Select(l => l.Valor).Should().Equal(20m, 10m);
        perdas.Should().ContainSingle().Which.Valor.Should().Be(20m);
    }

    public void Dispose() => _contexto.Dispose();
}

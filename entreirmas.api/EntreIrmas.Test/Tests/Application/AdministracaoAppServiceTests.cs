using EntreIrmas.Application.AppServices;
using EntreIrmas.Domain.DTOs.Categoria;
using EntreIrmas.Domain.DTOs.Lancamento;
using EntreIrmas.Domain.DTOs.Produto;
using EntreIrmas.Domain.DTOs.Usuario;
using EntreIrmas.Domain.Entities.CategoriaAggregate;
using EntreIrmas.Domain.Entities.LancamentoAggregate;
using EntreIrmas.Domain.Entities.PedidoAggregate;
using EntreIrmas.Domain.Entities.ProdutoAggregate;
using EntreIrmas.Domain.Entities.UsuarioAggregate;
using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Domain.Repositories;
using EntreIrmas.Domain.Seedwork;
using EntreIrmas.Domain.Services;
using EntreIrmas.Test.Config;
using FluentAssertions;
using MapsterMapper;
using Moq;
using Moq.AutoMock;

namespace EntreIrmas.Test.Tests.Application;

public class CatalogoAdminAppServiceTests
{
    private readonly AutoMocker _mocker = new();

    private static readonly DateTimeOffset Agora = new(2026, 11, 14, 10, 0, 0, TimeSpan.Zero);

    public CatalogoAdminAppServiceTests()
    {
        _mocker.Use<IMapper>(DadosDeTeste.CriarMapper());
        _mocker.Use<TimeProvider>(new TempoFixo(Agora));
        _mocker.GetMock<IAuthentication>().Setup(a => a.Login).Returns("suellen");
    }

    private Produto ComProduto(long id = 9)
    {
        var produto = DadosDeTeste.Produto(id);
        _mocker.GetMock<IProdutoRepository>()
            .Setup(r => r.ObterAdminPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(produto);
        return produto;
    }

    private void ComImagem(ImagemProduto? imagem) =>
        _mocker.GetMock<IImagemProdutoRepository>()
            .Setup(r => r.ObterPorProdutoAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(imagem);

    [Fact]
    public async Task DefinirImagem_SemFotoAnterior_CriaEApontaOProdutoComVersao()
    {
        var produto = ComProduto();
        ImagemProduto? criada = null;
        _mocker.GetMock<IImagemProdutoRepository>()
            .Setup(r => r.AdicionarAsync(It.IsAny<ImagemProduto>(), It.IsAny<CancellationToken>()))
            .Callback<ImagemProduto, CancellationToken>((i, _) => criada = i);
        var appService = _mocker.CreateInstance<ProdutoAppService>();

        var dto = await appService.DefinirImagemAsync(9, DadosDeTeste.ImagemPng());

        criada!.TipoConteudo.Should().Be("image/png");
        criada.CadastradoPor.Should().Be("suellen");
        dto.UrlImagem.Should().Be($"produtos/9/imagem?v={Agora.UtcTicks}");
        produto.UrlImagem.Should().Be(dto.UrlImagem);
        _mocker.Verify<IProdutoRepository>(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DefinirImagem_ComFotoAnterior_SubstituiSemCriarOutra()
    {
        ComProduto();
        var existente = new ImagemProduto(9, DadosDeTeste.ImagemJpeg(), "T");
        ComImagem(existente);
        var appService = _mocker.CreateInstance<ProdutoAppService>();

        await appService.DefinirImagemAsync(9, DadosDeTeste.ImagemWebp());

        existente.TipoConteudo.Should().Be("image/webp");
        _mocker.Verify<IImagemProdutoRepository>(
            r => r.AdicionarAsync(It.IsAny<ImagemProduto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DefinirImagem_ArquivoInvalidoOuProdutoInexistente_LancaExcecao()
    {
        ComProduto();
        var appService = _mocker.CreateInstance<ProdutoAppService>();

        var invalido = () => appService.DefinirImagemAsync(9, "texto"u8.ToArray());
        var inexistente = () => appService.DefinirImagemAsync(404, DadosDeTeste.ImagemPng());

        await invalido.Should().ThrowAsync<DomainException>();
        await inexistente.Should().ThrowAsync<ObjectNotFoundException>();
        _mocker.Verify<IProdutoRepository>(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoverImagem_ApagaAFotoEVoltaParaAIlustracao()
    {
        var produto = ComProduto();
        produto.DefinirImagem(1);
        var existente = new ImagemProduto(9, DadosDeTeste.ImagemJpeg(), "T");
        ComImagem(existente);
        var appService = _mocker.CreateInstance<ProdutoAppService>();

        var dto = await appService.RemoverImagemAsync(9);

        dto.UrlImagem.Should().BeNull();
        _mocker.Verify<IImagemProdutoRepository>(r => r.Remover(existente), Times.Once);
    }

    [Fact]
    public async Task ObterImagem_DevolveOArquivoOuLancaNaoEncontrado()
    {
        var appService = _mocker.CreateInstance<ProdutoAppService>();
        var semFoto = () => appService.ObterImagemAsync(9);
        await semFoto.Should().ThrowAsync<ObjectNotFoundException>();

        ComImagem(new ImagemProduto(9, DadosDeTeste.ImagemPng(), "T"));
        var imagem = await appService.ObterImagemAsync(9);

        imagem.TipoConteudo.Should().Be("image/png");
        imagem.Conteudo.Should().Equal(DadosDeTeste.ImagemPng());
    }

    [Fact]
    public async Task ExcluirProduto_ApagaTambemAFoto()
    {
        ComProduto();
        var existente = new ImagemProduto(9, DadosDeTeste.ImagemJpeg(), "T");
        ComImagem(existente);
        var appService = _mocker.CreateInstance<ProdutoAppService>();

        await appService.ExcluirAsync(9);

        _mocker.Verify<IImagemProdutoRepository>(r => r.Remover(existente), Times.Once);
    }

    private static SalvarProdutoDto ProdutoDto(string nome = "Bolo de Côco!") => new()
    {
        CategoriaId = 3,
        Nome = nome,
        Descricao = "Fofinho",
        Preco = 50m,
        Custo = 20m,
        Unidade = "unidade",
        QuantidadeMinima = 1,
        Ilustracao = "bolo"
    };

    private void ComCategoria(Categoria? categoria) =>
        _mocker.GetMock<ICategoriaRepository>()
            .Setup(r => r.ObterAdminPorIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(categoria);

    [Fact]
    public async Task CategoriaCriar_GeraOSlugPeloNomeEGravaQuemCadastrou()
    {
        Categoria? gravada = null;
        _mocker.GetMock<ICategoriaRepository>()
            .Setup(r => r.AdicionarAsync(It.IsAny<Categoria>(), It.IsAny<CancellationToken>()))
            .Callback<Categoria, CancellationToken>((c, _) => gravada = c);
        var appService = _mocker.CreateInstance<CategoriaAppService>();

        var criada = await appService.CriarAsync(new SalvarCategoriaDto { Nome = "Pães de Mel", Ordem = 4 });

        criada.Slug.Should().Be("paes-de-mel");
        criada.Ativo.Should().BeTrue();
        gravada!.CadastradoPor.Should().Be("suellen");
    }

    [Fact]
    public async Task CategoriaAtualizar_NomeRepetido_LancaDomainException()
    {
        ComCategoria(new Categoria("Bolos", "bolos", null, 1, "T"));
        _mocker.GetMock<ICategoriaRepository>()
            .Setup(r => r.ExisteSlugAsync("docinhos", 8, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var appService = _mocker.CreateInstance<CategoriaAppService>();

        var atualizar = () => appService.AtualizarAsync(8, new SalvarCategoriaDto { Nome = "Docinhos" });

        await atualizar.Should().ThrowAsync<DomainException>().WithMessage("Já existe*");
    }

    [Fact]
    public async Task CategoriaSituacaoEExcluir()
    {
        var categoria = new Categoria("Bolos", "bolos", null, 1, "T");
        ComCategoria(categoria);
        var appService = _mocker.CreateInstance<CategoriaAppService>();

        await appService.AlterarSituacaoAsync(1, false);
        categoria.Estado.Should().Be(EntityStatus.Inativo);
        await appService.AlterarSituacaoAsync(1, true);
        categoria.Estado.Should().Be(EntityStatus.Ativo);
        await appService.ExcluirAsync(1);
        categoria.Estado.Should().Be(EntityStatus.Arquivado);
    }

    [Fact]
    public async Task CategoriaInexistente_LancaObjectNotFound()
    {
        var appService = _mocker.CreateInstance<CategoriaAppService>();

        var excluir = () => appService.ExcluirAsync(404);

        await excluir.Should().ThrowAsync<ObjectNotFoundException>();
    }

    [Fact]
    public async Task ProdutoCriar_UsaACategoriaDoCadastroEOSlugDoNome()
    {
        var categoria = new Categoria("Bolos", "bolos", null, 1, "T");
        DadosDeTeste.DefinirId(categoria, 3);
        ComCategoria(categoria);
        var appService = _mocker.CreateInstance<ProdutoAppService>();

        var criado = await appService.CriarAsync(ProdutoDto());

        criado.Slug.Should().Be("bolo-de-coco");
        criado.CategoriaNome.Should().Be("Bolos");
        criado.Custo.Should().Be(20m);
        _mocker.Verify<IProdutoRepository>(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProdutoCriar_CategoriaInexistenteOuNomeSemLetras_LancaDomainException()
    {
        var appService = _mocker.CreateInstance<ProdutoAppService>();

        var semCategoria = () => appService.CriarAsync(ProdutoDto());
        await semCategoria.Should().ThrowAsync<DomainException>().WithMessage("Categoria não encontrada*");

        ComCategoria(new Categoria("Bolos", "bolos", null, 1, "T"));
        var semLetras = () => appService.CriarAsync(ProdutoDto("!!!"));
        await semLetras.Should().ThrowAsync<DomainException>().WithMessage("Use letras*");
    }

    [Fact]
    public async Task ProdutoAtualizarSituacaoEExcluir()
    {
        var produto = DadosDeTeste.Produto(9);
        _mocker.GetMock<IProdutoRepository>()
            .Setup(r => r.ObterAdminPorIdAsync(9, It.IsAny<CancellationToken>()))
            .ReturnsAsync(produto);
        var categoria = new Categoria("Bolos", "bolos", null, 1, "T");
        DadosDeTeste.DefinirId(categoria, 3);
        ComCategoria(categoria);
        var appService = _mocker.CreateInstance<ProdutoAppService>();

        var atualizado = await appService.AtualizarAsync(9, ProdutoDto("Bolo de Fubá"));
        await appService.AlterarSituacaoAsync(9, false);

        atualizado.Nome.Should().Be("Bolo de Fubá");
        (await appService.ObterAdminAsync(9)).Ativo.Should().BeFalse();
        await appService.AlterarSituacaoAsync(9, true);
        await appService.ExcluirAsync(9);
        produto.Estado.Should().Be(EntityStatus.Arquivado);
    }

    [Fact]
    public async Task ProdutoListarAdmin_Mapeia()
    {
        _mocker.GetMock<IProdutoRepository>()
            .Setup(r => r.ListarAdminAsync(It.IsAny<FiltroProdutoAdminDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Produto> { DadosDeTeste.Produto(1, custo: 3m) });
        var appService = _mocker.CreateInstance<ProdutoAppService>();

        var produtos = await appService.ListarAdminAsync(new FiltroProdutoAdminDto());

        produtos.Should().ContainSingle().Which.Custo.Should().Be(3m);
    }
}

public class UsuarioAppServiceTests
{
    private readonly AutoMocker _mocker = new();
    private readonly UsuarioAppService _appService;

    public UsuarioAppServiceTests()
    {
        _mocker.Use<IMapper>(DadosDeTeste.CriarMapper());
        _mocker.Use<ISenhaService>(DadosDeTeste.CriarSenhaService());
        _mocker.GetMock<IAuthentication>().Setup(a => a.Id).Returns(1);
        _mocker.GetMock<IAuthentication>().Setup(a => a.Login).Returns("suellen");
        _mocker.GetMock<IUsuarioRepository>()
            .Setup(r => r.ContarAtivosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);
        _appService = _mocker.CreateInstance<UsuarioAppService>();
    }

    private Usuario ComUsuario(long id, string login = "tatiane")
    {
        var usuario = DadosDeTeste.Usuario(id, login);
        _mocker.GetMock<IUsuarioRepository>()
            .Setup(r => r.ObterAdminPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        return usuario;
    }

    [Fact]
    public async Task Criar_GravaSoOHashDaSenha()
    {
        Usuario? gravado = null;
        _mocker.GetMock<IUsuarioRepository>()
            .Setup(r => r.AdicionarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
            .Callback<Usuario, CancellationToken>((u, _) => gravado = u);

        var criado = await _appService.CriarAsync(new SalvarUsuarioDto { Login = "Nova", Nome = "Nova", Senha = "senha-forte-1" });

        criado.Login.Should().Be("nova");
        gravado!.HashSenha.Should().StartWith("PBKDF2$").And.NotContain("senha-forte-1");
        gravado.CadastradoPor.Should().Be("suellen");
    }

    [Fact]
    public async Task Criar_SemSenhaOuLoginRepetido_LancaDomainException()
    {
        _mocker.GetMock<IUsuarioRepository>()
            .Setup(r => r.ExisteLoginAsync("tatiane", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var semSenha = () => _appService.CriarAsync(new SalvarUsuarioDto { Login = "nova", Nome = "Nova" });
        var repetido = () => _appService.CriarAsync(new SalvarUsuarioDto { Login = "tatiane", Nome = "T", Senha = "12345678" });

        await semSenha.Should().ThrowAsync<DomainException>().WithMessage("Informe a senha*");
        await repetido.Should().ThrowAsync<DomainException>().WithMessage("Já existe*");
    }

    [Fact]
    public async Task Atualizar_TrocaNomeESenhaMasNaoOLogin()
    {
        var usuario = ComUsuario(2);
        var hashAntigo = usuario.HashSenha;

        await _appService.AtualizarAsync(2, new SalvarUsuarioDto { Login = "tatiane", Nome = "Tati", Senha = "nova-senha-1" });
        var trocarLogin = () => _appService.AtualizarAsync(2, new SalvarUsuarioDto { Login = "outra", Nome = "Tati" });

        usuario.Nome.Should().Be("Tati");
        usuario.HashSenha.Should().NotBe(hashAntigo);
        await trocarLogin.Should().ThrowAsync<DomainException>().WithMessage("O login não pode*");
    }

    [Fact]
    public async Task Atualizar_SemSenha_MantemAAtual()
    {
        var usuario = ComUsuario(2);
        var hashAntigo = usuario.HashSenha;

        await _appService.AtualizarAsync(2, new SalvarUsuarioDto { Login = "tatiane", Nome = "Tati" });

        usuario.HashSenha.Should().Be(hashAntigo);
    }

    [Fact]
    public async Task DesativarOuExcluirOProprioUsuario_LancaDomainException()
    {
        ComUsuario(1, "suellen");

        var desativar = () => _appService.AlterarSituacaoAsync(1, false);
        var excluir = () => _appService.ExcluirAsync(1);

        await desativar.Should().ThrowAsync<DomainException>().WithMessage("*próprio usuário*");
        await excluir.Should().ThrowAsync<DomainException>().WithMessage("*próprio usuário*");
    }

    [Fact]
    public async Task DesativarOUltimoAtivo_LancaDomainException()
    {
        ComUsuario(2);
        _mocker.GetMock<IUsuarioRepository>()
            .Setup(r => r.ContarAtivosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var desativar = () => _appService.AlterarSituacaoAsync(2, false);

        await desativar.Should().ThrowAsync<DomainException>().WithMessage("*pelo menos um*");
    }

    [Fact]
    public async Task DesativarReativarEExcluirOutroUsuario()
    {
        var usuario = ComUsuario(2);

        await _appService.AlterarSituacaoAsync(2, false);
        usuario.Estado.Should().Be(EntityStatus.Inativo);
        await _appService.AlterarSituacaoAsync(2, true);
        usuario.Estado.Should().Be(EntityStatus.Ativo);
        await _appService.ExcluirAsync(2);
        usuario.Estado.Should().Be(EntityStatus.Arquivado);
    }

    [Fact]
    public async Task UsuarioInexistente_LancaObjectNotFound()
    {
        var excluir = () => _appService.ExcluirAsync(404);

        await excluir.Should().ThrowAsync<ObjectNotFoundException>();
    }
}

public class LancamentoAppServiceTests
{
    private readonly AutoMocker _mocker = new();
    private readonly LancamentoAppService _appService;

    public LancamentoAppServiceTests()
    {
        _mocker.Use<IMapper>(DadosDeTeste.CriarMapper());
        _mocker.GetMock<IAuthentication>().Setup(a => a.Login).Returns("tatiane");
        _appService = _mocker.CreateInstance<LancamentoAppService>();
    }

    [Fact]
    public async Task CriarAtualizarEExcluir()
    {
        var dto = new SalvarLancamentoDto
        {
            Tipo = TipoLancamento.Despesa,
            Descricao = "Leite condensado",
            Valor = 64.9m,
            Data = new DateOnly(2026, 11, 3)
        };

        var criado = await _appService.CriarAsync(dto);
        criado.CadastradoPor.Should().Be("tatiane");

        var existente = DadosDeTeste.Lancamento(TipoLancamento.Despesa, 10m, new DateOnly(2026, 11, 1));
        _mocker.GetMock<ILancamentoRepository>()
            .Setup(r => r.ObterPorIdAsync(4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existente);

        (await _appService.AtualizarAsync(4, dto)).Valor.Should().Be(64.9m);
        await _appService.ExcluirAsync(4);
        existente.Estado.Should().Be(EntityStatus.Arquivado);
    }

    [Fact]
    public async Task ListarComPeriodoInvertidoEExcluirInexistente_LancamExcecao()
    {
        var listar = () => _appService.ListarAsync(new FiltroLancamentoDto
        {
            Inicio = new DateOnly(2026, 11, 5),
            Fim = new DateOnly(2026, 11, 1)
        });
        var excluir = () => _appService.ExcluirAsync(404);

        await listar.Should().ThrowAsync<DomainException>();
        await excluir.Should().ThrowAsync<ObjectNotFoundException>();
    }
}

public class PainelAppServiceTests
{
    private static readonly DateTimeOffset Agora = new(2026, 11, 13, 10, 0, 0, TimeSpan.FromHours(-3));

    private readonly AutoMocker _mocker = new();
    private readonly PainelAppService _appService;
    private readonly List<Pedido> _pedidos = new();
    private readonly List<Lancamento> _lancamentos = new();

    public PainelAppServiceTests()
    {
        _mocker.Use<TimeProvider>(new TempoFixo(Agora));
        _mocker.GetMock<IPedidoRepository>()
            .Setup(r => r.ListarPorPeriodoAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_pedidos);
        _mocker.GetMock<ILancamentoRepository>()
            .Setup(r => r.ListarAsync(It.IsAny<FiltroLancamentoDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_lancamentos);
        _appService = _mocker.CreateInstance<PainelAppService>();
    }

    private void Pedido(StatusPedido status, int dia, params (Produto Produto, int Quantidade)[] itens)
    {
        var pedido = DadosDeTeste.Pedido(data: new DateOnly(2026, 11, dia), codigo: $"EI-{_pedidos.Count:000000}");
        foreach (var (produto, quantidade) in itens)
            pedido.AdicionarItem(produto, quantidade);
        pedido.AlterarStatus(status);
        _pedidos.Add(pedido);
    }

    [Fact]
    public async Task ObterAsync_SemDatas_UsaOMesCorrente()
    {
        var painel = await _appService.ObterAsync(null, null);

        painel.Inicio.Should().Be(new DateOnly(2026, 11, 1));
        painel.Fim.Should().Be(new DateOnly(2026, 11, 30));
        painel.Agrupamento.Should().Be("Dia");
        painel.Serie.Should().HaveCount(30);
        painel.Margem.Should().BeNull();
        painel.TicketMedio.Should().Be(0);
    }

    [Fact]
    public async Task ObterAsync_CalculaReceitaCustosLucroEMargem()
    {
        var bolo = DadosDeTeste.Produto(1, preco: 100m, custo: 40m, nome: "Bolo");
        var brigadeiro = DadosDeTeste.Produto(2, preco: 2m, custo: 0.5m, nome: "Brigadeiro");
        Pedido(StatusPedido.Entregue, 3, (bolo, 1), (brigadeiro, 50));   // 200 de receita, 65 de custo
        Pedido(StatusPedido.Confirmado, 3, (bolo, 1));                  // 100 de receita, 40 de custo
        Pedido(StatusPedido.Recebido, 5, (bolo, 2));                    // aguardando: fora da receita
        Pedido(StatusPedido.Cancelado, 6, (brigadeiro, 30));            // cancelado: fora da receita
        _lancamentos.Add(DadosDeTeste.Lancamento(TipoLancamento.Despesa, 30m, new DateOnly(2026, 11, 3)));
        _lancamentos.Add(DadosDeTeste.Lancamento(TipoLancamento.Perda, 15m, new DateOnly(2026, 11, 10)));

        var painel = await _appService.ObterAsync(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));

        painel.Receita.Should().Be(300m);
        painel.CustoProdutos.Should().Be(105m);
        painel.Despesas.Should().Be(30m);
        painel.Perdas.Should().Be(15m);
        painel.Lucro.Should().Be(150m);
        painel.Margem.Should().Be(50m);
        painel.QuantidadeVendas.Should().Be(2);
        painel.TicketMedio.Should().Be(150m);
        painel.AguardandoConfirmacao.Should().BeEquivalentTo(new { Quantidade = 1, Valor = 200m });
        painel.Cancelados.Should().BeEquivalentTo(new { Quantidade = 1, Valor = 60m });
        painel.PorStatus.Should().HaveCount(6).And.Contain(s => s.Status == StatusPedido.Entregue && s.Quantidade == 1);

        var dia3 = painel.Serie.Single(p => p.Data == new DateOnly(2026, 11, 3));
        dia3.Receita.Should().Be(300m);
        dia3.Custos.Should().Be(135m);
        dia3.Lucro.Should().Be(165m);
        painel.Serie.Single(p => p.Data == new DateOnly(2026, 11, 10)).Lucro.Should().Be(-15m);

        painel.ProdutosMaisVendidos.Select(p => p.Nome).Should().Equal("Bolo", "Brigadeiro");
        painel.ProdutosMaisVendidos[0].Lucro.Should().Be(120m);
        painel.ProdutosMaisVendidos[1].Quantidade.Should().Be(50);
    }

    [Fact]
    public async Task ObterAsync_PeriodoLongo_AgrupaPorMes()
    {
        Pedido(StatusPedido.Entregue, 20, (DadosDeTeste.Produto(1, preco: 10m, custo: 0m), 1));

        var painel = await _appService.ObterAsync(new DateOnly(2026, 9, 15), new DateOnly(2026, 12, 10));

        painel.Agrupamento.Should().Be("Mes");
        painel.Serie.Select(p => p.Data).Should().Equal(
            new DateOnly(2026, 9, 1), new DateOnly(2026, 10, 1), new DateOnly(2026, 11, 1), new DateOnly(2026, 12, 1));
        painel.Serie[2].Receita.Should().Be(10m);
    }

    [Fact]
    public async Task ObterAsync_PeriodoInvalido_LancaDomainException()
    {
        var invertido = () => _appService.ObterAsync(new DateOnly(2026, 11, 10), new DateOnly(2026, 11, 1));
        var longoDemais = () => _appService.ObterAsync(new DateOnly(2020, 1, 1), new DateOnly(2026, 1, 1));

        await invertido.Should().ThrowAsync<DomainException>();
        await longoDemais.Should().ThrowAsync<DomainException>().WithMessage("*3 anos*");
    }
}

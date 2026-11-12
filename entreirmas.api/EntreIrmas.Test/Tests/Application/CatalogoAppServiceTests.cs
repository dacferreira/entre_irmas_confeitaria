using EntreIrmas.Application.AppServices;
using EntreIrmas.Domain.DTOs.Produto;
using EntreIrmas.Domain.Entities.ProdutoAggregate;
using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Domain.Repositories;
using EntreIrmas.Test.Config;
using FluentAssertions;
using MapsterMapper;
using Moq;
using Moq.AutoMock;

namespace EntreIrmas.Test.Tests.Application;

public class CatalogoAppServiceTests
{
    private readonly AutoMocker _mocker = new();

    public CatalogoAppServiceTests()
    {
        _mocker.Use<IMapper>(DadosDeTeste.CriarMapper());
    }

    [Fact]
    public async Task ProdutoObterPorSlug_Inexistente_LancaObjectNotFound()
    {
        var appService = _mocker.CreateInstance<ProdutoAppService>();

        var obter = () => appService.ObterPorSlugAsync("nao-existe");

        await obter.Should().ThrowAsync<ObjectNotFoundException>();
    }

    [Fact]
    public async Task ProdutoObterPorSlug_NormalizaOSlug()
    {
        _mocker.GetMock<IProdutoRepository>()
            .Setup(r => r.ObterPorSlugAsync("brownie", It.IsAny<CancellationToken>()))
            .ReturnsAsync(DadosDeTeste.Produto(7));
        var appService = _mocker.CreateInstance<ProdutoAppService>();

        var produto = await appService.ObterPorSlugAsync(" Brownie ");

        produto.Id.Should().Be(7);
        produto.Nome.Should().Be("Brownie");
    }

    [Fact]
    public async Task ProdutoListar_RepassaFiltroEMapeia()
    {
        var filtro = new FiltroProdutoDto { Categoria = "bolos" };
        _mocker.GetMock<IProdutoRepository>()
            .Setup(r => r.ListarAsync(filtro, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Produto> { DadosDeTeste.Produto(1), DadosDeTeste.Produto(2, nome: "Fudge") });
        var appService = _mocker.CreateInstance<ProdutoAppService>();

        var produtos = await appService.ListarAsync(filtro);

        produtos.Select(p => p.Nome).Should().Equal("Brownie", "Fudge");
    }
}

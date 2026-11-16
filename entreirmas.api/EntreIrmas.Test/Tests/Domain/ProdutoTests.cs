using EntreIrmas.Domain.Entities.CategoriaAggregate;
using EntreIrmas.Domain.Entities.ProdutoAggregate;
using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Domain.Seedwork;
using FluentAssertions;

namespace EntreIrmas.Test.Tests.Domain;

public class ProdutoTests
{
    [Fact]
    public void Criar_NormalizaSlugETextos()
    {
        var produto = new Produto(1, "  Brownie  ", " Brownie-De-Nutella ", " Macio ", 12m, 4m, " unidade ", 1, " brownie ", true, false, "T");

        produto.Nome.Should().Be("Brownie");
        produto.Slug.Should().Be("brownie-de-nutella");
        produto.Unidade.Should().Be("unidade");
        produto.Custo.Should().Be(4m);
        produto.UrlImagem.Should().BeNull();
        produto.Estado.Should().Be(EntityStatus.Ativo);
    }

    [Theory]
    [InlineData(0, 0, 1, 1)]
    [InlineData(-1, 0, 1, 1)]
    [InlineData(10, 0, 0, 1)]
    [InlineData(10, -1, 1, 1)]
    [InlineData(10, 0, 1, 0)]
    public void Criar_ComDadosInvalidos_LancaDomainException(decimal preco, decimal custo, int minimo, long categoriaId)
    {
        var criar = () => new Produto(categoriaId, "Brownie", "brownie", "", preco, custo, "unidade", minimo, "brownie", false, false, "T");

        criar.Should().Throw<DomainException>();
    }

    [Fact]
    public void Atualizar_TrocaOsDadosEMantemOCadastro()
    {
        var produto = new Produto(1, "Brownie", "brownie", "Macio", 12m, 4m, "unidade", 1, "brownie", false, false, "T");

        produto.Atualizar(2, "Fudge", "fudge", "Cremoso", 18m, 7m, "pote", 2, "fudge", true, true);

        produto.CategoriaId.Should().Be(2);
        produto.Nome.Should().Be("Fudge");
        produto.Preco.Should().Be(18m);
        produto.Custo.Should().Be(7m);
        produto.Destaque.Should().BeTrue();
        produto.CadastradoPor.Should().Be("T");
    }

    [Fact]
    public void Excluir_ArquivaELiberaOSlug()
    {
        var produto = new Produto(1, "Brownie", "brownie", "Macio", 12m, 4m, "unidade", 1, "brownie", false, false, "T");
        Config.DadosDeTeste.DefinirId(produto, 7);

        produto.Excluir();

        produto.Estado.Should().Be(EntityStatus.Arquivado);
        produto.Slug.Should().Be("brownie~7");
    }

    [Fact]
    public void CategoriaExcluir_ComProdutos_LancaDomainException()
    {
        var categoria = new Categoria("Bolos", "bolos", null, 1, "T");
        var produtos = (List<Produto>)typeof(Categoria)
            .GetField("_produtos", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(categoria)!;
        produtos.Add(new Produto(1, "Bolo", "bolo", "x", 10m, 0m, "unidade", 1, "bolo", false, false, "T"));

        var excluir = () => categoria.Excluir();

        excluir.Should().Throw<DomainException>().WithMessage("*1 produto(s)*");
    }

    [Fact]
    public void CategoriaExcluir_SemProdutos_ArquivaELiberaOSlug()
    {
        var categoria = new Categoria("Bolos", "bolos", " ", 1, "T");
        Config.DadosDeTeste.DefinirId(categoria, 3);

        categoria.Excluir();

        categoria.Descricao.Should().BeNull();
        categoria.Estado.Should().Be(EntityStatus.Arquivado);
        categoria.Slug.Should().Be("bolos~3");
    }

    [Theory]
    [InlineData("Bolo de Pote!", "bolo-de-pote")]
    [InlineData("  Pão de Mel com Açúcar  ", "pao-de-mel-com-acucar")]
    [InlineData("Brownie & Fudge", "brownie-fudge")]
    [InlineData("!!!", "")]
    [InlineData(null, "")]
    public void Slug_Gerar_RemoveAcentosESimbolos(string? texto, string esperado)
    {
        Slug.Gerar(texto).Should().Be(esperado);
    }

    [Fact]
    public void Entity_AtivarInativarArquivar_AlteraEstado()
    {
        var categoria = new Categoria("Bolos", "Bolos", null, 1, "");

        categoria.CadastradoPor.Should().Be("SISTEMA");
        categoria.Slug.Should().Be("bolos");

        categoria.Inativar();
        categoria.Estado.Should().Be(EntityStatus.Inativo);
        categoria.Arquivar();
        categoria.Estado.Should().Be(EntityStatus.Arquivado);
        categoria.Ativar();
        categoria.Estado.Should().Be(EntityStatus.Ativo);
    }

    [Fact]
    public void Categoria_SemNome_LancaDomainException()
    {
        var criar = () => new Categoria(" ", "bolos", null, 1, "T");

        criar.Should().Throw<DomainException>();
    }
}

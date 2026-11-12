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
        var produto = new Produto(1, "  Brownie  ", " Brownie-De-Nutella ", " Macio ", 12m, " unidade ", 1, " brownie ", null, true, "T");

        produto.Nome.Should().Be("Brownie");
        produto.Slug.Should().Be("brownie-de-nutella");
        produto.Unidade.Should().Be("unidade");
        produto.Estado.Should().Be(EntityStatus.Ativo);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(10, 0)]
    public void Criar_ComPrecoOuMinimoInvalido_LancaDomainException(decimal preco, int minimo)
    {
        var criar = () => new Produto(1, "Brownie", "brownie", "", preco, "unidade", minimo, "brownie", null, false, "T");

        criar.Should().Throw<DomainException>();
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

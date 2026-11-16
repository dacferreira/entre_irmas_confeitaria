using EntreIrmas.Domain.Entities.ProdutoAggregate;
using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Test.Config;
using FluentAssertions;

namespace EntreIrmas.Test.Tests.Domain;

public class ImagemProdutoTests
{
    [Theory]
    [MemberData(nameof(Assinaturas))]
    public void Criar_IdentificaOTipoPelosBytes(byte[] conteudo, string tipo)
    {
        var imagem = new ImagemProduto(10, conteudo, "suellen");

        imagem.TipoConteudo.Should().Be(tipo);
        imagem.ProdutoId.Should().Be(10);
        imagem.CadastradoPor.Should().Be("suellen");
    }

    public static TheoryData<byte[], string> Assinaturas => new()
    {
        { DadosDeTeste.ImagemJpeg(), "image/jpeg" },
        { DadosDeTeste.ImagemPng(), "image/png" },
        { DadosDeTeste.ImagemWebp(), "image/webp" },
    };

    [Fact]
    public void Criar_ArquivoQueNaoEFoto_LancaDomainException()
    {
        // Um texto renomeado para .jpg continua sendo texto.
        var criar = () => new ImagemProduto(10, "isto nao e uma foto"u8.ToArray(), "T");

        criar.Should().Throw<DomainException>().WithMessage("*JPG, PNG ou WEBP*");
    }

    [Fact]
    public void Criar_VaziaOuGrandeDemais_LancaDomainException()
    {
        var vazia = () => new ImagemProduto(10, [], "T");
        var grande = new byte[ImagemProduto.TamanhoMaximo + 1];
        DadosDeTeste.ImagemPng().CopyTo(grande, 0);
        var grandeDemais = () => new ImagemProduto(10, grande, "T");

        vazia.Should().Throw<DomainException>().WithMessage("Escolha uma foto.");
        grandeDemais.Should().Throw<DomainException>().WithMessage("*2 MB*");
    }

    [Fact]
    public void Substituir_TrocaConteudoETipo()
    {
        var imagem = new ImagemProduto(10, DadosDeTeste.ImagemJpeg(), "T");

        imagem.Substituir(DadosDeTeste.ImagemWebp());

        imagem.TipoConteudo.Should().Be("image/webp");
        imagem.Conteudo.Should().Equal(DadosDeTeste.ImagemWebp());
    }

    [Fact]
    public void ProdutoDefinirImagem_MontaOCaminhoComVersaoEVoltaParaIlustracao()
    {
        var produto = DadosDeTeste.Produto(7);

        produto.DefinirImagem(638_999);
        produto.UrlImagem.Should().Be("produtos/7/imagem?v=638999");

        produto.DefinirImagem(null);
        produto.UrlImagem.Should().BeNull();
    }
}

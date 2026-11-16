using EntreIrmas.Domain.Entities.PedidoAggregate;
using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Test.Config;
using FluentAssertions;

namespace EntreIrmas.Test.Tests.Domain;

public class PedidoTests
{
    [Fact]
    public void Criar_ComRetirada_DescartaEnderecoEIniciaComoRecebido()
    {
        var pedido = DadosDeTeste.Pedido(TipoEntrega.Retirada, endereco: DadosDeTeste.Endereco(), observacao: "  ");

        pedido.Endereco.Should().BeNull();
        pedido.EnderecoEntrega.Should().BeNull();
        pedido.Observacao.Should().BeNull();
        pedido.Status.Should().Be(StatusPedido.Recebido);
        pedido.CadastradoPor.Should().Be("TESTE");
    }

    [Fact]
    public void Criar_EntregaSemEndereco_LancaDomainException()
    {
        var criar = () => DadosDeTeste.Pedido(TipoEntrega.Entrega, endereco: null);

        criar.Should().Throw<DomainException>().WithMessage("*endereço*");
    }

    [Fact]
    public void Criar_ComEntrega_GuardaAsPartesEOEnderecoEmUmaLinha()
    {
        var pedido = DadosDeTeste.Pedido(TipoEntrega.Entrega, endereco: DadosDeTeste.Endereco());

        pedido.EnderecoEntrega!.Cep.Should().Be("20000000");
        pedido.EnderecoEntrega.Uf.Should().Be("RJ");
        pedido.Endereco.Should().Be("Rua das Flores, 10 - Apto 2 - Centro, Rio de Janeiro/RJ - CEP 20000-000");
    }

    [Fact]
    public void Endereco_SemComplemento_NaoDeixaTracoSobrando()
    {
        DadosDeTeste.Endereco(complemento: " ").Formatar()
            .Should().Be("Rua das Flores, 10 - Centro, Rio de Janeiro/RJ - CEP 20000-000");
        DadosDeTeste.Endereco().Should().Be(DadosDeTeste.Endereco());
    }

    [Theory]
    [InlineData("2000000", "Rua", "10", "Centro", "Rio", "RJ")]
    [InlineData("20000000", " ", "10", "Centro", "Rio", "RJ")]
    [InlineData("20000000", "Rua", "", "Centro", "Rio", "RJ")]
    [InlineData("20000000", "Rua", "10", "", "Rio", "RJ")]
    [InlineData("20000000", "Rua", "10", "Centro", " ", "RJ")]
    [InlineData("20000000", "Rua", "10", "Centro", "Rio", "R")]
    public void Endereco_Incompleto_LancaDomainException(string cep, string rua, string numero, string bairro,
        string cidade, string uf)
    {
        var criar = () => new EnderecoEntrega(cep, rua, numero, null, bairro, cidade, uf);

        criar.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("", "Maria", "21999998888")]
    [InlineData("EI-1", "", "21999998888")]
    [InlineData("EI-1", "Maria", "")]
    public void Criar_SemDadoObrigatorio_LancaDomainException(string codigo, string nome, string telefone)
    {
        var criar = () => new Pedido(codigo, nome, telefone, TipoEntrega.Retirada, null, new DateOnly(2026, 12, 1), null, "T");

        criar.Should().Throw<DomainException>();
    }

    [Fact]
    public void AdicionarItem_CalculaTotalComPrecoDoProduto()
    {
        var pedido = DadosDeTeste.Pedido();

        pedido.AdicionarItem(DadosDeTeste.Produto(1, preco: 12m), 2);
        pedido.AdicionarItem(DadosDeTeste.Produto(2, preco: 2.5m, nome: "Brigadeiro"), 30);

        pedido.Itens.Should().HaveCount(2);
        pedido.ValorTotal.Should().Be(99m);
    }

    [Fact]
    public void AdicionarItem_MesmoProduto_SomaQuantidade()
    {
        var pedido = DadosDeTeste.Pedido();
        var produto = DadosDeTeste.Produto(1, preco: 10m);

        pedido.AdicionarItem(produto, 1);
        pedido.AdicionarItem(produto, 2);

        pedido.Itens.Should().ContainSingle().Which.Quantidade.Should().Be(3);
        pedido.ValorTotal.Should().Be(30m);
    }

    [Fact]
    public void AdicionarItem_AbaixoDoPedidoMinimo_LancaDomainException()
    {
        var pedido = DadosDeTeste.Pedido();
        var brigadeiro = DadosDeTeste.Produto(1, preco: 2.5m, quantidadeMinima: 25, nome: "Brigadeiro");

        var adicionar = () => pedido.AdicionarItem(brigadeiro, 10);

        adicionar.Should().Throw<DomainException>().WithMessage("*mínimo de Brigadeiro é de 25*");
    }

    [Fact]
    public void ValidarParaEnvio_SemItens_LancaDomainException()
    {
        var validar = () => DadosDeTeste.Pedido().ValidarParaEnvio();

        validar.Should().Throw<DomainException>();
    }
}

using EntreIrmas.Domain.DTOs.Pedido;
using EntreIrmas.Domain.Entities.PedidoAggregate;
using EntreIrmas.Domain.Validations;
using FluentAssertions;

namespace EntreIrmas.Test.Tests.Domain;

public class CriarPedidoValidationTests
{
    private readonly CriarPedidoValidation _validacao = new();

    private static CriarPedidoDto PedidoValido() => new()
    {
        NomeCliente = "Maria",
        Telefone = "(21) 99999-8888",
        TipoEntrega = TipoEntrega.Retirada,
        DataDesejada = new DateOnly(2026, 12, 20),
        Itens = [new ItemCriarPedidoDto { ProdutoId = 1, Quantidade = 2 }]
    };

    [Fact]
    public void Validar_PedidoValido_NaoTemErros()
    {
        _validacao.Validate(PedidoValido()).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("2199999")]
    [InlineData("219999988887")]
    public void Validar_TelefoneComTamanhoErrado_TemErro(string telefone)
    {
        var dto = PedidoValido();
        dto.Telefone = telefone;

        _validacao.Validate(dto).Errors.Should().ContainSingle(e => e.PropertyName == nameof(CriarPedidoDto.Telefone));
    }

    [Fact]
    public void Validar_EntregaSemEndereco_TemErro()
    {
        var dto = PedidoValido();
        dto.TipoEntrega = TipoEntrega.Entrega;

        _validacao.Validate(dto).Errors.Should().Contain(e => e.PropertyName == nameof(CriarPedidoDto.Endereco));
    }

    [Fact]
    public void Validar_SemItensESemData_TemErros()
    {
        var dto = PedidoValido();
        dto.Itens.Clear();
        dto.DataDesejada = default;

        var erros = _validacao.Validate(dto).Errors;

        erros.Should().Contain(e => e.PropertyName == nameof(CriarPedidoDto.Itens));
        erros.Should().Contain(e => e.PropertyName == nameof(CriarPedidoDto.DataDesejada));
    }

    [Fact]
    public void Validar_QuantidadeForaDoLimite_TemErro()
    {
        var dto = PedidoValido();
        dto.Itens[0].Quantidade = CriarPedidoValidation.QuantidadeMaximaPorItem + 1;

        _validacao.Validate(dto).Errors.Should().Contain(e => e.PropertyName == "Itens[0].Quantidade");
    }

    [Fact]
    public void SomenteDigitos_RemoveMascara()
    {
        CriarPedidoValidation.SomenteDigitos("(21) 99999-8888").Should().Be("21999998888");
        CriarPedidoValidation.SomenteDigitos(null).Should().BeEmpty();
    }
}

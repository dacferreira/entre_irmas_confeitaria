using EntreIrmas.Domain.Entities.LojaAggregate;
using EntreIrmas.Domain.Exceptions;
using FluentAssertions;

namespace EntreIrmas.Test.Tests.Domain;

public class LojaTests
{
    [Fact]
    public void NovaLoja_ComecaAbertaComIdUnico()
    {
        var loja = new Loja("TESTE");

        loja.Id.Should().Be(Loja.IdUnico);
        loja.Aberta.Should().BeTrue();
        loja.MensagemFechada.Should().BeNull();
        loja.AlteradoPor.Should().Be("TESTE");
    }

    [Fact]
    public void Fechar_GuardaAMensagemAparadaEQuemFechou_EAbrirLimpaAMensagem()
    {
        var loja = new Loja("TESTE");

        loja.Fechar("  Voltamos segunda às 9h  ", "suellen");

        loja.Aberta.Should().BeFalse();
        loja.MensagemFechada.Should().Be("Voltamos segunda às 9h");
        loja.AlteradoPor.Should().Be("suellen");

        loja.Abrir("tatiane");

        loja.Aberta.Should().BeTrue();
        loja.MensagemFechada.Should().BeNull();
        loja.AlteradoPor.Should().Be("tatiane");
    }

    [Fact]
    public void Fechar_SemMensagem_FicaNula()
    {
        var loja = new Loja("TESTE");

        loja.Fechar("   ", "suellen");

        loja.MensagemFechada.Should().BeNull();
    }

    [Fact]
    public void Fechar_MensagemLonga_LancaDomainException()
    {
        var fechar = () => new Loja("TESTE").Fechar(new string('a', Loja.TamanhoMaximoDaMensagem + 1), "suellen");

        fechar.Should().Throw<DomainException>();
    }
}
